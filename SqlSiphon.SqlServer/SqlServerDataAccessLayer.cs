/*
https://www.github.com/capnmidnight/SqlSiphon
Copyright (c) 2009, 2010, 2011, 2012, 2013 Sean T. McBeth
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this 
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or 
  other materials provided with the distribution.

* Neither the name of McBeth Software Systems nor the names of its contributors
  may be used to endorse or promote products derived from this software without 
  specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE 
OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED 
OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using SqlSiphon.Mapping;

namespace SqlSiphon.SqlServer
{
    /// <summary>
    /// A base class for building Data Access Layers that connect to MS SQL Server 2005/2008
    /// databases and execute store procedures stored within.
    /// </summary>
    public class SqlServerDataAccessLayer : SqlSiphon<SqlConnection, SqlCommand, SqlParameter, SqlDataAdapter, SqlDataReader>
    {
        public const string DATABASE_TYPE_NAME = "Microsoft SQL Server";
        public override string DatabaseType
        {
            get { return DATABASE_TYPE_NAME; }
        }
        /// <summary>
        /// creates a new connection to a MS SQL Server 2005/2008 database and automatically
        /// opens the connection. 
        /// </summary>
        /// <param name="connectionString">a standard MS SQL Server connection string</param>
        public SqlServerDataAccessLayer(string connectionString)
            : base(connectionString)
        {
        }

        public SqlServerDataAccessLayer(SqlConnection connection)
            : base(connection)
        {
        }

        public SqlServerDataAccessLayer(SqlServerDataAccessLayer dal)
            : base(dal)
        {
        }

        public SqlServerDataAccessLayer(string server, string database, string userName, string password)
            : base(server, database, userName, password)
        {
        }

        protected SqlServerDataAccessLayer()
            : base()
        {

        }

        public override string MakeConnectionString(string server, string database, string userName, string password)
        {
            var builder = new System.Data.SqlClient.SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = database
            };

            builder.IntegratedSecurity = string.IsNullOrWhiteSpace(userName)
                || string.IsNullOrWhiteSpace(password);
            if (!builder.IntegratedSecurity)
            {
                builder.UserID = userName.Trim();
                builder.Password = password.Trim();
            }
            return builder.ConnectionString;
        }

        protected override string IdentifierPartBegin { get { return "["; } }
        protected override string IdentifierPartEnd { get { return "]"; } }
        public override string DefaultSchemaName { get { return "dbo"; } }
        private static Dictionary<string, int> defaultTypePrecisions;
        public override int DefaultTypePrecision(string typeName, int testPrecision)
        {
            if (!defaultTypePrecisions.ContainsKey(typeName))
            {
                throw new Exception(string.Format("I don't know the default precision for type `{0}`. Perhaps it is {1}?", typeName, testPrecision));
            }
            return defaultTypePrecisions[typeName];
        }

        private const SqlServerOptions STANDARD_OPTIONS = SqlServerOptions.ANSI_WARNINGS | SqlServerOptions.ANSI_PADDING | SqlServerOptions.ANSI_NULLS | SqlServerOptions.ARITHABORT | SqlServerOptions.QUOTED_IDENTIFIER | SqlServerOptions.ANSI_NULL_DFLT_ON | SqlServerOptions.CONCAT_NULL_YIELDS_NULL;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query = @"select cast(@@OPTIONS as int) as Options")]
        public SqlServerOptions GetSqlServerOptions()
        {
            return (SqlServerOptions)this.Get<int>(0);
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            try
            {
                var options = this.GetSqlServerOptions();
                var allOptions = Enum.GetValues(typeof(SqlServerOptions))
                    .Cast<SqlServerOptions>()
                    .ToArray();

                foreach (var option in allOptions)
                {
                    if ((STANDARD_OPTIONS & option) != SqlServerOptions.None
                        && (options & option) == SqlServerOptions.None)
                    {
                        this.Execute(string.Format("SET {0} ON;", option));
                    }
                }
            }
            catch
            {
                // not going to care about the error
            }
        }

        private static Dictionary<string, Type> typeMapping;
        private static Dictionary<Type, string> reverseTypeMapping;
        static SqlServerDataAccessLayer()
        {
            typeMapping = new Dictionary<string, Type>();
            typeMapping.Add("bigint", typeof(long));
            typeMapping.Add("int", typeof(int));
            typeMapping.Add("smallint", typeof(short));
            typeMapping.Add("tinyint", typeof(byte));
            typeMapping.Add("decimal", typeof(decimal));
            typeMapping.Add("numeric", typeof(decimal));
            typeMapping.Add("money", typeof(decimal));
            typeMapping.Add("smallmoney", typeof(decimal));
            typeMapping.Add("bit", typeof(bool));
            typeMapping.Add("float", typeof(float));
            typeMapping.Add("real", typeof(double));
            typeMapping.Add("datetime2", typeof(DateTime));
            typeMapping.Add("datetime", typeof(DateTime));
            typeMapping.Add("smalldatetime", typeof(DateTime));
            typeMapping.Add("date", typeof(DateTime));
            typeMapping.Add("datetimeoffset", typeof(DateTime));
            typeMapping.Add("time", typeof(DateTime));
            typeMapping.Add("timestamp", typeof(DateTime));
            typeMapping.Add("nvarchar", typeof(string));
            typeMapping.Add("char", typeof(string));
            typeMapping.Add("varchar", typeof(string));
            typeMapping.Add("text", typeof(string));
            typeMapping.Add("nchar", typeof(string));
            typeMapping.Add("ntext", typeof(string));
            typeMapping.Add("varbinary", typeof(byte[]));
            typeMapping.Add("binary", typeof(byte[]));
            typeMapping.Add("image", typeof(byte[]));
            typeMapping.Add("uniqueidentifier", typeof(Guid));

            defaultTypePrecisions = new Dictionary<string, int>();
            defaultTypePrecisions.Add("nvarchar", 0); 
            defaultTypePrecisions.Add("int", 10);
            defaultTypePrecisions.Add("real", 24);
            defaultTypePrecisions.Add("datetime2", 27);

            reverseTypeMapping = typeMapping
                .GroupBy(kv => kv.Value, kv => kv.Key)
                .ToDictionary(g => g.Key, g => g.First());

            reverseTypeMapping.Add(typeof(char[]), "nchar");

            reverseTypeMapping.Add(typeof(int?), "int");
            reverseTypeMapping.Add(typeof(uint), "int");
            reverseTypeMapping.Add(typeof(uint?), "int");

            reverseTypeMapping.Add(typeof(long?), "bigint");
            reverseTypeMapping.Add(typeof(ulong), "bigint");
            reverseTypeMapping.Add(typeof(ulong?), "bigint");

            reverseTypeMapping.Add(typeof(short?), "smallint");
            reverseTypeMapping.Add(typeof(ushort), "smallint");
            reverseTypeMapping.Add(typeof(ushort?), "smallint");

            reverseTypeMapping.Add(typeof(byte?), "tinyint");
            reverseTypeMapping.Add(typeof(sbyte), "tinyint");
            reverseTypeMapping.Add(typeof(sbyte?), "tinyint");
            reverseTypeMapping.Add(typeof(char), "tinyint");
            reverseTypeMapping.Add(typeof(char?), "tinyint");

            reverseTypeMapping.Add(typeof(decimal?), "decimal");
            reverseTypeMapping.Add(typeof(bool?), "bit");
            reverseTypeMapping.Add(typeof(float?), "float");
            reverseTypeMapping.Add(typeof(double?), "real");
            reverseTypeMapping.Add(typeof(DateTime?), "datetime2");
            reverseTypeMapping.Add(typeof(Guid?), "uniqueidentifier");
        }


        public static string MakeUDTTName(Type t)
        {
            if (t.IsArray)
                t = t.GetElementType();

            var attr = DatabaseObjectAttribute.GetAttribute<SqlServerTableAttribute>(t);
            if (attr == null)
            {
                attr = new SqlServerTableAttribute();
                attr.Name = t.Name;
            }

            attr.InferProperties(t);

            return attr.Name + "UDTT";
        }

        public override string MakeRoutineIdentifier(RoutineAttribute routine)
        {
            return this.MakeIdentifier(routine.Schema ?? this.DefaultSchemaName, routine.Name);
        }

        public override string MakeDropRoutineScript(RoutineAttribute info)
        {
            return string.Format("drop procedure {0};", this.MakeRoutineIdentifier(info));
        }

        public override string MakeRoutineBody(RoutineAttribute info)
        {
            var query = info.Query.Replace("into returnValue", "")
                .Replace("return query ", "")
                .Replace("perform ", "select ");
            if (info.EnableTransaction)
            {
                string transactionName = string.Format("TRANS{0}{1}", info.Schema ?? DefaultSchemaName, info.Name);
                int len = transactionName.Length;
                if (len > 32)
                {
                    var lenlen = (int)Math.Ceiling(Math.Log10(len));
                    transactionName = transactionName.Substring(0, 32 - lenlen) + lenlen.ToString();
                }
                string transactionBegin = string.Format(@"begin try
    begin transaction {0};", transactionName);
                string transactionEnd = string.Format(@"commit transaction {0};
end try
begin catch
    declare @msg nvarchar(4000), @lvl int, @stt int;
    select @msg = error_message(), @lvl = error_severity(), @stt = error_state();
    rollback transaction {0};
    raiserror(@msg, @lvl, @stt);
end catch;", transactionName);
                query = string.Join(Environment.NewLine, transactionBegin, query, transactionEnd);
            }
            var identifier = this.MakeIdentifier(info.Schema ?? DefaultSchemaName, info.Name);
            var parameters = info.Parameters.Select(this.MakeParameterString);
            var parameterSection = string.Join(", ", parameters);
            var withRecompile = info.GetAttribute<SqlServerWithRecompileAttribute>() != null;
            return string.Format(
@"create procedure {0}
    {1}
{2}as begin
    set nocount on;
    {3}
end",
                identifier,
                parameterSection,
                withRecompile ? "with recompile\r\n" : "",
                query);
        }

        public override string MakeCreateRoutineScript(RoutineAttribute info, bool createBody = true)
        {
            return createBody ? this.MakeRoutineBody(info) : info.Query;
        }

        public override void AnalyzeQuery(string routineText, RoutineAttribute routine)
        {
            base.AnalyzeQuery(routineText, routine);
            routine.EnableTransaction = routineText.IndexOf("begin transaction TRANS") > -1;
        }

        public override string MakeCreateTableScript(TableAttribute info)
        {
            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = this.MakeIdentifier(schema, info.Name);
            var columnSection = this.MakeColumnSection(info, false);
            return string.Format(
@"if not exists(select * from information_schema.tables where table_schema = '{0}' and table_name = '{1}')
create table {2}(
    {3}
);",
                schema,
                info.Name,
                identifier,
                columnSection);
        }

        internal string MakeCreateUDTTScript(TableAttribute info)
        {
            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = this.MakeIdentifier(schema, info.Name);
            var columnSection = this.MakeColumnSection(info, false);
            return string.Format(
@"if not exists(select * from sys.table_types tt inner join sys.schemas s on s.schema_id = tt.schema_id where s.name = '{0}' and tt.name = '{1}') create type {2} as table(
    {3}
)",
                schema,
                info.Name,
                identifier,
                columnSection);
        }

        public override string MakeDropTableScript(TableAttribute info)
        {
            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = this.MakeIdentifier(schema, info.Name);
            return string.Format(
@"if exists(select * from information_schema.tables where table_schema = '{0}' and table_name = '{1}')
    drop table {2};",
                schema,
                info.Name,
                identifier);
        }

        internal string MakeDropUDTTScript(TableAttribute info)
        {
            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = this.MakeIdentifier(schema, info.Name);
            return string.Format(
@"if exists(select * from sys.table_types tt inner join sys.schemas s on s.schema_id = tt.schema_id where s.name = '{0}' and tt.name = '{1}')
    drop type {2}", schema, info.Name, identifier);
        }

        public override string MakeCreateIndexScript(Index idx)
        {
            var columnSection = string.Join(",", idx.Columns);
            var tableName = MakeIdentifier(idx.Table.Schema ?? DefaultSchemaName, idx.Table.Name);
            return string.Format(
@"if not exists(select * from sys.indexes where name = '{0}')
CREATE NONCLUSTERED INDEX {0} ON {1}({2});",
                idx.Name,
                tableName,
                columnSection);
        }

        public override string MakeDropIndexScript(Index idx)
        {
            var tableName = MakeIdentifier(idx.Table.Schema ?? DefaultSchemaName, idx.Table.Name);
            return string.Format(
@"if exists(select * from sys.indexes where name = '{0}')
DROP INDEX {0} ON {1};",
                idx.Name,
                tableName);
        }

        private static string GetDefaultValue(DatabaseObjectAttribute p)
        {
            var defaultValue = p.DefaultValue;
            if (defaultValue == null)
            {
                defaultValue = "";
            }
            else if (p.SystemType == typeof(bool))
            {
                var testValue = defaultValue.ToLowerInvariant();
                defaultValue = testValue == "true" ? "1" : "0";
            }
            return defaultValue;
        }

        protected override string MakeParameterString(ParameterAttribute p)
        {
            var typeStr = MakeSqlTypeString(p);
            var isUDTT = p.IsUDTT || (p.SystemType != null && IsUDTT(p.SystemType));
            return string.Join(" ",
                "@" + p.Name,
                typeStr,
                GetDefaultValue(p),
                isUDTT ? "readonly" : "").Trim();
        }

        protected override string MakeColumnString(ColumnAttribute p, bool isReturnType)
        {
            var typeStr = MakeSqlTypeString(p);
            var defaultString = "";
            if (p.DefaultValue != null)
                defaultString = string.Format("DEFAULT ({0})", GetDefaultValue(p));
            else if (p.IsIdentity)
                defaultString = "IDENTITY(1, 1)";

            return string.Format("{0} {1} {2} {3}",
                p.Name,
                typeStr,
                p.IsOptional ? "" : "NOT NULL",
                defaultString);
        }

        public override string MakeCreateColumnScript(ColumnAttribute prop)
        {
            return string.Format("alter table {0} add {1} {2};",
                this.MakeIdentifier(prop.Table.Schema ?? DefaultSchemaName, prop.Table.Name),
                this.MakeIdentifier(prop.Name),
                this.MakeSqlTypeString(prop));
        }

        public override string MakeDropColumnScript(ColumnAttribute prop)
        {
            return string.Format("alter table {0} drop column {1};",
                this.MakeIdentifier(prop.Table.Schema ?? DefaultSchemaName, prop.Table.Name),
                this.MakeIdentifier(prop.Name));
        }

        public override bool ColumnChanged(ColumnAttribute final, ColumnAttribute initial)
        {
            var finalType = final.SystemType;
            if (finalType.IsEnum)
            {
                finalType = typeof(int);
            }
            var tests = new bool[]{
                final.Include != initial.Include,
                final.IsOptional != initial.IsOptional,
                final.Name.ToLowerInvariant() != initial.Name.ToLowerInvariant(),
                finalType != initial.SystemType,
                final.Table == null || initial.Table == null || final.Table.Schema.ToLowerInvariant() != initial.Table.Schema.ToLowerInvariant(),
                final.Table == null || initial.Table == null || final.Table.Name.ToLowerInvariant() != initial.Table.Name.ToLowerInvariant()
            };
            var changed = tests.Aggregate((a, b) => a || b);
            if (!changed)
            {
                changed = final.DefaultValue != initial.DefaultValue;
                if (changed
                    && final.DefaultValue != null
                    && initial.DefaultValue != null)
                {
                    bool valuesMatch = true;
                    if (final.SystemType == typeof(bool))
                    {
                        var xb = final.DefaultValue.Replace("'", "").ToLowerInvariant();
                        var xbb = xb == "true" || xb == "1";
                        var yb = initial.DefaultValue
                            .Replace("'", "")
                            .Replace("((", "")
                            .Replace("))", "")
                            .ToLowerInvariant();
                        var ybb = yb == "true" || yb == "1";
                        valuesMatch = xbb == ybb;
                    }
                    else if (final.SystemType == typeof(DateTime)
                        || final.SystemType == typeof(double)
                        || final.SystemType == typeof(int))
                    {
                        valuesMatch = final.DefaultValue == "(" + initial.DefaultValue + ")"
                            || initial.DefaultValue == "(" + final.DefaultValue + ")"
                            || final.DefaultValue == "((" + initial.DefaultValue + "))"
                            || initial.DefaultValue == "((" + final.DefaultValue + "))"
                            || final.DefaultValue == "('" + initial.DefaultValue + "')"
                            || initial.DefaultValue == "('" + final.DefaultValue + "')"; ;
                    }

                    if (valuesMatch)
                    {
                        initial.DefaultValue = final.DefaultValue;
                    }
                    changed = !valuesMatch;
                }
                else if (final.Size != initial.Size)
                {
                    changed = true;
                }
            }
            return changed;
        }

        public override string MakeAlterColumnScript(ColumnAttribute final, ColumnAttribute initial)
        {
            var preamble = string.Format(
                "alter table {0}",
                this.MakeIdentifier(final.Table.Schema ?? DefaultSchemaName, final.Table.Name));

            if (final.Include != initial.Include)
            {
                return string.Format(
                    "{0} {1} {2} {3};",
                    preamble,
                    final.Include ? "add" : "drop column",
                    this.MakeIdentifier(final.Name),
                    final.Include ? this.MakeSqlTypeString(final) : "")
                    .Trim();
            }
            else if (final.DefaultValue != initial.DefaultValue)
            {
                if (final.DefaultValue != null)
                {
                    return string.Format(
                        "{0} add constraint {1} default({2}) for {3};",
                        preamble,
                        this.MakeIdentifier("DEF_" + final.Name),
                        final.DefaultValue ?? "",
                        final.Name);
                }
                else
                {
                    return string.Format(
                        "{0} drop constraint {1};",
                        preamble,
                        this.MakeIdentifier("DEF_" + final.Name));
                }
            }
            else if (final.IsOptional != initial.IsOptional)
            {
                return string.Format(
                    "{0} alter column {1} {2} {3} null;",
                    preamble,
                    this.MakeIdentifier(final.Name),
                    this.MakeSqlTypeString(final),
                    final.IsOptional ? "" : "not");
            }
            else if (final.SystemType != initial.SystemType
                || final.Size != initial.Size
                || final.Precision != initial.Precision)
            {
                return string.Format(
                    "{0} alter column {1} set data type {2};",
                    preamble,
                    this.MakeIdentifier(final.Name),
                    this.MakeSqlTypeString(final));
            }
            else
            {
                // by this point, the columns should be identical, but we still need a base case
                return null;
            }
        }

        protected override string MakeSqlTypeString(string sqlType, Type systemType, int? size, int? precision, bool isIdentity)
        {
            if (sqlType == null)
            {
                if (reverseTypeMapping.ContainsKey(systemType))
                    sqlType = reverseTypeMapping[systemType];
                else if (IsUDTT(systemType))
                    sqlType = MakeUDTTName(systemType);
            }

            if (sqlType != null)
            {
                if (sqlType[sqlType.Length - 1] == ')') // someone already setup the type name, so skip it
                    return sqlType;
                else
                {
                    var typeStr = new StringBuilder(sqlType);
                    if (size.HasValue && (systemType != typeof(int) || size.Value > 0))
                    {
                        typeStr.AppendFormat("({0}", size);
                        if (precision.HasValue)
                        {
                            typeStr.AppendFormat(", {0}", precision);
                        }
                        typeStr.Append(")");
                    }

                    if (sqlType.Contains("var")
                        && typeStr[typeStr.Length - 1] != ')')
                    {
                        typeStr.Append("(MAX)");
                    }
                    return typeStr.ToString();
                }
            }
            else
            {
                return null;
            }
        }

        private string MaybeMakeColumnTypeString(ColumnAttribute attr, bool skipDefault = false)
        {
            if (reverseTypeMapping.ContainsKey(attr.SystemType))
            {
                var typeStr = MakeSqlTypeString(attr);
                return string.Format("{0} {1} {2}NULL {3}",
                    attr.Name,
                    typeStr,
                    attr.IsOptional ? "" : "NOT ",
                    !skipDefault ? attr.DefaultValue ?? "" : "").Trim();
            }
            return null;
        }

        private static DataTable MakeDataTable(string tableName, Type t, System.Collections.IEnumerable array)
        {
            var table = new DataTable(tableName);
            if (reverseTypeMapping.ContainsKey(t))
            {
                table.Columns.Add("Value", t);
                foreach (object obj in array)
                {
                    table.Rows.Add(new object[] { obj });
                }
            }
            else
            {
                // don't upload auto-incrementing identity columns
                // or columns that have a default value defined
                var props = GetProperties(t);
                var columns = props
                    .Where(p => p.Include && !p.IsIdentity && (p.IsIncludeSet || p.DefaultValue == null))
                    .ToList();
                foreach (var column in columns)
                {
                    table.Columns.Add(column.Name, column.SystemType);
                }
                foreach (object obj in array)
                {
                    List<object> row = new List<object>();
                    foreach (var column in columns)
                    {
                        var element = column.GetValue<object>(obj);
                        row.Add(element);
                    }
                    table.Rows.Add(row.ToArray());
                }
            }
            return table;
        }

        public override string MakeCreateRelationshipScript(Relationship relation)
        {
            var fromColumns = string.Join(", ", relation.FromColumns.Select(c => this.MakeIdentifier(c.Name)));
            var toColumns = string.Join(", ", relation.To.PrimaryKey.KeyColumns.Select(c => this.MakeIdentifier(c.Name)));

            return string.Format(
@"alter table {0} add constraint {1}
    foreign key({2})
    references {3}({4})",
                    this.MakeIdentifier(relation.From.Schema ?? DefaultSchemaName, relation.From.Name),
                    this.MakeIdentifier(relation.GetName(this)),
                    fromColumns,
                    this.MakeIdentifier(relation.To.Schema ?? DefaultSchemaName, relation.To.Name),
                    toColumns);
        }

        public override string MakeDropRelationshipScript(Relationship relation)
        {
            return string.Format(@"alter table {0} drop constraint {1}",
                    this.MakeIdentifier(relation.From.Schema ?? DefaultSchemaName, relation.From.Name),
                    this.MakeIdentifier(relation.GetName(this)));
        }

        public override string MakeDropPrimaryKeyScript(PrimaryKey key)
        {
            return string.Format(@"alter table {0} drop constraint {1};",
                this.MakeIdentifier(key.Table.Schema ?? DefaultSchemaName, key.Table.Name),
                this.MakeIdentifier(key.GetName(this)));
        }

        public override string MakeCreatePrimaryKeyScript(PrimaryKey key)
        {
            var keys = string.Join(", ", key.KeyColumns.Select(c => this.MakeIdentifier(c.Name)));
            return string.Format(@"alter table {0} add constraint {1} primary key({2});",
                this.MakeIdentifier(key.Table.Schema ?? DefaultSchemaName, key.Table.Name),
                this.MakeIdentifier(key.GetName(this)),
                keys);
        }


        public override bool DescribesIdentity(InformationSchema.Columns column)
        {
            return column.is_identity == 1;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query = @"select name from sys.sysusers")]
        public override List<string> GetDatabaseLogins()
        {
            return this.GetList<string>();
        }

        public override string MakeCreateDatabaseLoginScript(string userName, string password, string database)
        {
            return string.Format(@"CREATE LOGIN {0} WITH PASSWORD = '{1}', DEFAULT_DATABASE={2};
USE {2};
CREATE USER {0} FOR LOGIN {0};
ALTER USER {0} WITH DEFAULT_SCHEMA=dbo;
ALTER ROLE db_owner ADD MEMBER {0};", userName, password, database);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select schema_name from information_schema.schemata where schema_name not like 'db_%' and schema_name not in ('information_schema', 'dbo', 'guest', 'sys');")]
        public override List<string> GetSchemata()
        {
            return this.GetList<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select s.name as table_schema, tt.name as table_name, c.name as column_name, t.name as data_type, c.max_length as character_maximum_length, c.precision as numeric_precision, c.scale as numeric_scale, c.is_nullable, c.is_identity, d.definition as column_default
from sys.table_types tt
	inner join sys.columns c on c.object_id = tt.type_table_object_id
	inner join sys.schemas s on s.schema_id = tt.schema_id
	inner join sys.types t on t.system_type_id = c.system_type_id
	left outer join sys.default_constraints d on d.object_id = c.default_object_id
where t.name != 'sysname'
order by s.name, tt.name, c.column_id;")]
        private List<InformationSchema.Columns> GetUDTTColumns()
        {
            var columns = this.GetEnumerator<InformationSchema.Columns>()
                .Select(RemoveDefaultValueParens)
                .ToList();
            foreach (var column in columns)
            {
                if (column.data_type == "nvarchar" && column.character_maximum_length != -1)
                {
                    column.character_maximum_length /= 2;
                }
            }
            return columns;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select *, COLUMNPROPERTY(object_id(TABLE_NAME), COLUMN_NAME, 'IsIdentity') as is_identity
from information_schema.columns
where table_schema != 'information_schema'
order by table_catalog, table_schema, table_name, ordinal_position;")]
        public override List<InformationSchema.Columns> GetColumns()
        {
            var columns = this.GetEnumerator<InformationSchema.Columns>()
                .Select(RemoveDefaultValueParens)
                .ToList();
            return columns;
        }

        /// <summary>
        /// SQL Server wraps default values in parens, and it can do this many times, just to vex us!
        /// </summary>
        /// <param name="columns"></param>
        private static InformationSchema.Columns RemoveDefaultValueParens(InformationSchema.Columns column)
        {
            if (column.column_default != null)
            {
                while (column.column_default.StartsWith("("))
                {
                    column.column_default = column.column_default.Substring(1);
                }
                while (column.column_default.EndsWith(")"))
                {
                    column.column_default = column.column_default.Substring(0, column.column_default.Length - 1);
                }
                if (column.column_default.EndsWith("("))
                {
                    column.column_default += ")";
                }
            }
            return column;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"SELECT 
	s.name as table_schema,
	t.name as table_name,
	ind.name as index_name,
	col.name as column_name
FROM sys.indexes ind
INNER JOIN sys.index_columns ic ON ind.object_id = ic.object_id and ind.index_id = ic.index_id 
INNER JOIN sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id 
INNER JOIN sys.tables t ON ind.object_id = t.object_id 
INNER JOIN sys.schemas s on t.schema_id = s.schema_id
WHERE 
     ind.is_primary_key = 0 
     AND ind.is_unique = 0 
     AND ind.is_unique_constraint = 0 
     AND t.is_ms_shipped = 0 
ORDER BY 
     t.name, ind.name, ind.index_id, ic.index_column_id;")]
        public override List<InformationSchema.IndexColumnUsage> GetIndexColumns()
        {
            return GetList<InformationSchema.IndexColumnUsage>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select *
from information_schema.table_constraints
where table_schema != 'information_schema'
order by table_catalog, table_schema, table_name;")]
        public override List<InformationSchema.TableConstraints> GetTableConstraints()
        {
            return GetList<InformationSchema.TableConstraints>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select *
from information_schema.referential_constraints
where constraint_schema != 'information_schema'
order by constraint_catalog, constraint_schema, constraint_name;")]
        public override List<InformationSchema.ReferentialConstraints> GetReferentialConstraints()
        {
            return GetList<InformationSchema.ReferentialConstraints>();
        }


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select
specific_catalog,
specific_schema,
specific_name,
routine_catalog,
routine_schema,
routine_name,
object_definition(object_id(routine_schema + '.' + routine_name)) as routine_definition
from information_schema.routines
where specific_schema != 'information_schema'
order by specific_catalog, specific_schema, specific_name;")]
        public override List<InformationSchema.Routines> GetRoutines()
        {
            return GetList<InformationSchema.Routines>();
        }


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select *
from information_schema.parameters
where specific_schema != 'information_schema'
order by specific_catalog, specific_schema, specific_name, ordinal_position;")]
        public override List<InformationSchema.Parameters> GetParameters()
        {
            return GetList<InformationSchema.Parameters>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select * 
from information_schema.constraint_column_usage
where constraint_schema != 'information_schema';")]
        public override List<InformationSchema.ConstraintColumnUsage> GetConstraintColumns()
        {
            return GetList<InformationSchema.ConstraintColumnUsage>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select * 
from information_schema.key_column_usage
where constraint_schema != 'information_schema';")]
        public override List<InformationSchema.KeyColumnUsage> GetKeyColumns()
        {
            return GetList<InformationSchema.KeyColumnUsage>();
        }

        /// <summary>
        /// Implements bulk-insert capabilities for MS SQL Server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public override void InsertAll<T>(IEnumerable<T> data)
        {
            if (data != null)
            {
                var t = typeof(T);
                var attr = DatabaseObjectAttribute.GetAttribute<TableAttribute>(t);
                if (attr == null)
                {
                    throw new Exception(string.Format("Type {0}.{1} could not be automatically inserted.", t.Namespace, t.Name));
                }
                attr.InferProperties(t);
                var tableData = MakeDataTable(attr.Name, t, data);

                //should make it using bulk insert when mono-project fix it for varbinary data
                //see https://bugzilla.xamarin.com/show_bug.cgi?id=20563
                var usesVarBinary = tableData.Columns.Cast<DataColumn>().Any(c => c.DataType == typeof(byte[]));
                if (IsOnMonoRuntime && usesVarBinary)
                {
                    base.InsertAll(data);
                }
                else
                {
                    if (this.Connection.State == ConnectionState.Closed)
                    {
                        this.Connection.Open();
                    }
                    var bulkCopy = new SqlBulkCopy(this.Connection);
                    foreach (var column in tableData.Columns.Cast<DataColumn>())
                    {
                        bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }
                    bulkCopy.DestinationTableName = attr.Name;
                    bulkCopy.WriteToServer(tableData);
                }
            }
        }

        public override Type GetSystemType(string sqlType)
        {
            return typeMapping.ContainsKey(sqlType) ? typeMapping[sqlType] : null;
        }

        public override DatabaseState GetInitialState(string catalogueName, Regex filter)
        {
            var state = base.GetInitialState(catalogueName, filter);
            var ssState = new SqlServerDatabaseState(state);
            if (ssState.CatalogueExists.HasValue && ssState.CatalogueExists.Value)
            {
                var udttColumns = this.GetUDTTColumns().ToHash(col => this.MakeIdentifier(col.table_schema, col.table_name));
                foreach (var udtt in udttColumns)
                {
                    ssState.UDTTs.Add(udtt.Key.ToLowerInvariant(), new TableAttribute(udtt.Value, this));
                }
            }
            return ssState;
        }

        public override DatabaseState GetFinalState(Type dalType, string userName, string password)
        {
            var state = base.GetFinalState(dalType, userName, password);
            var ssState = new SqlServerDatabaseState(state);
            var types = new HashSet<Type>();
            foreach (var parameter in ssState.Functions.Values.SelectMany(f => f.Parameters))
            {
                if (IsUDTT(parameter.SystemType))
                {
                    var elementType = parameter.SystemType;
                    if (elementType.IsArray)
                    {
                        elementType = elementType.GetElementType();
                    }
                    types.Add(elementType);
                }
            }
            foreach (var type in types)
            {
                var attr = new TableAttribute();
                var scanType = type;
                attr.Name = type.Name + "UDTT";
                if (DataConnector.IsTypePrimitive(type))
                {
                    var valueColumn = new ColumnAttribute
                    {
                        Name = "Value"
                    };
                    valueColumn.InferProperties(type);
                    attr.Properties.Add(valueColumn);
                    scanType = null;
                }
                ssState.AddTable(ssState.UDTTs, scanType, this, attr);
            }
            return ssState;
        }

        public static bool IsUDTT(Type t)
        {
            var isUDTT = false;
            if (t.IsArray)
            {
                t = t.GetElementType();
                isUDTT = t != typeof(byte) && DataConnector.IsTypePrimitive(t);
            }
            var attr = Mapping.DatabaseObjectAttribute.GetAttribute<SqlServerTableAttribute>(t);
            return (attr != null && attr.IsUploadable) || isUDTT;
        }

        protected override void ToOutput(string value)
        {
            if (ParseMessageForError(value))
            {
                base.ToError(value);
            }
            else
            {
                base.ToOutput(value);
            }
        }

        private static Regex ErrorMessageFormat = new Regex("Msg \\d+, Level \\d+, State \\d+,", RegexOptions.Compiled);

        private bool ParseMessageForError(string message)
        {
            var match = ErrorMessageFormat.Match(message);
            return match.Success;
        }

        public override bool RunCommandLine(string executablePath, string configurationPath, string server, string database, string adminUser, string adminPass, string query)
        {
            bool succeeded = true;
            var onStdErrorOutput = new IOEventHandler((o, e) => succeeded = false);
            this.OnStandardError += onStdErrorOutput;
            bool complete = RunProcess(
                executablePath,
                new string[]{
                    "-S " + server, 
                    string.IsNullOrWhiteSpace(adminUser) ? null : "-U " + adminUser, 
                    string.IsNullOrWhiteSpace(adminPass) ? null : "-P " + adminPass, 
                    (database == null) ? null : "-d " + database,
                    string.Format("-Q \"{0}\"", query)
                });
            this.OnStandardError -= onStdErrorOutput;
            return complete && succeeded;
        }
    }
}