using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Daedalus
{
    [Serializable]
    class Table
    {
        public static string DefaultSchema;
        static Table()
        {
            DefaultSchema = "dbo";
        }

        public string Schema;
        private readonly string name;
        public List<Column> Columns, Constraints;
        public bool IncludeInScript;

        public string PrimaryKey
        {
            get
            {
                return string.Join(", ", (from column in this.Columns where column.IsPrimaryKey select column.Name).ToArray());
            }
        }

        public string FullName
        {
            get
            {
                return this.Schema + "." + this.name;
            }
        }

        public static Table ParseTable(string input, AuthType auth)
        {
            input = input
                .Replace("[TSPAN]", "[TSTART]\r\n[TEND]")
                .Replace("[TSTART]", "CreatedOn DateTime default(getdate())")
                .Replace("[TEND]", "DeactivatedOn DateTime default('9999-12-31')")
                .Replace("[LCODE]", "[SPK]\r\n[LCODENAME]");

            var lines = from line in input.Split('\n') select line.Trim();
            var table = new Table(lines.First().Substring(1), lines.Skip(1));

            return table;
        }


        public Table(string name, IEnumerable<string> lines)
        {
            Schema = Table.DefaultSchema;
            this.name = name;
            this.IncludeInScript = this.name[0] != '&';
            if (!this.IncludeInScript)
                this.name = this.name.Substring(1);
            if (this.name.Contains('.'))
            {
                this.Schema = this.name.Substring(0, this.name.IndexOf('.'));
                this.name = this.name.Substring(this.name.IndexOf('.') + 1);
            }
            this.Columns = new List<Column>();
            this.Constraints = new List<Column>();
            foreach (var line in lines)
            {
                try
                {
                    var col = Column.ParseColumn(line.Trim(), this.name);
                    if (col.IsForeignKey)
                        this.Constraints.Add(col);
                    else
                        this.Columns.Add(col);
                }
                catch (FormatException exp)
                {
                    Console.Error.WriteLine(@">> {0}
in table {1} is not valid: {2}", line, this.FullName, exp.Message);
                }
            }
        }

        public string GetTrackingTrigger()
        {
            var columnNames = string.Join(", ", (from column in this.Columns where !column.IsComment select column.Name).ToArray());
            return string.Format(@"go
create trigger {0}_Tracker
	on {0}
	after insert, update, delete
	as
		insert into audit_{0}
		({1}, sys_Operation)
		select {1}, 'Deleted'
		from deleted;
		insert into audit_{0}
		({1}, sys_Operation)
		select {1}, 'Inserted'
		from inserted;",
                       this.FullName,
                       columnNames);
        }

        public void AddColumnsForForeignKeys(Dictionary<string, Table> tables)
        {
            this.Constraints.ForEach(c => this.Columns.AddRange(c.CreateForeignKeyColumns(tables)));
        }

        public string GetCreateTableText(Dictionary<string, Table> tables)
        {
            var output = new List<string>();
            output.AddRange(from column in this.Columns select "    " + column.GetColumnText());

            if (tables.ContainsKey(this.FullName))
                output.Add(string.Format("    constraint PK_{0} primary key({1})",
                    this.FullName.Replace('.', '_'),
                    tables[this.FullName].PrimaryKey));

            var sb = new StringBuilder();
            sb.AppendFormat(@"if exists(select * from information_schema.tables where table_name = '{1}' and table_schema = '{2}') drop table {0};
create table {0}
(
", this.FullName,
 this.name,
 this.Schema);
            sb.AppendLine(string.Join("," + Environment.NewLine, output.ToArray()));

            sb.AppendLine(");");
            return sb.ToString();
        }

        public string GetAddConstraintsText(Dictionary<string, Table> tables)
        {
            var output = new List<string>();
            output.AddRange(from column in this.Constraints where column.IsForeignKey select column.GetAddConstraintsText(this, tables));

            var sb = new StringBuilder();
            output.ToList().ForEach(l => sb.AppendLine(l));
            return sb.ToString();
        }

        public string GetDropConstraintsText(Dictionary<string, Table> tables)
        {
            var output = new List<string>();
            output.AddRange(from column in this.Constraints where column.IsForeignKey select column.GetDropConstraintsText(this, tables));

            var sb = new StringBuilder();
            output.ToList().ForEach(l => sb.AppendLine(l));
            return sb.ToString();
        }

        public string MakeParam(IEnumerable<Column> columns, Func<Column, string> formater, string separator)
        {
            return string.Join(separator, (from col in columns select formater(col)).ToArray());
        }
       

        public string GetEntityObjectText(string Namespace)
        {
            var fields = MakeParam(Columns,
                (col => col.GetCSharpText()),
                "\r\n        ");
            return string.Format(@"using System;

namespace {0}.{1}
{{
    public class {2}
    {{
        {3}
    }}
}}",
  Namespace,
  this.Schema,
  this.name,
  fields);
        }

        public string GetMappedMethodsText()
        {
            var cols = from col in Columns where !col.IsComment select col;
            var pkCols = from col in cols where col.IsPrimaryKey select col;

            var getParamList1 = MakeParam(
                pkCols,
                (col => string.Format("{0} {1}", col.DotNetType, col.Name)),
                ", ");

            var getParamList2 = MakeParam(
                pkCols,
                (col => col.Name),
                ", ");

            var setParamList1 = MakeParam(
                cols,
                (col => string.Format("{0}{2} {1}", col.DotNetType, col.Name, (col.IsIdentity || col.IsNullable) && !col.DotNetType.Equals("string") ? "?" : "")),
                ", ");

            var setParamList2 = MakeParam(
                cols,
                (col => col.Name),
                ", ");

            return string.Format(@"
        [MappedMethod(Schema = ""{0}"")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public List<{1}> Get_{1}({2})
        {{
            return this.GetList<{1}>({3});
        }}

        [MappedMethod(Schema = ""{0}"")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public void Set_{1}({4})
        {{
            this.Execute({5});
        }}

        [MappedMethod(Schema = ""{0}"")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public void Delete_{1}({4})
        {{
            this.Execute({5});
        }}",
           this.Schema,
           this.name,
           getParamList1,
           getParamList2,
           setParamList1,
           setParamList2);
        }
    }
}
