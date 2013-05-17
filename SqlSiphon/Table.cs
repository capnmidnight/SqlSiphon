using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SqlSiphon
{
    [Serializable]
    public class Table
    {
        public static string DefaultSchema;
        static Table()
        {
            DefaultSchema = "dbo";
        }


        public bool IsHistoryTracked { get { return isHistoryTracked; } }
        public string PreAddConstraintScript { get { return preAddConstraintScript; } }
        public string PostAddConstraintScript { get { return postAddConstraintScript; } }

        public readonly string Name;
        public readonly string Schema;
        public readonly Type originalType;
        public readonly bool IsEnumeration;
        public readonly bool IncludeInSynch;
        private List<Column> Columns;
        public List<Column> Constraints;
        private bool isHistoryTracked;
        private string preAddConstraintScript;
        private string postAddConstraintScript;

        public string PrimaryKeyText
        {
            get
            {
                return string.Join(
                    ", ",
                    this.KeyColumns
                        .Select(c => c.Name)
                        .ToArray());
            }
        }

        public List<Column> KeyColumns
        {
            get
            {
                return this.GetAllColumns()
                    .Where(c => c.IsPrimaryKey)
                    .ToList();
            }
        }

        public List<Column> SettableColumns
        {
            get
            {
                return this.GetAllColumns()
                    .Where(c => c.IncludeInInsert)
                    .ToList();
            }
        }

        public string FullName
        {
            get
            {
                return this.Schema + "." + this.Name;
            }
        }

        public string KeyName
        {
            get
            {
                return this.Schema + "_" + this.Name;
            }
        }

        public Table(Type type)
            :this()
        {
            this.originalType = type;
            var tableHints = (MappedClassAttribute)type.GetCustomAttributes(typeof(MappedClassAttribute), true).FirstOrDefault();
            if (tableHints == null)
                tableHints = new MappedClassAttribute
                {
                    Name = type.Name
                };

            this.Schema = tableHints.Schema ?? DefaultSchema;
            this.Name = tableHints.Name ?? type.Name;
            this.Columns = new List<Column>();
            this.Constraints = new List<Column>();
            this.IncludeInSynch = tableHints.IncludeInSynch;
            this.IsEnumeration = type.IsEnum;
            if (this.IsEnumeration)
            {
                AnalyzeEnumeration(type);
            }
            else
            {
                AnalyzeClass(type, tableHints);
            }
        }
        private Table()
        {
            this.getAllColumns = new Lazy<List<Column>>(() => Columns
                .Union(Constraints
                    .SelectMany(c => SqlClientQueryGenerator.Tables[c.ForeignKeyTarget]
                        .KeyColumns
                        .Select(f => new Column(c, f))))
                .ToList());
        }
        public Table(Table table)
            :this()
        {
            this.originalType = table.originalType;
            this.Schema = table.Schema + "_history";
            this.Name = table.Name;
            this.isHistoryTracked = false;
            this.IncludeInSynch = table.IncludeInSynch;
            this.Columns = new List<Column>(table.GetAllColumns().Select(c => new Column(c)));
            this.Constraints = new List<Column>();
        }

        public List<string[]> GetSelectedColumns(int depth = 10, string tableNamePrefix = "")
        {
            var output = new List<string[]>();
            if (depth >= 0)
            {
                output.AddRange(this.GetAllColumns()
                 .Select(c => new string[] { tableNamePrefix, c.Name })
                 .Union(Constraints
                     .Where(c => !c.IsEnumerationValue)
                     .SelectMany(c => SqlClientQueryGenerator.Tables[c.ForeignKeyTarget].GetSelectedColumns(depth - 1, tableNamePrefix + c.Name + "."))));
            }
            return output;
        }
        public string FromQuery(int depth = 10)
        {
            var join = GetJoinStatement(depth).ToArray();
            return "from " 
                + this.FullName 
                + Environment.NewLine 
                + string.Join(Environment.NewLine, join);
        }

        public string SelectQuery(string whereClause = "", int depth = 10)
        {
            return string.Format(@"select 
{0}
{1}
{2};", string.Join("," + Environment.NewLine,
                GetSelectedColumns(depth)
                .Select(pair => pair[0].Length == 0
                    ? this.Name + "." + pair[1]
                    : string.Format("[{0}].[{1}] as [{0}.{1}]", pair[0].Substring(0, pair[0].Length - 1), pair[1]))
                .ToArray()),
                FromQuery(depth),
                whereClause.Length > 0 ? "where " + whereClause : "");
        }

        public string UpdateQuery<T>(T obj, string whereClause = "", int depth = 10)
        {
            return string.Format(@"update {0} set 
{1}
{2}
{3};",
                this.FullName,
                obj is string
                    ? obj.ToString()
                    : string.Join("," + Environment.NewLine,
                        (from c in this.SettableColumns
                         let val = c.Get(obj)
                         select string.Format("{0}.{1} = {2}{3}{2}",
                                 this.Name,
                                 c.Name,
                                 val == null ? "" : "'",
                                 val == null ? "NULL" : val.ToString())).ToArray()),
                FromQuery(depth),
                whereClause.Length > 0 ? "where " + whereClause : "");
        }

        public string DeleteQuery(string whereClause = "", int depth = 10)
        {
            return string.Format(@"delete from {0}
{1}
{2};",
                this.FullName,
                FromQuery(depth),
                whereClause.Length > 0 ? "where " + whereClause : "");
        }

        public string InsertHeader()
        {
            return string.Format("insert into {0}({1}) values",
                this.FullName,
                string.Join(", ", this.SettableColumns
                    .Select(c => c.Name).ToArray()));
        }

        public string InsertQuery<T>(T obj)
        {
            return string.Format(
@"{0}
{1};",
                this.InsertHeader(),
                ValueClause(obj));
        }

        public string ValueClause<T>(T obj)
        {
            var output = new List<string>();
            foreach (var col in this.SettableColumns)
            {
                var val = col.Get(obj);
                if (val != null)
                    output.Add(string.Format("'{0}'", val.ToString().Replace("'", "''")));
                else
                    output.Add("NULL");
            }
            var clause = string.Format("({0})", string.Join(", ", output.ToArray()));
            return clause;
        }

        public List<string> GetJoinStatement(int depth = 10, List<string> output = null, string tableNamePrefix = "", bool outerJoin = false)
        {
            if(output == null)
                output = new List<string>();

            if (depth > 0)
            {
                foreach (var constraint in Constraints.Where(c => !c.IsEnumerationValue))
                {
                    output.Add(string.Format("{0} join {1} as [{2}{3}] on",
                        outerJoin || constraint.IsNullable ? "left outer" : "inner",
                        constraint.ForeignKeyTarget.Name,
                        tableNamePrefix,
                        constraint.Name));
                    var temp = SqlClientQueryGenerator.Tables[constraint.ForeignKeyTarget]
                        .KeyColumns
                        .Select(c => string.Format(tableNamePrefix.Length > 0 ? "[{0}].{1}{2} = [{0}.{4}].{2}" : "{3}.{1}{2} = [{4}].{2}",
                                    tableNamePrefix.Length > 0 ? tableNamePrefix.Substring(0, tableNamePrefix.Length - 1) : null,
                                    constraint.PrefixName ? constraint.Name : "",
                                    c.Name,
                                    this.Name,
                                    constraint.Name)).ToArray();
                    output.Add(string.Join(Environment.NewLine + "and ", temp));
                    SqlClientQueryGenerator.Tables[constraint.ForeignKeyTarget].GetJoinStatement(
                        depth - 1,
                        output,
                        tableNamePrefix + constraint.Name + ".",
                        outerJoin || constraint.IsNullable);
                }
            }
            return output;
        }

        private void AnalyzeClass(Type type, MappedClassAttribute tableHints)
        {
            this.isHistoryTracked = tableHints.IsHistoryTracked;// && !this.isView;
            foreach (var property in type.GetProperties())
            {
                var col = new Column(property);
                if (!col.Ignore)
                {
                    if (MappedTypeAttribute.SqlTypes.ContainsKey(property.PropertyType))
                        this.Columns.Add(col);
                    else
                        this.Constraints.Add(col);
                }
            }
            //if (!this.isView)
            this.preAddConstraintScript = tableHints.PreAddConstraintScript;
            this.postAddConstraintScript = tableHints.PostAddConstraintScript;
        }

        private void AnalyzeEnumeration(Type type)
        {
            var values = string.Join(
                    "," + Environment.NewLine,
                    type.GetFields(BindingFlags.Public | BindingFlags.Static)
                        .Select(f => string.Format("('{0}')",
                            f.Name))
                        .ToArray());
            if (values.Length > 0)
            {
                this.Columns.Add(new Column("Value", "nvarchar(256)", type.GetEnumName(0), true));
                this.preAddConstraintScript = string.Format(@"insert into {0} (Value) values {1};", this.FullName, values);
            }
        }

        public string TrackingTrigger
        {
            get
            {
                var cols = this.GetAllColumns();
                if (cols.Count > 0)
                {
                    var columnNames = string.Join(", ", cols.Select(c => c.Name).ToArray());
                    return string.Format(@"create trigger {0}_Tracker on {0}
after update, delete as
	insert into {2}_history.{3}
	({1})
	select {1}
	from deleted;",
                               this.FullName,
                               columnNames,
                               this.Schema,
                               this.Name);
                }
                return "";
            }
        }

        public Table MakeTrackingTable()
        {
            var table = new Table(this);
            if (table.Columns.Count > 0)
            {
                foreach (var column in table.Columns)
                {
                    column.IsPrimaryKey = false;
                    column.IsIdentity = false;
                }
                table.Columns.Add(new Column("history_ID", "int", null, true, true));
                table.Columns.Add(new Column("DeactivatedOn", "DateTime2", "GETDATE()"));
            }
            return table;
        }

        public string GetCreateTableText()
        {
            var cols = this.GetAllColumns();
            var output = cols.Select(c => c.InTableDefSql).ToList();
            if (this.KeyColumns.Count > 0)
                output.Add(string.Format("constraint PK_{0} primary key({1})",
                    this.KeyName,
                    this.PrimaryKeyText));

            var drop = string.Format(@"if exists(select * from information_schema.tables where table_name = '{1}' and table_schema = '{2}')
begin 
    if not exists(select * from information_schema.tables where table_name = '{1}' and table_schema = '{2}_backup') select * into {2}_backup.{1} from {0};
    drop table {0};
end",
                this.FullName,
                this.Name,
                this.Schema);
            string create = "";
            if (cols.Count > 0)
            {
                create = string.Format(@"
create table {0}
(
    {1}
);",
                this.FullName,
                string.Join("," + Environment.NewLine + "    ", output.ToArray()));
            }
            return drop + create;
        }

        public string GetDropConstraintsText()
        {
            var sb = new StringBuilder();
            foreach (var c in this.Constraints)
            {
                var def = string.Format("if exists(select * from information_schema.CONSTRAINT_table_usage where constraint_name = 'FK_{1}_{2}_{3}') alter table {0} drop constraint FK_{1}_{2}_{3};",
                        this.FullName,
                        this.KeyName,
                        SqlClientQueryGenerator.Tables[c.ForeignKeyTarget].KeyName,
                        c.Name);
                sb.AppendLine(def);
            }
            return sb.ToString();
        }
        static Dictionary<string, string> constraints = new Dictionary<string, string>();
        public string GetAddConstraintsText()
        {
            var sb = new StringBuilder();
            foreach (var c in this.Constraints)
            {
                var name = string.Format("FK_{0}_{1}_{2}", this.KeyName, SqlClientQueryGenerator.Tables[c.ForeignKeyTarget].KeyName, c.Name);
                var def = string.Format("alter table {0} add constraint FK_{1}_{2}_{3} foreign key({4}) references {5}({6}){7};",
                        this.FullName,
                        this.KeyName,
                        SqlClientQueryGenerator.Tables[c.ForeignKeyTarget].KeyName,
                        c.Name,
                        string.Join(
                            ", ",
                           SqlClientQueryGenerator.Tables[c.ForeignKeyTarget]
                                .KeyColumns
                                .Select(f => c.PrefixName ? c.Name + f.Name : f.Name)
                                .ToArray()),
                        SqlClientQueryGenerator.Tables[c.ForeignKeyTarget].FullName,
                        string.Join(
                            ", ",
                            SqlClientQueryGenerator.Tables[c.ForeignKeyTarget]
                                .KeyColumns
                                .Select(f => f.Name)
                                .ToArray()),
                                c.Cascade ? " on delete cascade on update cascade" : "");
                if (!constraints.ContainsKey(name))
                {
                    constraints.Add(name, def);
                    sb.AppendLine(def);
                }
                else if (constraints[name] != def)
                {
                    return "--- dork ---";
                }
            }
            return sb.ToString();
        }

        private Dictionary<string, Column> columnCache;
        public void Set(object row, string columnName, object value)
        {
            var currentTable = this;
            var parts = columnName.Split('.');
            for (int i = 0; i < parts.Length; ++i)
            {
                if (currentTable.columnCache == null)
                    currentTable.columnCache = currentTable.GetAllColumns().ToDictionary(c => c.Name);
                if (i == parts.Length - 1)
                {
                    if (currentTable.columnCache.ContainsKey(parts[i]))
                        currentTable.columnCache[parts[i]].Set(row, value);
                }
                else
                {
                    Type t = row.GetType();
                    var prop = t.GetProperty(parts[i]);
                    var temp = prop.GetValue(row, null);
                    if (temp == null)
                    {
                        var cons = prop.PropertyType.GetConstructor(Type.EmptyTypes);
                        temp = cons.Invoke(null);
                        prop.SetValue(row, temp, null);
                    }

                    currentTable = SqlClientQueryGenerator.Tables[prop.PropertyType];
                    row = temp;
                }
            }
        }

        public List<T> Read<T>(DbDataReader reader)
        {
            var cons = this.originalType.GetConstructor(Type.EmptyTypes);
            var output = new List<T>();
            var columnNames = Table.GetColumnNames(reader);
            while (reader.Read())
            {
                var row = cons.Invoke(null);
                output.Add((T)row);
                foreach (var columnName in columnNames)
                {
                    var value = reader[columnName];
                    Set(row, columnName, value);
                }
            }
            return output;
        }

        public IEnumerable<T> Schlep<T,ConnectionT, CommandT, ParameterT, DataAdapterT, DataReaderT>(DataReaderT reader)
            where ConnectionT : DbConnection, new()
            where CommandT : DbCommand, new()
            where ParameterT : DbParameter, new()
            where DataAdapterT : DbDataAdapter, new()
            where DataReaderT : DbDataReader
        {
            var cons = this.originalType.GetConstructor(Type.EmptyTypes);
            var columnNames = Table.GetColumnNames(reader);
            while (reader.Read())
            {
                var row = cons.Invoke(null);
                foreach (var columnName in columnNames)
                {
                    var value = reader[columnName];
                    Set(row, columnName, value);
                }
                yield return (T)row;
            }
        }

        private Lazy<List<Column>> getAllColumns;

        public List<Column> GetAllColumns()
        {
            return getAllColumns.Value;
        }

        public string BatchInsertQuery<T>(IEnumerable<T> objs)
        {
            var sb = new StringBuilder(this.InsertHeader());
            sb.Append(" ");
            var len = sb.Length;
            foreach (var obj in objs)
            {
                Console.Write('.');
                sb.AppendFormat(@"
{0},", this.ValueClause(obj));
            }
            sb[sb.Length - 1] = ';';
            if (sb.Length > len)
                return sb.ToString();
            return null;
        }

        public static string MakePrimaryKeyCode(object obj)
        {
            var t = obj.GetType();
            if (SqlClientQueryGenerator.Tables.ContainsKey(t))
            {
                return SqlClientQueryGenerator.Tables[t].MakePKCode(obj);
            }
            return null;
        }

        private string MakePKCode(object obj)
        {
            return string.Join("_", this.KeyColumns.Select(c => c.Get(obj).ToString()).ToArray());
        }

        internal string MakePKWhereClause<T>(T obj)
        {
            var whereClause = string.Join(
                Environment.NewLine + "AND ",
                this
                    .KeyColumns
                    .Select(c => string.Format(
                        "{0}.{1} = '{2}'",
                        this.Name,
                        c.Name,
                        c.Get(obj)))
                    .ToArray());
            return whereClause;
        }
    }
}
