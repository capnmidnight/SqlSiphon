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
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

using SqlSiphon.Mapping;
using SqlSiphon.Model;

namespace SqlSiphon.SqlServer
{
    /// <summary>
    /// A base class for building Data Access Layers that connect to MS SQL Server 2005/2008
    /// databases and execute store procedures stored within.
    /// </summary>
    public class SqlServerDataAccessLayer : SqlSiphon<SqlConnection, SqlCommand, SqlParameter, SqlDataAdapter, SqlDataReader>
    {
        public override string DataSource => Connection.DataSource;
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
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = server
            };

            builder.IntegratedSecurity = true;
            if (!string.IsNullOrWhiteSpace(userName)
                && !string.IsNullOrWhiteSpace(password))
            {
                builder.IntegratedSecurity = false;
                builder.UserID = userName.Trim();
                builder.Password = password.Trim();
            };

            if (database is object)
            {
                builder.InitialCatalog = database;
            }

            return builder.ConnectionString;
        }

        protected override string IdentifierPartBegin => "[";
        protected override string IdentifierPartEnd => "]";
        public override string DefaultSchemaName => "dbo";

        private const SqlServerOptions STANDARD_OPTIONS = SqlServerOptions.ANSI_WARNINGS | SqlServerOptions.ANSI_PADDING | SqlServerOptions.ANSI_NULLS | SqlServerOptions.ARITHABORT | SqlServerOptions.QUOTED_IDENTIFIER | SqlServerOptions.ANSI_NULL_DFLT_ON | SqlServerOptions.CONCAT_NULL_YIELDS_NULL;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query = @"select cast(@@OPTIONS as int) as Options")]
        public SqlServerOptions GetSqlServerOptions()
        {
            return (SqlServerOptions)Get<int>(0);
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            try
            {
                var options = GetSqlServerOptions();
                var allOptions = Enum.GetValues(typeof(SqlServerOptions))
                    .Cast<SqlServerOptions>()
                    .ToArray();

                foreach (var option in allOptions)
                {
                    if ((STANDARD_OPTIONS & option) != SqlServerOptions.None
                        && (options & option) == SqlServerOptions.None)
                    {
                        Execute($"SET {option} ON;");
                    }
                }
            }
            catch
            {
                // not going to care about the error
            }
        }

        private static readonly Dictionary<string, Type> stringToType = new Dictionary<string, Type>();
        private static readonly Dictionary<Type, string> typeToString = new Dictionary<Type, string>();
        private static readonly Dictionary<string, int> defaultTypePrecisions = new Dictionary<string, int>();


        private static void SetTypeMappings<T>(string name, int? defaultPrecision = null)
        {
            SetTypeMappings(typeof(T), name, defaultPrecision);
        }

        private static void SetTypeMappings(Type type, string name, int? defaultPrecision = null)
        {
            if (!stringToType.ContainsKey(name)) { stringToType.Add(name, type); }
            if (!typeToString.ContainsKey(type)) { typeToString.Add(type, name); }
            if (defaultPrecision.HasValue && !defaultTypePrecisions.ContainsKey(name)) { defaultTypePrecisions.Add(name, defaultPrecision.Value); }
        }

        static SqlServerDataAccessLayer()
        {
            SetTypeMappings<string>("nvarchar", 0);
            SetTypeMappings<string>("char");
            SetTypeMappings<string>("varchar");
            SetTypeMappings<string>("text");
            SetTypeMappings<string>("nchar");
            SetTypeMappings<string>("ntext");


            SetTypeMappings<int>("int", 10);
            SetTypeMappings<int?>("int");
            SetTypeMappings<uint>("int");
            SetTypeMappings<uint?>("int");

            SetTypeMappings<float>("real", 24);
            SetTypeMappings<float?>("real");

            SetTypeMappings<double>("float", 53);
            SetTypeMappings<double?>("float", 53);

            SetTypeMappings<DateTime>("datetime2", 27);
            SetTypeMappings<DateTime?>("datetime2");
            SetTypeMappings<DateTime>("datetime");
            SetTypeMappings<DateTime?>("datetime");
            SetTypeMappings<DateTime>("date");
            SetTypeMappings<DateTime?>("date");
            SetTypeMappings<DateTime>("time");
            SetTypeMappings<DateTime?>("time");
            SetTypeMappings<DateTime>("timestamp");
            SetTypeMappings<DateTime?>("timestamp");
            SetTypeMappings<DateTime>("smalldatetime");
            SetTypeMappings<DateTime?>("smalldatetime");
            SetTypeMappings<DateTime>("datetimeoffset");
            SetTypeMappings<DateTime?>("datetimeoffset");


            SetTypeMappings<long>("bigint", 19);
            SetTypeMappings<long?>("bigint");
            SetTypeMappings<ulong>("bigint");
            SetTypeMappings<ulong?>("bigint");

            SetTypeMappings<short>("smallint");
            SetTypeMappings<short?>("smallint");
            SetTypeMappings<ushort>("smallint");
            SetTypeMappings<ushort?>("smallint");
            SetTypeMappings<char>("smallint");
            SetTypeMappings<char?>("smallint");

            SetTypeMappings<byte>("tinyint");
            SetTypeMappings<byte?>("tinyint");
            SetTypeMappings<sbyte>("tinyint");
            SetTypeMappings<sbyte?>("tinyint");

            SetTypeMappings<char[]>("nchar");

            SetTypeMappings<decimal>("decimal");
            SetTypeMappings<decimal?>("decimal");
            SetTypeMappings<decimal>("numeric");
            SetTypeMappings<decimal?>("numeric");
            SetTypeMappings<decimal>("money");
            SetTypeMappings<decimal?>("money");
            SetTypeMappings<decimal>("smallmoney");
            SetTypeMappings<decimal?>("smallmoney");

            SetTypeMappings<bool>("bit");
            SetTypeMappings<bool?>("bit");

            SetTypeMappings<Guid>("uniqueidentifier");
            SetTypeMappings<Guid?>("uniqueidentifier");

            SetTypeMappings<byte[]>("varbinary");
            SetTypeMappings<byte[]>("binary");
            SetTypeMappings<byte[]>("image");
        }

        public static string MakeUDTTName(Type t)
        {
            if (t is null)
            {
                throw new ArgumentNullException(nameof(t));
            }

            if (t.IsArray)
            {
                t = t.GetElementType();
            }

            var attr = DatabaseObjectAttribute.GetAttribute(t) as SqlServerTableAttribute;
            return (attr?.Name ?? t.Name) + "UDTT";
        }

        public override int DefaultTypePrecision(string typeName, int testPrecision)
        {
            if (!defaultTypePrecisions.ContainsKey(typeName))
            {
                throw new Exception($"I don't know the default precision for type `{typeName}`. Perhaps it is {testPrecision}?");
            }
            return defaultTypePrecisions[typeName];
        }

        public override string MakeRoutineIdentifier(RoutineAttribute routine)
        {
            if (routine is null)
            {
                throw new ArgumentNullException(nameof(routine));
            }

            return MakeIdentifier(routine.Schema ?? DefaultSchemaName, routine.Name);
        }

        public override string MakeDropRoutineScript(RoutineAttribute info)
        {
            var routineName = MakeRoutineIdentifier(info);
            return $"drop procedure {routineName};";
        }

        public override string MakeRoutineBody(RoutineAttribute info)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (info.SystemType != null)
            {
                _ = DatabaseObjectAttribute.GetAttribute(DataConnector.CoalesceCollectionType(info.SystemType));
            }

            var query = info.Query.Replace("into returnValue", "")
                .Replace("return query ", "")
                .Replace("perform ", "select ");
            if (info.EnableTransaction)
            {
                var schemaName = info.Schema ?? DefaultSchemaName;
                var transactionName = $"TRANS{schemaName}{info.Name}";
                var len = transactionName.Length;
                if (len > 32)
                {
                    var logLen = (int)Math.Ceiling(Math.Log10(len));
                    transactionName = transactionName.Substring(0, 32 - logLen)
                        + logLen.ToString(CultureInfo.InvariantCulture);
                }
                var transactionBegin = $@"begin try
    begin transaction {transactionName};";
                var transactionEnd = $@"commit transaction {transactionName};
end try
begin catch
    declare @msg nvarchar(4000), @lvl int, @stt int;
    select @msg = error_message(), @lvl = error_severity(), @stt = error_state();
    rollback transaction {transactionName};
    raiserror(@msg, @lvl, @stt);
end catch;";
                query = string.Join(Environment.NewLine, transactionBegin, query, transactionEnd);
            }
            var identifier = MakeIdentifier(info.Schema ?? DefaultSchemaName, info.Name);
            var parameterSection = MakeParameterSection(info);
            var withRecompile = info.GetOtherAttribute<SqlServerWithRecompileAttribute>() != null;
            var withRecompileStatement = withRecompile ? "with recompile\r\n" : "";
            return $@"create procedure {identifier}
    {parameterSection}
{withRecompileStatement}as begin
    set nocount on;
    {query}
end";
        }

        public override string MakeCreateRoutineScript(RoutineAttribute info, bool createBody = true)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            return createBody ? MakeRoutineBody(info) : info.Query;
        }

        public override void AnalyzeQuery(string routineText, RoutineAttribute routine)
        {
            if (routineText is null)
            {
                throw new ArgumentNullException(nameof(routineText));
            }

            if (routine is null)
            {
                throw new ArgumentNullException(nameof(routine));
            }

            base.AnalyzeQuery(routineText, routine);
            routine.EnableTransaction = routineText.IndexOf("begin transaction TRANS", StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        public override string MakeCreateTableScript(TableAttribute info)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = MakeIdentifier(schema, info.Name);
            var columnSection = MakeColumnSection(info, false);
            return $@"create table {identifier}(
    {columnSection}
);";
        }

        public string MakeCreateUDTTScript(TableAttribute info)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var columns = info.Properties
                .Where(p => p.Include
                && !p.IsIdentity
                && (p.DefaultValue == null || p.IsIncludeSet))
                .ToArray();
            if (columns.Length == 0)
            {
                throw new TableHasNoColumnsException(info);
            }
            var columnSection = ArgumentList(columns, p => MakeColumnString(p, false).Trim());

            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = MakeIdentifier(schema, info.Name);
            return $@"create type {identifier} as table(
    {columnSection}
);";
        }

        public override string MakeDropTableScript(TableAttribute info)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = MakeIdentifier(schema, info.Name);
            return $@"drop table {identifier};";
        }

        internal string MakeDropUDTTScript(TableAttribute info)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = MakeIdentifier(schema, info.Name);
            return $@"drop type {identifier};";
        }

        public override string MakeCreateIndexScript(TableIndex idx)
        {
            if (idx is null)
            {
                throw new ArgumentNullException(nameof(idx));
            }

            var indexName = MakeIdentifier(idx.Name);
            var columnSection = string.Join(",", idx.Columns.Select(c => MakeIdentifier(c)));
            var tableName = MakeIdentifier(idx.Table.Schema ?? DefaultSchemaName, idx.Table.Name);
            var clusterType = idx.IsClustered ? "" : "non";
            return $@"create {clusterType}clustered index {indexName} on {tableName}({columnSection});";
        }

        public override string MakeDropIndexScript(TableIndex idx)
        {
            if (idx is null)
            {
                throw new ArgumentNullException(nameof(idx));
            }

            var tableName = MakeIdentifier(idx.Table.Schema ?? DefaultSchemaName, idx.Table.Name);
            return $@"drop index {idx.Name} on {tableName};";
        }

        private static string GetDefaultValue(DatabaseObjectAttribute p)
        {
            if (p is null)
            {
                throw new ArgumentNullException(nameof(p));
            }

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
            if (p is null)
            {
                throw new ArgumentNullException(nameof(p));
            }

            var typeStr = MakeSqlTypeString(p);
            var defaultValue = GetDefaultValue(p);
            var isUDTT = p.IsUDTT || (p.SystemType != null && IsUDTT(p.SystemType));
            var readonlyConstraint = isUDTT ? "readonly" : "";
            return $"@{p.Name} {typeStr} {defaultValue} {readonlyConstraint}".Trim();
        }

        protected override string MakeColumnString(ColumnAttribute p, bool isReturnType)
        {
            if (p is null)
            {
                throw new ArgumentNullException(nameof(p));
            }

            var typeStr = MakeSqlTypeString(p);
            var defaultString = "";
            if (p.DefaultValue != null)
            {
                var defaultValue = GetDefaultValue(p);
                defaultString = $"default ({defaultValue})";
            }
            else if (p.IsIdentity)
            {
                defaultString = "identity(1, 1)";
            }

            var columnName = MakeIdentifier(p.Name);
            var nullConstraint = p.IsOptional ? "" : "NOT NULL";
            return $"{columnName} {typeStr} {nullConstraint} {defaultString}";
        }

        public override string MakeCreateColumnScript(ColumnAttribute prop)
        {
            if (prop is null)
            {
                throw new ArgumentNullException(nameof(prop));
            }

            var tableName = MakeIdentifier(prop.Table.Schema ?? DefaultSchemaName, prop.Table.Name);
            var columnName = MakeIdentifier(prop.Name);
            var columnType = MakeSqlTypeString(prop);
            return $"alter table {tableName} add {columnName} {columnType};";
        }

        public override string MakeDropColumnScript(ColumnAttribute prop)
        {
            if (prop is null)
            {
                throw new ArgumentNullException(nameof(prop));
            }

            var tableName = MakeIdentifier(prop.Table.Schema ?? DefaultSchemaName, prop.Table.Name);
            var columnName = MakeIdentifier(prop.Name);
            return $"alter table {tableName} drop column {columnName};";
        }

        protected override string CheckDefaultValueDifference(ColumnAttribute final, ColumnAttribute initial)
        {
            if (final is null)
            {
                throw new ArgumentNullException(nameof(final));
            }

            if (initial is null)
            {
                throw new ArgumentNullException(nameof(initial));
            }

            var valuesMatch = false;
            if (final.DefaultValue != null && initial.DefaultValue != null)
            {
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
                    // this is junk.
                    valuesMatch = final.DefaultValue == "(" + initial.DefaultValue + ")"
                        || initial.DefaultValue == "(" + final.DefaultValue + ")"
                        || final.DefaultValue == "((" + initial.DefaultValue + "))"
                        || initial.DefaultValue == "((" + final.DefaultValue + "))"
                        || final.DefaultValue == "('" + initial.DefaultValue + "')"
                        || initial.DefaultValue == "('" + final.DefaultValue + "')"; ;
                }
            }

            if (valuesMatch)
            {
                initial.DefaultValue = final.DefaultValue;
                return null;
            }
            else
            {
                return $"Column default value has changed. Was {initial.DefaultValue}, now {final.DefaultValue}.";
            }
        }

        public override string MakeAlterColumnScript(ColumnAttribute final, ColumnAttribute initial)
        {
            if (final is null)
            {
                throw new ArgumentNullException(nameof(final));
            }

            if (initial is null)
            {
                throw new ArgumentNullException(nameof(initial));
            }

            var tableName = MakeIdentifier(final.Table.Schema ?? DefaultSchemaName, final.Table.Name);
            var preamble = $"alter table {tableName}";
            var columnName = MakeIdentifier(final.Name);
            var columnType = MakeSqlTypeString(final);
            var conditionalColumnType = final.Include ? columnType : "";

            if (final.Include != initial.Include)
            {
                var modifier = final.Include ? "add" : "drop column";
                return $"{preamble} {modifier} {columnName} {conditionalColumnType};".Trim();
            }
            else if (final.DefaultValue != initial.DefaultValue)
            {
                var constraintName = MakeIdentifier("DEF_" + final.Name);
                if (final.DefaultValue != null)
                {
                    var defaultValue = final.DefaultValue ?? "";
                    return $"{preamble} add constraint {constraintName} default({defaultValue}) for {final.Name};";
                }
                else
                {
                    return $"{preamble} drop constraint {constraintName};";
                }
            }
            else if (final.IsOptional != initial.IsOptional)
            {
                var nullableType = final.IsOptional ? "" : "not";
                return $"{preamble} alter column {columnName} {columnType} {nullableType} null;";
            }
            else if (final.SystemType != initial.SystemType
                || final.Size != initial.Size
                || final.Precision != initial.Precision)
            {
                return $"{preamble} alter column {columnName} {columnType};";
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
                if (typeToString.ContainsKey(systemType))
                {
                    sqlType = typeToString[systemType];
                }
                else if (IsUDTT(systemType))
                {
                    sqlType = MakeUDTTName(systemType);
                }
            }

            if (sqlType != null)
            {
                if (sqlType[sqlType.Length - 1] == ')') // someone already setup the type name, so skip it
                {
                    return sqlType;
                }
                else
                {
                    var typeStr = new StringBuilder(sqlType);
                    if (size.HasValue && (systemType != typeof(int) || size.Value > 0))
                    {
                        _ = typeStr.AppendFormat("({0}", size);
                        if (precision.HasValue)
                        {
                            _ = typeStr.AppendFormat(", {0}", precision);
                        }
                        _ = typeStr.Append(")");
                    }

                    if (sqlType.Contains("var")
                        && typeStr[typeStr.Length - 1] != ')')
                    {
                        _ = typeStr.Append("(MAX)");
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
            if (typeToString.ContainsKey(attr.SystemType))
            {
                var typeStr = MakeSqlTypeString(attr);
                var nullConstraintType = attr.IsOptional ? "" : "NOT ";
                var defaultValue = !skipDefault ? attr.DefaultValue ?? "" : "";
                return $"{attr.Name} {typeStr} {nullConstraintType}NULL {defaultValue}".Trim();
            }
            return null;
        }

        private static DataTable MakeDataTable(string tableName, Type t, System.Collections.IEnumerable array)
        {
            var table = new DataTable(tableName);
            if (typeToString.ContainsKey(t))
            {
                _ = table.Columns.Add("Value", t);
                foreach (var obj in array)
                {
                    _ = table.Rows.Add(new object[] { obj });
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
                    _ = table.Columns.Add(column.Name, column.SystemType);
                }
                foreach (var obj in array)
                {
                    var row = new List<object>();
                    foreach (var column in columns)
                    {
                        var element = column.GetValue<object>(obj);
                        row.Add(element);
                    }
                    _ = table.Rows.Add(row.ToArray());
                }
            }
            return table;
        }

        public override string MakeCreateRelationshipScript(Relationship relation)
        {
            if (relation is null)
            {
                throw new ArgumentNullException(nameof(relation));
            }

            var fromTableName = MakeIdentifier(relation.From.Schema ?? DefaultSchemaName, relation.From.Name);
            var constraintName = MakeIdentifier(relation.GetRelationshipName(this));
            var fromColumns = string.Join(", ", relation.FromColumns.Select(c => MakeIdentifier(c.Name)));
            var toTableName = MakeIdentifier(relation.To.Schema ?? DefaultSchemaName, relation.To.Name);
            var toColumns = string.Join(", ", relation.To.PrimaryKey.KeyColumns.Select(c => MakeIdentifier(c.Name)));

            return $@"alter table {fromTableName} add constraint {constraintName}
    foreign key({fromColumns})
    references {toTableName}({toColumns});";
        }

        public override string MakeDropRelationshipScript(Relationship relation)
        {
            if (relation is null)
            {
                throw new ArgumentNullException(nameof(relation));
            }

            var tableName = MakeIdentifier(relation.From.Schema ?? DefaultSchemaName, relation.From.Name);
            var constraintName = MakeIdentifier(relation.GetRelationshipName(this));
            return $@"alter table {tableName} drop constraint {constraintName}";
        }

        public override string MakeDropPrimaryKeyScript(PrimaryKey key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var tableName = MakeIdentifier(key.Table.Schema ?? DefaultSchemaName, key.Table.Name);
            var constraintName = base.MakeIdentifier(key.Name);
            return $@"alter table {tableName} drop constraint {constraintName};";
        }

        public override string MakeCreatePrimaryKeyScript(PrimaryKey key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var tableName = MakeIdentifier(key.Table.Schema ?? DefaultSchemaName, key.Table.Name);
            var constraintName = MakeIdentifier(key.Name);
            var keys = string.Join(", ", key.KeyColumns.Select(c => MakeIdentifier(c.Name)));
            return $@"alter table {tableName} add constraint {constraintName} primary key({keys});";
        }


        public override bool DescribesIdentity(InformationSchema.Columns column)
        {
            if (column is null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            return column.is_identity == 1;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query = @"select name from sys.sysusers")]
        public override List<string> GetDatabaseLogins()
        {
            return GetList<string>();
        }

        public override string MakeCreateDatabaseLoginScript(string userName, string password, string database)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("Must provide a user name", nameof(userName));
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Must provide a password", nameof(password));
            }

            if (string.IsNullOrEmpty(database))
            {
                throw new ArgumentException("Must provide a database name", nameof(database));
            }

            return $@"create login {userName} with password = '{password}', default_database = {database};
use {database};
create user {userName} for login {userName};
alter user {userName} with default_schema = dbo;
alter role db_owner add member {userName};";
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select schema_name from information_schema.schemata where schema_name not like 'db_%' and schema_name not in ('information_schema', 'dbo', 'guest', 'sys');")]
        public override List<string> GetSchemata()
        {
            return GetList<string>();
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
            var columns = GetEnumerator<InformationSchema.Columns>()
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
            var columns = GetEnumerator<InformationSchema.Columns>()
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
                while (column.column_default.StartsWith("(", StringComparison.InvariantCultureIgnoreCase))
                {
                    column.column_default = column.column_default.Substring(1);
                }
                while (column.column_default.EndsWith(")", StringComparison.InvariantCultureIgnoreCase))
                {
                    column.column_default = column.column_default.Substring(0, column.column_default.Length - 1);
                }
                if (column.column_default.EndsWith("(", StringComparison.InvariantCultureIgnoreCase))
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
	col.name as column_name,
	ind.is_primary_key,
	ind.is_unique,
	ind.is_unique_constraint
FROM sys.indexes ind
INNER JOIN sys.index_columns ic ON ind.object_id = ic.object_id and ind.index_id = ic.index_id 
INNER JOIN sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id 
INNER JOIN sys.tables t ON ind.object_id = t.object_id 
INNER JOIN sys.schemas s on t.schema_id = s.schema_id
WHERE 
     t.is_ms_shipped = 0 
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
where constraint_schema != 'information_schema'
order by ordinal_position;")]
        public override List<InformationSchema.KeyColumnUsage> GetKeyColumns()
        {
            return GetList<InformationSchema.KeyColumnUsage>();
        }

        /// <summary>
        /// Implements bulk-insert capabilities for MS SQL Server.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public override void InsertAll(Type t, System.Collections.IEnumerable data)
        {
            if (t != null && data != null)
            {
                var attr = DatabaseObjectAttribute.GetAttribute(t);
                if (attr == null)
                {
                    throw new Exception($"Type {t.Namespace}.{t.Name} could not be automatically inserted.");
                }
                using var tableData = MakeDataTable(attr.Name, t, data);

                //should make it using bulk insert when mono-project fix it for varbinary data
                //see https://bugzilla.xamarin.com/show_bug.cgi?id=20563
                var usesVarBinary = tableData.Columns.Cast<DataColumn>().Any(c => c.DataType == typeof(byte[]));
                if (IsOnMonoRuntime && usesVarBinary)
                {
                    base.InsertAll(t, data);
                }
                else
                {
                    if (Connection.State == ConnectionState.Closed)
                    {
                        Connection.Open();
                    }
                    using var bulkCopy = new SqlBulkCopy(Connection);
                    foreach (var column in tableData.Columns.Cast<DataColumn>())
                    {
                        _ = bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }
                    bulkCopy.DestinationTableName = attr.Name;
                    bulkCopy.WriteToServer(tableData);
                }
            }
        }

        public override Type GetSystemType(string sqlType)
        {
            return stringToType.ContainsKey(sqlType) ? stringToType[sqlType] : null;
        }

        public override DatabaseState GetInitialState(string catalogueName, Regex filter)
        {
            var state = base.GetInitialState(catalogueName, filter);
            var ssState = new SqlServerDatabaseState(state);
            if (ssState.CatalogueExists.HasValue && ssState.CatalogueExists.Value)
            {
                var udttColumns = GetUDTTColumns().ToHash(col => MakeIdentifier(col.table_schema, col.table_name));
                foreach (var udtt in udttColumns)
                {
                    ssState.UDTTs.Add(udtt.Key.ToLowerInvariant(), new TableAttribute(udtt.Value, this));
                }
            }
            return ssState;
        }

        public override DatabaseState GetFinalState(Type dalType, string userName, string password, string database)
        {
            var state = base.GetFinalState(dalType, userName, password, database);
            var ssState = new SqlServerDatabaseState(state);
            var types = new HashSet<Type>();
            foreach (var parameter in ssState.Functions.Values.SelectMany(f => f.Parameters))
            {
                if (IsUDTT(parameter.SystemType))
                {
                    _ = types.Add(parameter.SystemType);
                }
            }

            foreach (var type in types)
            {
                var attr = MakeUDTTTableAttribute(type);
                ssState.AddTable(ssState.UDTTs, DataConnector.IsTypePrimitive(type) ? null : type, this, attr);
            }
            return ssState;
        }

        public TableAttribute MakeUDTTTableAttribute(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var elementType = type;
            if (elementType.IsArray)
            {
                elementType = elementType.GetElementType();
            }
            TableAttribute attr;
            if (DataConnector.IsTypePrimitive(elementType))
            {
                attr = new TableAttribute();
                var valueColumn = new ColumnAttribute
                {
                    Table = attr,
                    Name = "Value"
                };
                valueColumn.SetSystemType(elementType);
                attr.Properties.Add(valueColumn);
            }
            else
            {
                attr = DatabaseObjectAttribute.GetAttribute(elementType);
            }
            attr.Name = $"{elementType.Name}UDTT";
            return attr;
        }

        public static bool IsUDTT(Type t)
        {
            if (t is null)
            {
                throw new ArgumentNullException(nameof(t));
            }

            var isUDTT = false;
            if (t.IsArray)
            {
                t = t.GetElementType();
                isUDTT = t != typeof(byte) && DataConnector.IsTypePrimitive(t);
            }
            var attr = Mapping.DatabaseObjectAttribute.GetAttribute(t) as SqlServerTableAttribute;
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

        private static readonly Regex ErrorMessageFormat = new Regex("Msg \\d+, Level \\d+, State \\d+,", RegexOptions.Compiled);

        private bool ParseMessageForError(string message)
        {
            var match = ErrorMessageFormat.Match(message);
            return match.Success;
        }

        public override bool RunCommandLine(string executablePath, string configurationPath, string server, string database, string adminUser, string adminPass, string query)
        {
            var succeeded = true;
            var onStdErrorOutput = new IOEventHandler((o, e) => succeeded = false);
            OnStandardError += onStdErrorOutput;
            var complete = RunProcess(
                executablePath,
                new string[]{
                    "-S " + server,
                    string.IsNullOrWhiteSpace(adminUser) ? null : "-U " + adminUser,
                    string.IsNullOrWhiteSpace(adminPass) ? null : "-P " + adminPass,
                    (database == null) ? null : "-d " + database,
                    $"-Q \"{query}\""
                });
            OnStandardError -= onStdErrorOutput;
            return complete && succeeded;
        }

        public override string MakeInsertScript(TableAttribute table, object value)
        {
            if (table is null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            var columns = table.Properties
                .Where(p => p.Include && !p.IsIdentity && (p.IsIncludeSet || p.DefaultValue == null))
                .ToArray();

            var columnNames = columns.Select(c => c.Name).ToArray();
            var columnValues = columns.Select(c =>
            {
                var v = c.GetValue(value);
                string val = null;
                if (v == null)
                {
                    value = "NULL";
                }
                else
                {
                    var t = v.GetType();
                    if(t == typeof(string))
                    {
                        v = ((string)v).Replace("'", "''");
                    }

                    if (DataConnector.IsTypeBarePrimitive(t))
                    {
                        val = v.ToString();
                    }
                    else if (DataConnector.IsTypeQuotedPrimitive(t))
                    {
                        val = $"'{v}'";
                    }
                    else
                    {
                        throw new Exception("Can't insert value");
                    }
                }
                return val;
            }).ToArray();

            var tableName = MakeIdentifier(table.Schema ?? DefaultSchemaName, table.Name);
            var columnNameList = string.Join(", ", columnNames);
            var columnValueList = string.Join(", ", columnValues);
            return $"insert into {tableName}({columnNameList}) values({columnValueList});";
        }
    }
}