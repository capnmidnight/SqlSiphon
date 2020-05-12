using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using Npgsql;

using SqlSiphon.Mapping;
using SqlSiphon.Model;
using SqlSiphon.Postgres.PgCatalog;

namespace SqlSiphon.Postgres
{
    /// <summary>
    /// A base class for building Data Access Layers that connect to MySQL
    /// databases and execute store procedures stored within.
    /// </summary>
    public class PostgresDataAccessLayer : SqlSiphon<NpgsqlConnection, NpgsqlCommand, NpgsqlParameter, NpgsqlDataAdapter, NpgsqlDataReader>
    {
        public override string DataSource => Connection.DataSource;

        private static readonly Dictionary<string, Type> stringToType = new Dictionary<string, Type>
        {
            ["int8"] = typeof(long),
            ["bigint"] = typeof(long),
            ["bigserial"] = typeof(long),
            ["serial8"] = typeof(long),

            ["bit"] = typeof(bool[]),
            ["varbit"] = typeof(bool[]),
            ["bit varying"] = typeof(bool[]),

            ["boolean"] = typeof(bool),
            ["bool"] = typeof(bool),

            ["bytea"] = typeof(byte[]),

            ["text"] = typeof(string),
            ["character varying"] = typeof(string),
            ["varchar"] = typeof(string),
            ["tsquery"] = typeof(string),
            ["xml"] = typeof(string),
            ["json"] = typeof(string),
            ["name"] = typeof(string),
            ["character"] = typeof(string),
            ["char"] = typeof(string),

            ["inet"] = typeof(System.Net.IPAddress),
            ["cidr"] = typeof(string),

            ["date"] = typeof(DateTime),
            ["datetime"] = typeof(DateTime), // included for tranlating T-SQL to PG/PSQL
            ["datetime2"] = typeof(DateTime), // included for tranlating T-SQL to PG/PSQL

            ["float8"] = typeof(double),
            ["double precision"] = typeof(double),

            ["float4"] = typeof(float),
            ["real"] = typeof(float),

            ["integer"] = typeof(int),
            ["int4"] = typeof(int),
            ["int"] = typeof(int),
            ["serial"] = typeof(int),
            ["serial4"] = typeof(int),

            ["int2"] = typeof(short),
            ["smallint"] = typeof(short),
            ["smallserial"] = typeof(short),
            ["serial2"] = typeof(short),

            ["interval"] = typeof(TimeSpan),

            ["money"] = typeof(decimal),
            ["numeric"] = typeof(decimal),

            ["time"] = typeof(DateTime),
            ["time with time zone"] = typeof(DateTime),
            ["timestamp"] = typeof(DateTime),
            ["timestamp with time zone"] = typeof(DateTime),

            ["uuid"] = typeof(Guid),
            ["uniqueidentifier"] = typeof(Guid), // included for tranlating T-SQL to PG/PSQL

            ["box"] = typeof(NpgsqlTypes.NpgsqlBox),
            ["circle"] = typeof(NpgsqlTypes.NpgsqlCircle),
            ["lseg"] = typeof(NpgsqlTypes.NpgsqlLSeg),
            ["macaddr"] = typeof(string),
            ["path"] = typeof(NpgsqlTypes.NpgsqlPath),
            ["point"] = typeof(NpgsqlTypes.NpgsqlPoint),
            ["polygon"] = typeof(NpgsqlTypes.NpgsqlPolygon),
            ["geometry"] = typeof(string),
            ["geography"] = typeof(string),
            ["tsvector"] = typeof(string)
        };

        private static readonly Dictionary<string, int> defaultTypePrecisions = new Dictionary<string, int>
        {
            ["float8"] = 53,
            ["double precision"] = 53,
            ["money"] = 64,
            ["int8"] = 64,
            ["bigint"] = 64,
            ["bigserial"] = 64,
            ["serial8"] = 64,
            ["float4"] = 24,
            ["real"] = 24,
            ["integer"] = 32,
            ["int"] = 32,
            ["int4"] = 32,
            ["serial"] = 32,
            ["serial4"] = 32,
            ["bool"] = 32,
            ["boolean"] = 32,
            ["smallint"] = 16,
            ["int2"] = 16,
            ["smallserial"] = 16,
            ["serial2"] = 16
        };

        private static readonly Dictionary<Type, int> defaultTypeSizes = TypeInfo.typeSizes.ToDictionary(kv => kv.Key, kv => kv.Value);

        private static readonly Dictionary<Type, string> typeToString = stringToType
                .GroupBy(kv => kv.Value, kv => kv.Key)
                .ToDictionary(g => g.Key, g => g.First());

        static PostgresDataAccessLayer()
        {
            typeToString.Add(typeof(int?), "int");
            typeToString.Add(typeof(uint), "int");
            typeToString.Add(typeof(uint?), "int");

            typeToString.Add(typeof(long?), "bigint");
            typeToString.Add(typeof(ulong), "bigint");
            typeToString.Add(typeof(ulong?), "bigint");

            typeToString.Add(typeof(short?), "smallint");
            typeToString.Add(typeof(ushort), "smallint");
            typeToString.Add(typeof(ushort?), "smallint");
            typeToString.Add(typeof(byte), "smallint");
            typeToString.Add(typeof(byte?), "smallint");
            typeToString.Add(typeof(sbyte), "smallint");
            typeToString.Add(typeof(sbyte?), "smallint");
            typeToString.Add(typeof(char), "smallint");
            typeToString.Add(typeof(char?), "smallint");

            typeToString.Add(typeof(decimal?), "decimal");
            typeToString.Add(typeof(bool?), "boolean");
            typeToString.Add(typeof(float?), "real");
            typeToString.Add(typeof(double?), "double precision");
            typeToString.Add(typeof(DateTime?), "time with time zone");
            typeToString.Add(typeof(Guid?), "uuid");

            defaultTypeSizes[typeof(bool)] = 4;
            defaultTypeSizes[typeof(bool?)] = 4;
        }

        /// <summary>
        /// creates a new connection to a Postgress database and automatically
        /// opens the connection. 
        /// </summary>
        /// <param name="connectionString"></param>
        public PostgresDataAccessLayer(string connectionString)
            : base(connectionString)
        {
        }

        public PostgresDataAccessLayer(NpgsqlConnection connection)
            : base(connection)
        {
        }

        public PostgresDataAccessLayer(PostgresDataAccessLayer dal)
            : base(dal?.Connection)
        {
        }

        public override bool RunCommandLine(string executablePath, string configurationPath, string server, string database, string adminUser, string adminPass, string query)
        {
            /// This is no longer necessary. It's only here to satisfy the interface.
            return false;
        }


        protected override string IdentifierPartBegin => "\"";
        protected override string IdentifierPartEnd => "\"";
        public override string DefaultSchemaName => "public";
        public override int GetDefaultTypePrecision(string typeName, int testSize)
        {
            if (!defaultTypePrecisions.ContainsKey(typeName))
            {
                throw new Exception($"I don't know the default type size for `{typeName}`. Perhaps it is {testSize}?\n\ndefaultTypeSizes.Add(\"{typeName}\", {testSize});\n\n");
            }
            return defaultTypePrecisions[typeName];
        }

        public override bool HasDefaultTypeSize(Type type)
        {
            return type != null
                && (defaultTypeSizes.ContainsKey(type)
                    || type.IsEnum
                        && defaultTypeSizes.ContainsKey(type.GetEnumUnderlyingType()));
        }

        public override int GetDefaultTypeSize(Type type)
        {
            if (!HasDefaultTypeSize(type))
            {
                throw new Exception($"I don't know the default precision for type `{type}`. Perhaps it is {Marshal.SizeOf(type)}?");
            }

            if (type.IsEnum)
            {
                type = type.GetEnumUnderlyingType();
            }

            return defaultTypeSizes[type];
        }

        protected override void JiggerParameter(NpgsqlParameter p, bool isProc)
        {
            if (isProc)
            {
                p.ParameterName = "_" + p.ParameterName.ToLowerInvariant();
                if (p.Value is DateTime)
                {
                    p.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Date;
                }
            }

            base.JiggerParameter(p, isProc);
        }

        public override string NormalizeSqlType(string sqlType)
        {
            if (sqlType is object)
            {
                sqlType = sqlType.ToLowerInvariant();
                if (stringToType.ContainsKey(sqlType))
                {
                    var type = stringToType[sqlType];
                    if (typeToString.ContainsKey(type))
                    {
                        sqlType = typeToString[type];
                    }
                }
            }

            return sqlType;
        }

        public override string MakeIdentifier(params string[] parts)
        {
            var goodParts = parts.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
            for (var i = 0; i < goodParts.Length; ++i)
            {
                if (goodParts[i].Length > 63)
                {
                    var len = goodParts[i].Length.ToString(CultureInfo.InvariantCulture);
                    var lengthLength = len.Length;
                    goodParts[i] = goodParts[i].Substring(0, 63 - lengthLength) + len;
                }
            }
            return base.MakeIdentifier(goodParts).ToLowerInvariant();
        }

        public override Type GetSystemType(string sqlType)
        {
            return stringToType.ContainsKey(sqlType) ? stringToType[sqlType] : null;
        }

        public override DatabaseState GetInitialState(string catalogueName, Regex filter)
        {
            var state = base.GetInitialState(catalogueName, filter);
            var pgState = new PostgresDatabaseState(state);

            if (pgState.CatalogueExists == true)
            {
                GetExtensions().ForEach(pgState.AddExtension);
                GetUserSettings().ForEach(pgState.AddUserSettings);
            }

            return pgState;
        }

        public override DatabaseState GetFinalState(Type dalType, string userName, string password, string database)
        {
            var state = base.GetFinalState(dalType, userName, password, database);
            var pgState = new PostgresDatabaseState(state);

            pgState.DatabaseLogins.Add("postgres", null);

            if (pgState.TypeExists<Guid>())
            {
                pgState.AddExtension("uuid-ossp", "1.0");
            }

            pgState.AddExtension("postgis", "2.1.1");

            pgState.Schemata.AddRange(pgState.Extensions.Keys.Where(e => !pgState.Schemata.Contains(e)));

            var schemata = pgState.Schemata
                .Select(s => MakeIdentifier(s))
                .Prepend("public");
            var searchPath = string.Join(",", schemata);
            foreach (var uName in pgState.DatabaseLogins.Keys)
            {
                pgState.AddUserSetting(uName, "search_path", searchPath);
            }

            foreach (var table in pgState.Tables.Values)
            {
                foreach (var column in table.Properties)
                {
                    if (!column.IsStringLengthSet
                        && HasDefaultTypeSize(column.SystemType))
                    {
                        column.StringLength = GetDefaultTypeSize(column.SystemType);
                    }
                }
            }

            return pgState;
        }

        protected override string MakeSqlTypeString(string sqlType, Type systemType, int? size, int? precision, bool isIdentity, bool skipSize, bool isArray)
        {
            string typeName = null;

            if (sqlType != null)
            {
                typeName = sqlType;
            }
            else if (systemType != null)
            {
                if (typeToString.ContainsKey(systemType))
                {
                    typeName = typeToString[systemType];
                }

                if (typeName == null)
                {
                    typeName = MakeComplexSqlTypeString(systemType);
                    if (typeName == null && systemType.Name != "Void")
                    {
                        throw new Exception($"Couldn't find type description for type: {systemType.FullName}");
                    }
                }
            }

            if (size.HasValue && typeName.IndexOf("(", StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                var sizeStr = precision.HasValue
                    ? $"({size},{precision})"
                    : $"({size})";

                var bracketsIndex = typeName.IndexOf("[]", StringComparison.InvariantCultureIgnoreCase);
                if (bracketsIndex > -1)
                {
                    isArray = true;
                    typeName = typeName.Substring(0, bracketsIndex);
                }
                if (typeName == "text")
                {
                    typeName = "varchar";
                }
                if (!skipSize)
                {
                    typeName += sizeStr;
                }
            }

            if (isArray)
            {
                typeName += "[]";
            }

            return typeName;
        }

        public override bool DescribesIdentity(InformationSchema.Column column)
        {
            if (column is null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            if (column.column_default != null && column.column_default.IndexOf("nextval", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                column.column_default = null;
                column.udt_name = "integer";
                return true;
            }
            return false;
        }

        public override bool SupportsScriptType(ScriptType type)
        {
            return true;
        }

        public override string MakeCreateColumnScript(ColumnAttribute prop)
        {
            if (prop is null)
            {
                throw new ArgumentNullException(nameof(prop));
            }

            var tableName = MakeIdentifier(prop.Table.Schema ?? DefaultSchemaName, prop.Table.Name);
            var columnName = MakeIdentifier(prop.Name);
            var columnType = MakeSqlTypeString(prop, true, false);
            return $"alter table if exists {tableName} add column {columnName} {columnType};";
        }

        public override string MakeDropColumnScript(ColumnAttribute prop)
        {
            if (prop is null)
            {
                throw new ArgumentNullException(nameof(prop));
            }

            var tableName = MakeIdentifier(prop.Table.Schema ?? DefaultSchemaName, prop.Table.Name);
            var columnName = MakeIdentifier(prop.Name);
            return $"alter table if exists {tableName} drop column if exists {columnName};";
        }

        public override string RoutineChanged(RoutineAttribute a, RoutineAttribute b)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b is null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            var typeA = MakeSqlTypeString(a, true, false) ?? "void";
            var isArray = typeA.EndsWith("[]", StringComparison.InvariantCulture);
            if (isArray)
            {
                typeA = typeA.Substring(0, typeA.Length - 2);
                typeA = NormalizeSqlType(typeA);
                typeA += "[]";
            }
            var changedReturnType = typeA != b.SqlType;
            return changedReturnType ? "Return type changed" : base.RoutineChanged(a, b);
        }

        public override bool IsUserDefinedType(Type systemType)
        {
            var baseType = DataConnector.CoalesceCollectionType(systemType);
            return baseType != typeof(void)
                && !typeToString.ContainsKey(baseType)
                && DatabaseObjectAttribute.GetTable(baseType) == null;
        }


        public override TableAttribute MakeUserDefinedTypeTableAttribute(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.IsArray)
            {
                type = type.GetElementType();
                if (type is null)
                {
                    throw new ArgumentException($"Array {nameof(type)} Element Type is null");
                }
            }

            var attr = DatabaseObjectAttribute.GetTable(type);
            attr.Name = $"{type.Name}UserDefinedType";
            return attr;
        }

        private string MakeComplexSqlTypeString(Type systemType)
        {
            var baseType = DataConnector.CoalesceCollectionType(systemType);

            string sqlType;
            if (baseType == typeof(void))
            {
                sqlType = "void";
            }
            else if (typeToString.ContainsKey(baseType))
            {
                sqlType = typeToString[baseType];
            }
            else
            {
                var attr = DatabaseObjectAttribute.GetTable(baseType)
                    ?? new TableAttribute(baseType);
                sqlType = MakeIdentifier(attr.Schema ?? DefaultSchemaName, attr.Name);
            }

            if (DataConnector.IsTypeCollection(systemType))
            {
                sqlType += "[]";
            }

            return sqlType;
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
            var preamble = $"alter table if exists {tableName}";
            var columnName = MakeIdentifier(final.Name);
            var columnType = MakeSqlTypeString(final, true, false);

            if (final.Include != initial.Include)
            {
                var conditionalColumnType = final.Include ? columnType : "";
                var op = final.Include ? "add" : "drop";
                return $"{preamble} {op} column {columnName} {conditionalColumnType};"
                    .Trim();
            }
            else if (final.DefaultValue != initial.DefaultValue)
            {
                var op = final.DefaultValue == null ? "drop" : "set";
                var defaultValue = final.DefaultValue ?? "";
                return $"{preamble} alter column {columnName} {op} default {defaultValue};"
                    .Trim();
            }
            else if (final.IsOptional != initial.IsOptional)
            {
                var op = final.IsOptional ? "drop" : "set";
                return $"{preamble} alter column {columnName} {op} not null;";
            }
            else if (final.SystemType != initial.SystemType
                || final.StringLength != initial.StringLength
                || final.NumericPrecision != initial.NumericPrecision
                || final.IsIdentity != initial.IsIdentity)
            {
                return $"{preamble} alter column {columnName} set data type {columnType};";
            }
            else
            {
                // by this point, the columns should be identical, but we still need a base case
                return null;
            }
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

            bool valuesMatch;
            if (final.SystemType == typeof(bool))
            {
                var xb = final.DefaultValue.Replace("'", "").ToLowerInvariant();
                var xbb = xb == "true" || xb == "1";
                var yb = initial.DefaultValue.Replace("'", "").ToLowerInvariant();
                var ybb = yb == "true" || yb == "1";
                valuesMatch = xbb == ybb;
            }
            else if (final.SystemType == typeof(DateTime))
            {
                valuesMatch = (final.DefaultValue == "'9999/12/31 23:59:59.99999'"
                        && initial.DefaultValue == "'9999-12-31'::date")
                    || (final.DefaultValue == "getdate()"
                        && (initial.DefaultValue == "('now'::text)::date"
                            || initial.DefaultValue == "CURRENT_DATE"));
            }
            else if (final.SystemType == typeof(double))
            {
                valuesMatch = final.DefaultValue == "(" + initial.DefaultValue + ")"
                    || initial.DefaultValue == "(" + final.DefaultValue + ")";
            }
            else
            {
                valuesMatch = (final.DefaultValue == "newid()" && initial.DefaultValue == "uuid_generate_v4()")
                    || (final.IsIdentity && initial.IsIdentity);
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

        protected override string MakeColumnString(ColumnAttribute column, bool isReturnType)
        {
            if (column is null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            var typeStr = MakeSqlTypeString(column, true, false);
            var nullString = "";
            var defaultString = "";
            if (!isReturnType)
            {
                nullString = column.IsOptional ? "NULL" : "NOT NULL";
                if (column.DefaultValue != null)
                {
                    var val = column.DefaultValue.ToString().ToLowerInvariant();
                    if (val == "newid()")
                    {
                        val = "uuid_generate_v4()";
                    }
                    else if (val == "getdate()")
                    {
                        val = "current_date";
                    }
                    defaultString = "default " + val;
                }
                else if (column.IsIdentity)
                {
                    typeStr = typeStr.Replace("integer", "serial");
                }
            }

            var columnName = MakeIdentifier(column.Name);
            return $"{columnName} {typeStr} {nullString} {defaultString}".Trim();
        }

        public override string MakeDropRoutineScript(RoutineAttribute routine)
        {
            if (routine is null)
            {
                throw new ArgumentNullException(nameof(routine));
            }

            var routineName = MakeRoutineIdentifier(routine, false);
            return $@"drop function if exists {routineName} cascade;";
        }

        public override string MakeRoutineIdentifier(RoutineAttribute routine, bool withParameterNames)
        {
            if (routine is null)
            {
                throw new ArgumentNullException(nameof(routine));
            }

            var identifier = MakeIdentifier(routine.Schema ?? DefaultSchemaName, routine.Name);
            var parameterSection = MakeParameterSection(routine, withParameterNames);
            return $@"{identifier}({parameterSection})";
        }

        private static readonly Regex HoistPattern = new Regex(@"declare\s+(@\w+\s+\w+(,\s+@\w+\s+\w+)*);?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override string MakeCreateRoutineScript(RoutineAttribute routine, bool createBody = true)
        {
            if (routine is null)
            {
                throw new ArgumentNullException(nameof(routine));
            }

            var queryBody = createBody ? MakeRoutineBody(routine) : routine.Query.Trim();
            var identifier = MakeIdentifier(routine.Schema ?? DefaultSchemaName, routine.Name);
            var returnType = MakeSqlTypeString(routine, true, false) ?? "void";

            if (returnType.Contains("[]"))
            {
                returnType = "setof " + returnType.Substring(0, returnType.Length - 2);
            }

            var parameterSection = MakeParameterSection(routine, true);
            var query = $@"create function {identifier}({parameterSection})
returns {returnType} 
as $$
{queryBody}
$$
language plpgsql;";
            return query;
        }

        protected override string MakeParameterSection(RoutineAttribute info, bool withNames)
        {
            return base.MakeParameterSection(info, withNames).ToLowerInvariant();
        }

        protected override string MakeParameterString(ParameterAttribute param, bool withName)
        {
            if (param is null)
            {
                throw new ArgumentNullException(nameof(param));
            }

            var dirString = "";
            var typeStr = MakeSqlTypeString(param, true, param.IsArray);
            switch (param.Direction)
            {
                case ParameterDirection.InputOutput:
                dirString = "inout";
                break;
                case ParameterDirection.Output:
                dirString = "out";
                break;
            }

            var defaultString = "";
            if (param.DefaultValue != null)
            {
                defaultString = " default = " + param.DefaultValue.ToString();
            }

            if (withName)
            {
                return $"{dirString} _{param.Name} {typeStr}{defaultString}".Trim();
            }
            else
            {
                return $"{dirString} {typeStr}".Trim();
            }
        }

        private static readonly Regex SetVariablePattern = new Regex("select (@\\w+) = (\\w+)", RegexOptions.Compiled);
        private static readonly Regex ReplaceFuncsPattern = new Regex("(newid|getdate)\\(\\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override string MakeRoutineBody(RoutineAttribute routine)
        {
            if (routine is null)
            {
                throw new ArgumentNullException(nameof(routine));
            }

            var queryBody = routine.Query;
            queryBody = ReplaceFuncsPattern.Replace(queryBody, new MatchEvaluator(m =>
            {
                var func = m.Groups[1].Value.ToLowerInvariant();
                if (func == "newid")
                {
                    return "uuid_generate_v4()";
                }
                else if (func == "getdate")
                {
                    return "current_date";
                }
                return "";
            }));

            queryBody = SetVariablePattern.Replace(queryBody, "select $2 into $1");
            var declarations = new List<string>();
            queryBody = HoistPattern.Replace(queryBody, new MatchEvaluator(m =>
            {
                var parts = m.Groups
                    .Cast<Group>()
                    .Skip(1)
                    .Select(g => g.Value.Trim())
                    .Where(s => s.Length > 0)
                    .First()
                    .Split(',')
                    .ToArray();
                declarations.AddRange(parts);
                return "";
            }));


            var returnType = MakeSqlTypeString(routine, true, false);
            if (returnType != null)
            {
                if (returnType.Contains("[]") || returnType.StartsWith("table", StringComparison.InvariantCultureIgnoreCase))
                {
                    returnType = null;
                }
                else
                {
                    declarations.Add("returnValue " + returnType);
                    queryBody += "\nreturn returnValue;";
                }
            }

            for (var i = 0; i < declarations.Count; ++i)
            {
                var parts = declarations[i]
                    .Trim()
                    .Split(' ', '\t', '\n', '\r')
                    .Select(p => p.Trim())
                    .Where(p => p.Length > 0)
                    .ToArray();
                parts[1] = NormalizeSqlType(parts[1]);
                declarations[i] = "\n\t" + string.Join(" ", parts) + ";";
            }
            var declarationString = "";
            if (declarations.Count > 0)
            {
                declarationString = "declare " + string.Join("", declarations);
            }

            queryBody = $@"{declarationString}
begin
{queryBody}
end;";
            queryBody = queryBody.Replace("@", "_");
            return queryBody.Trim();
        }

        public override string MakeCreateTableScript(TableAttribute table)
        {
            if (table is null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            var schema = table.Schema ?? DefaultSchemaName;
            var identifier = MakeIdentifier(schema, table.Name);
            var reset = new List<ColumnAttribute>();
            foreach (var prop in table.Properties)
            {
                if (string.IsNullOrWhiteSpace(prop.SqlType)
                    && prop.SystemType == typeof(int)
                    && prop.IsIdentity)
                {
                    prop.SqlType = "serial";
                    reset.Add(prop);
                }
            }
            var columnSection = MakeColumnSection(table, false);
            foreach (var column in reset)
            {
                column.SqlType = null;
            }
            return $@"create table {identifier} (
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
            return $"drop table if exists {identifier};";
        }

        public override string MakeCreateViewScript(ViewAttribute view)
        {
            if (view is null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            var schema = view.Schema ?? DefaultSchemaName;
            var identifier = MakeIdentifier(schema, view.Name);
            return $@"create view {identifier}
    as {view.Query};";
        }

        public override string MakeDropViewScript(ViewAttribute info)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = MakeIdentifier(schema, info.Name);
            return $"drop view if exists {identifier};";
        }

        public override string MakeCreateRelationshipScript(Relationship relation)
        {
            if (relation is null)
            {
                throw new ArgumentNullException(nameof(relation));
            }

            var fromSchemName = relation.From.Schema ?? DefaultSchemaName;
            var fromTableName = MakeIdentifier(fromSchemName, relation.From.Name);
            var constraintName = MakeIdentifier(relation.GetRelationshipName(this));
            var fromColumns = string.Join(", ", relation.FromColumns.Select(c => MakeIdentifier(c.Name)));
            var toSchemaName = relation.To.Schema ?? DefaultSchemaName;
            var toTableName = MakeIdentifier(toSchemaName, relation.To.Name);
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
            return $@"alter table if exists {tableName} drop constraint if exists {constraintName};";
        }

        public override string MakeDropPrimaryKeyScript(PrimaryKey key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var schemaName = key.Table.Schema ?? DefaultSchemaName;
            var tableName = MakeIdentifier(schemaName, key.Table.Name);
            var constraintName = MakeIdentifier(key.Name);
            var indexName = MakeIdentifier("idx_" + key.Name);
            return $@"alter table if exists {tableName} drop constraint if exists {constraintName};
drop index if exists {indexName};";
        }

        public override string MakeCreatePrimaryKeyScript(PrimaryKey key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var keys = string.Join(", ", key.KeyColumns.Select(c => MakeIdentifier(c.Name)));
            var indexName = MakeIdentifier("idx_" + key.Name);
            var schemaName = key.Table.Schema ?? DefaultSchemaName;
            var tableName = MakeIdentifier(schemaName, key.Table.Name);
            var constraintName = MakeIdentifier(key.Name);
            return $@"create unique index {indexName} on {tableName} ({keys});
alter table {tableName} add constraint {constraintName} primary key using index {indexName};";
        }

        public override string MakeCreateIndexScript(TableIndex idx)
        {
            if (idx is null)
            {
                throw new ArgumentNullException(nameof(idx));
            }

            var indexName = MakeIdentifier(idx.Name);
            var columnSection = string.Join(",", idx.Columns.Select(c => MakeIdentifier(c)));
            var schemaName = idx.Table.Schema ?? DefaultSchemaName;
            var tableName = MakeIdentifier(schemaName, idx.Table.Name);
            return $@"create index {indexName} on {tableName}({columnSection});";
        }

        public override string MakeDropIndexScript(TableIndex idx)
        {
            if (idx is null)
            {
                throw new ArgumentNullException(nameof(idx));
            }

            return $@"drop index if exists {idx.Name};";
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query = @"select usename, useconfig from pg_catalog.pg_user")]
        internal List<User> GetUserSettings()
        {
            return GetList<User>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query = @"select usename from pg_catalog.pg_user")]
        public override List<string> GetDatabaseLogins()
        {
            return GetList<string>();
        }

        public override string MakeCreateDatabaseLoginScript(string userName, string password, string database)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("User name is required", nameof(userName));
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password is required", nameof(password));
            }

            if (string.IsNullOrEmpty(database))
            {
                throw new ArgumentException("Database is required", nameof(database));
            }

            return $"create user {userName} with superuser password '{password}';";
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select schema_name from information_schema.schemata where schema_name not like 'pg_%' and schema_name not in ('information_schema', 'public');")]
        public override List<string> GetSchemata()
        {
            return GetList<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select         
    ordinal_position,
    character_maximum_length,
    character_octet_length,
    numeric_precision,
    numeric_precision_radix,
    numeric_scale,
    datetime_precision,
    table_catalog,
    table_schema,
    table_name,
    column_name,
    column_default,
    is_nullable,
    data_type,
    udt_catalog,
    udt_schema,
    udt_name,
    case is_identity when 'YES' then 1 when 'NO' then 0 end as is_identity
from information_schema.columns
where table_schema != 'information_schema'
    and table_schema != 'pg_catalog'
	and is_updatable = @isUpdatable
order by table_catalog, table_schema, table_name, ordinal_position;")]
        private List<InformationSchema.Column> GetColumns(string isUpdatable)
        {
            var columns = GetList<InformationSchema.Column>(isUpdatable);
            foreach (var column in columns)
            {
                column.data_type = NormalizeSqlType(column.data_type);
                column.udt_name = NormalizeSqlType(column.udt_name);
            }
            return columns;
        }

        public override List<InformationSchema.Column> GetTableColumns()
        {
            return GetColumns("YES");
        }

        public override List<InformationSchema.Column> GetViewColumns()
        {
            return GetColumns("NO");
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select         
    ordinal_position,
    character_maximum_length,
    character_octet_length,
    numeric_precision,
    numeric_precision_radix,
    numeric_scale,
    datetime_precision,
    udt_catalog as table_catalog,
    udt_schema as table_schema,
    udt_name as table_name,
    attribute_name as column_name,
    attribute_default as column_default,
    is_nullable,
    data_type,
    udt_catalog,
    udt_schema,
    udt_name,
    0 as is_identity
from information_schema.attributes
where udt_schema != 'information_schema'
    and udt_schema != 'pg_catalog'
order by udt_catalog, udt_schema, udt_name, ordinal_position;")]
        public override List<InformationSchema.Column> GetUserDefinedTypeColumns()
        {
            return GetList<InformationSchema.Column>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select
    nt.nspname as table_schema,
    t.relname as table_name,
	ni.nspname as index_schema,
    i.relname as index_name,
    a.attname as column_name
from
    pg_class t
    inner join pg_namespace nt on nt.oid = t.relnamespace
    inner join pg_index ix on t.oid = ix.indrelid
    inner join pg_class i on i.oid = ix.indexrelid
    inner join pg_attribute a on a.attrelid = t.oid and a.attnum = ANY(ix.indkey)
	inner join pg_namespace ni on ni.oid = i.relnamespace
where
	t.relkind = 'r'
order by
    nt.nspname,
    t.relname,
	ni.nspname,
    i.relname;")]
        public override List<InformationSchema.IndexColumnUsage> GetIndexColumns()
        {
            return GetList<InformationSchema.IndexColumnUsage>();
        }



        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select 
	table_schema, 
	table_name, 
	view_definition 
from information_schema.views
where table_schema != 'information_schema'
    and table_schema != 'pg_catalog';")]
        public override List<InformationSchema.View> GetViews()
        {
            return GetList<InformationSchema.View>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select *
from information_schema.table_constraints
where table_schema != 'information_schema'
    and table_schema != 'pg_catalog'
order by table_catalog, table_schema, table_name;")]
        public override List<InformationSchema.TableConstraint> GetTableConstraints()
        {
            return GetList<InformationSchema.TableConstraint>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select *
from information_schema.referential_constraints
where constraint_schema != 'information_schema'
    and constraint_schema != 'pg_catalog'
order by constraint_catalog, constraint_schema, constraint_name;")]
        public override List<InformationSchema.ReferentialConstraint> GetReferentialConstraints()
        {
            return GetList<InformationSchema.ReferentialConstraint>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select 
    r.specific_catalog,
    r.specific_schema,
    r.specific_name,
    r.routine_catalog,
    r.routine_schema,
    r.routine_name,
    r.data_type,
    p.prosrc as routine_definition,
	p.proretset as is_array
from information_schema.routines as r
    inner join pg_proc as p on p.proname = r.routine_name
where specific_schema != 'information_schema'
    and specific_schema != 'pg_catalog'
order by specific_catalog, specific_schema, specific_name;")]
        public override List<InformationSchema.Routine> GetRoutines()
        {
            return GetList<InformationSchema.Routine>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select *
from information_schema.parameters
where specific_schema != 'information_schema'
    and specific_schema != 'pg_catalog'
    and parameter_mode in ('IN', 'INOUT')
order by specific_catalog, specific_schema, specific_name, ordinal_position;")]
        public override List<InformationSchema.Parameter> GetParameters()
        {
            var parameters = GetList<InformationSchema.Parameter>();
            foreach (var parameter in parameters)
            {
                parameter.data_type = NormalizeSqlType(parameter.data_type);
                parameter.user_defined_type_name = NormalizeSqlType(parameter.user_defined_type_name);
                parameter.udt_name = NormalizeSqlType(parameter.udt_name);
            }
            return parameters;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select * 
from information_schema.constraint_column_usage
where constraint_schema != 'information_schema'
    and constraint_schema != 'pg_catalog';")]
        public override List<InformationSchema.ConstraintColumnUsage> GetConstraintColumns()
        {
            return GetList<InformationSchema.ConstraintColumnUsage>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select * 
from information_schema.key_column_usage
where constraint_schema != 'information_schema'
    and constraint_schema != 'pg_catalog';")]
        public override List<InformationSchema.KeyColumnUsage> GetKeyColumns()
        {
            return GetList<InformationSchema.KeyColumnUsage>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select extname, extversion from pg_extension where extname != 'plpgsql';")]
        internal List<Extension> GetExtensions()
        {
            return GetList<Extension>();
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

            var columnNames = columns.Select(c => MakeIdentifier(c.Name)).ToArray();
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
                    if (t == typeof(string))
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
            var columnNamesList = string.Join(", ", columnNames);
            var columnValuesList = string.Join(", ", columnValues);
            return $"insert into {tableName}({columnNamesList}) values({columnValuesList});";
        }
    }
}