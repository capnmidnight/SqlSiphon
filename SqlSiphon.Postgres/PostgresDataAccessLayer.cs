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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using Npgsql;

using SqlSiphon.Mapping;
using SqlSiphon.Model;

namespace SqlSiphon.Postgres
{
    /// <summary>
    /// A base class for building Data Access Layers that connect to MySQL
    /// databases and execute store procedures stored within.
    /// </summary>
    public class PostgresDataAccessLayer : SqlSiphon<NpgsqlConnection, NpgsqlCommand, NpgsqlParameter, NpgsqlDataAdapter, NpgsqlDataReader>
    {
        public override string DataSource => Connection.DataSource;

        private static readonly Dictionary<string, Type> typeMapping = new Dictionary<string, Type>
        {
            ["bigint"] = typeof(long),
            ["int8"] = typeof(long),
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

            ["double precision"] = typeof(double),
            ["float8"] = typeof(double),

            ["integer"] = typeof(int),
            ["int"] = typeof(int),
            ["int4"] = typeof(int),
            ["serial"] = typeof(int),
            ["serial4"] = typeof(int),

            ["interval"] = typeof(TimeSpan),

            ["money"] = typeof(decimal),
            ["numeric"] = typeof(decimal),

            ["real"] = typeof(float),
            ["float4"] = typeof(float),

            ["smallint"] = typeof(short),
            ["int2"] = typeof(short),
            ["smallserial"] = typeof(short),
            ["serial2"] = typeof(short),

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

        private static readonly Dictionary<string, int> defaultTypeSizes = new Dictionary<string, int>
        {
            ["float4"] = 24,
            ["float8"] = 53,
            ["integer"] = 32
        };


        private static readonly Dictionary<Type, string> reverseTypeMapping = typeMapping
                .GroupBy(kv => kv.Value, kv => kv.Key)
                .ToDictionary(g => g.Key, g => g.First());

        static PostgresDataAccessLayer()
        {
            reverseTypeMapping.Add(typeof(int?), "int");
            reverseTypeMapping.Add(typeof(uint), "int");
            reverseTypeMapping.Add(typeof(uint?), "int");

            reverseTypeMapping.Add(typeof(long?), "bigint");
            reverseTypeMapping.Add(typeof(ulong), "bigint");
            reverseTypeMapping.Add(typeof(ulong?), "bigint");

            reverseTypeMapping.Add(typeof(short?), "smallint");
            reverseTypeMapping.Add(typeof(ushort), "smallint");
            reverseTypeMapping.Add(typeof(ushort?), "smallint");
            reverseTypeMapping.Add(typeof(byte), "smallint");
            reverseTypeMapping.Add(typeof(byte?), "smallint");
            reverseTypeMapping.Add(typeof(sbyte), "smallint");
            reverseTypeMapping.Add(typeof(sbyte?), "smallint");
            reverseTypeMapping.Add(typeof(char), "smallint");
            reverseTypeMapping.Add(typeof(char?), "smallint");

            reverseTypeMapping.Add(typeof(decimal?), "decimal");
            reverseTypeMapping.Add(typeof(bool?), "boolean");
            reverseTypeMapping.Add(typeof(float?), "real");
            reverseTypeMapping.Add(typeof(double?), "double precision");
            reverseTypeMapping.Add(typeof(DateTime?), "time with time zone");
            reverseTypeMapping.Add(typeof(Guid?), "uuid");
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
            : base(dal.Connection)
        {
        }

        public PostgresDataAccessLayer(string server, string database, string userName, string password)
            : base(server, database, userName, password)
        {
        }

        public override string MakeConnectionString(string server, string database, string userName, string password)
        {
            var builder = new Npgsql.NpgsqlConnectionStringBuilder
            {
                Database = database.ToLower()
            };

            if (!string.IsNullOrWhiteSpace(userName)
                && !string.IsNullOrWhiteSpace(password))
            {
                builder.UserName = userName.Trim();
                builder.Add("Password", password.Trim());
            }

            var i = server.IndexOf(":", StringComparison.InvariantCultureIgnoreCase);
            if (i > -1)
            {
                builder.Host = server.Substring(0, i);
                if (int.TryParse(server.Substring(i + 1), out var port))
                {
                    builder.Port = port;
                }
            }
            else
            {
                builder.Host = server;
            }

            return builder.ConnectionString;
        }

        public override bool RunCommandLine(string executablePath, string configurationPath, string server, string database, string adminUser, string adminPass, string query)
        {
            if (string.IsNullOrWhiteSpace(adminUser) || string.IsNullOrWhiteSpace(adminPass))
            {
                throw new Exception("PSQL does not support Windows Authentication. Please provide a username and password for the server process.");
            }
            else
            {
                if (database != null)
                {
                    // Postgres is case-insensitive to database names in general, but
                    // the psql program is not. You can't make a mixed-case database,
                    // but psql will not match mixed-case names to their lowercase version.
                    database = database.ToLower();
                }

                query = query.Replace("\"", "\\\"");

                // I prefer colon-separated address/port specifications.
                string port = null;
                var i = server.IndexOf(":", StringComparison.InvariantCultureIgnoreCase);
                if (i > -1)
                {
                    port = server.Substring(i + 1);
                    server = server.Substring(0, i);
                }

                configurationPath = FindConfigurationFile(configurationPath);

                var originalConf = InjectUserCredentials(configurationPath, server, port, database, adminUser, adminPass);

                var succeeded = false;
                try
                {
                    succeeded = RunProcess(
                        executablePath,
                        new string[]{
                            "-h " + server,
                            string.IsNullOrWhiteSpace(port) ? null : "-p " + port,
                            "-U " + adminUser,
                            (database == null) ? null : "-d " + database,
                            string.Format("-c \"{0}\"", query)
                    });
                }
                finally
                {
                    RevertConfigurationFile(configurationPath, originalConf);
                }

                return succeeded;
            }
        }

        private static void RevertConfigurationFile(string configurationPath, string[] originalConf)
        {
            if (originalConf != null)
            {
                if (originalConf.Length == 0)
                {
                    File.Delete(configurationPath);
                }
                else
                {
                    File.WriteAllLines(configurationPath, originalConf);
                }
            }
        }

        /// <summary>
        /// Starting from InitDB's configuration file, finds the Postgres configuration file.
        /// </summary>
        /// <param name="configurationPath"></param>
        /// <returns></returns>
        private static string FindConfigurationFile(string configurationPath)
        {
            // If either InitDB or Postgres' configuration file moves, this will break. But
            // for now, they are both in the User's AppData directory.
            var j = configurationPath.IndexOf("InitDB", StringComparison.InvariantCultureIgnoreCase);
            configurationPath = Path.Combine(configurationPath.Substring(0, j), "postgresql", "pgpass.conf");
            return configurationPath;
        }

        /// <summary>
        /// We can't send a password to the server directly, so we have twiddle the user's
        /// pgpass.conf file. See this page for more information on the pgpass.conf file:
        ///     http://www.postgresql.org/docs/current/static/libpq-pgpass.html
        /// </summary>
        /// <param name="configurationPath"></param>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <param name="database"></param>
        /// <param name="adminUser"></param>
        /// <param name="adminPass"></param>
        /// <returns></returns>
        private static string[] InjectUserCredentials(string configurationPath, string server, string port, string database, string adminUser, string adminPass)
        {
            string[] originalConf = null;
            if (File.Exists(configurationPath))
            {
                originalConf = File.ReadAllLines(configurationPath);
            }

            var lineToAdd = string.Format(
                "{0}:{1}:{2}:{3}:{4}",
                server,
                port ?? "*",
                database ?? "*",
                adminUser,
                adminPass);

            if (originalConf == null)
            {
                // No configuration file exists, so make one.
                originalConf = new string[0];
                File.WriteAllText(configurationPath, lineToAdd);
            }
            else
            {
                var conf = originalConf.ToList();
                var i = conf.IndexOf(lineToAdd);
                if (i == -1)
                {
                    conf.Add(lineToAdd);
                    File.WriteAllLines(configurationPath, conf);
                }
                else
                {
                    // the configuration file is already setup for us,
                    // so don't make any changes to it.
                    originalConf = null;
                }
            }
            return originalConf;
        }


        protected override string IdentifierPartBegin => "\"";
        protected override string IdentifierPartEnd => "\"";
        public override string DefaultSchemaName => "public";
        public override int DefaultTypePrecision(string typeName, int testSize)
        {
            if (!defaultTypeSizes.ContainsKey(typeName))
            {
                throw new Exception(string.Format("I don't know the default type size for `{0}`. Perhaps it is {1}?\n\ndefaultTypeSizes.Add(\"{0}\", {1});\n\n", typeName, testSize));
            }
            return defaultTypeSizes[typeName];
        }

        public override string MakeIdentifier(params string[] parts)
        {
            var goodParts = parts.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
            for (var i = 0; i < goodParts.Length; ++i)
            {
                if (goodParts[i].Length > 63)
                {
                    var len = goodParts[i].Length.ToString();
                    var lengthLength = len.Length;
                    goodParts[i] = goodParts[i].Substring(0, 63 - lengthLength) + len;
                }
            }
            return base.MakeIdentifier(goodParts).ToLowerInvariant();
        }

        public override Type GetSystemType(string sqlType)
        {
            return typeMapping.ContainsKey(sqlType) ? typeMapping[sqlType] : null;
        }

        public override DatabaseState GetInitialState(string catalogueName, Regex filter)
        {
            var state = base.GetInitialState(catalogueName, filter);
            var pgState = new PostgresDatabaseState(state);
            if (pgState.CatalogueExists.HasValue && pgState.CatalogueExists.Value)
            {
                GetExtensions().ForEach(pgState.AddExtension);
            }
            return pgState;
        }

        public override DatabaseState GetFinalState(Type dalType, string userName, string password)
        {
            var state = base.GetFinalState(dalType, userName, password);
            var pgState = new PostgresDatabaseState(state);
            var guidType = typeof(Guid);

            if (pgState.TypeExists<Guid>())
            {
                pgState.AddExtension("uuid-ossp", "1.0");
            }
            pgState.AddExtension("postgis", "2.1.1");
            pgState.Schemata.AddRange(pgState.Extensions.Keys.Where(e => !pgState.Schemata.Contains(e)));
            return pgState;
        }

        protected override string MakeSqlTypeString(string sqlType, Type systemType, int? size, int? precision, bool isIdentity)
        {
            string typeName = null;

            if (sqlType != null)
            {
                typeName = sqlType;
            }
            else if (systemType != null)
            {
                if (reverseTypeMapping.ContainsKey(systemType))
                {
                    typeName = reverseTypeMapping[systemType];
                }

                if (typeName == null)
                {
                    typeName = MakeComplexSqlTypeString(systemType);

                    if (typeName == null && systemType.Name != "Void")
                    {
                        throw new Exception(string.Format("Couldn't find type description for type: {0}", systemType?.FullName ?? "N/A"));
                    }
                }
            }

            if (size.HasValue && typeName.IndexOf("(", StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                var format = precision.HasValue
                    ? "({0},{1})"
                    : "({0})";
                var sizeStr = string.Format(format, size, precision);
                var bracketsIndex = typeName.IndexOf("[]", StringComparison.InvariantCultureIgnoreCase);
                if (bracketsIndex > -1)
                {
                    typeName = typeName.Substring(0, bracketsIndex);
                }
                if (typeName == "text")
                {
                    typeName = "varchar";
                }
                typeName += sizeStr;
                if (bracketsIndex > -1)
                {
                    typeName += "[]";
                }
            }
            return typeName;
        }

        public override bool DescribesIdentity(InformationSchema.Columns column)
        {
            if (column.column_default != null && column.column_default.IndexOf("nextval", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                column.column_default = null;
                column.udt_name = "integer";
                return true;
            }
            return false;
        }

        public override string MakeCreateColumnScript(ColumnAttribute prop)
        {
            return string.Format("alter table if exists {0} add column {1} {2};",
                MakeIdentifier(prop.Table.Schema ?? DefaultSchemaName, prop.Table.Name),
                prop.Name,
                MakeSqlTypeString(prop));
        }

        public override string MakeDropColumnScript(ColumnAttribute prop)
        {
            return string.Format("alter table if exists {0} drop column if exists {1};",
                MakeIdentifier(prop.Table.Schema ?? DefaultSchemaName, prop.Table.Name),
                MakeIdentifier(prop.Name));
        }

        public override string RoutineChanged(RoutineAttribute a, RoutineAttribute b)
        {
            var typeA = MakeSqlTypeString(a) ?? "void";
            var changedReturnType = typeA != b.SqlType;
            return changedReturnType ? "IDK" : base.RoutineChanged(a, b);
        }

        private string MakeComplexSqlTypeString(Type systemType)
        {
            var baseType = DataConnector.CoalesceCollectionType(systemType);

            string sqlType;
            if (baseType == typeof(void))
            {
                sqlType = "void";
            }
            else if (reverseTypeMapping.ContainsKey(baseType))
            {
                sqlType = reverseTypeMapping[baseType];
            }
            else
            {
                var attr = DatabaseObjectAttribute.GetAttribute(baseType) ?? new TableAttribute(baseType);
                sqlType = string.Format("table ({0})", MakeColumnSection(attr, true));
            }

            if (DataConnector.IsTypeCollection(systemType))
            {
                sqlType += "[]";
            }

            return sqlType;
        }

        public override string MakeAlterColumnScript(ColumnAttribute final, ColumnAttribute initial)
        {
            var preamble = string.Format(
                "alter table if exists {0}",
                MakeIdentifier(final.Table.Schema ?? DefaultSchemaName, final.Table.Name));

            if (final.Include != initial.Include)
            {
                return string.Format(
                    "{0} {1} column {2} {3};",
                    preamble,
                    final.Include ? "add" : "drop",
                    MakeIdentifier(final.Name),
                    final.Include ? MakeSqlTypeString(final) : "")
                    .Trim();
            }
            else if (final.DefaultValue != initial.DefaultValue)
            {
                return string.Format(
                    "{0} alter column {1} {2} default {3};",
                    preamble,
                    MakeIdentifier(final.Name),
                    final.DefaultValue == null ? "drop" : "set",
                    final.DefaultValue ?? "")
                    .Trim();
            }
            else if (final.IsOptional != initial.IsOptional)
            {
                return string.Format(
                    "{0} alter column {1} {2} not null;",
                    preamble,
                    MakeIdentifier(final.Name),
                    final.IsOptional ? "drop" : "set");
            }
            else if (final.SystemType != initial.SystemType
                || final.Size != initial.Size
                || final.Precision != initial.Precision
                || final.IsIdentity != initial.IsIdentity)
            {
                return string.Format(
                    "{0} alter column {1} set data type {2};",
                    preamble,
                    MakeIdentifier(final.Name),
                    MakeSqlTypeString(final));
            }
            else
            {
                // by this point, the columns should be identical, but we still need a base case
                return null;
            }
        }


        protected override string CheckDefaultValueDifference(ColumnAttribute final, ColumnAttribute initial)
        {
            var valuesMatch = true;
            if (final.SystemType == typeof(bool))
            {
                var xb = final.DefaultValue.Replace("'", "").ToLower();
                var xbb = xb == "true" || xb == "1";
                var yb = initial.DefaultValue.Replace("'", "").ToLower();
                var ybb = yb == "true" || yb == "1";
                valuesMatch = xbb == ybb;
            }
            else if (final.SystemType == typeof(DateTime))
            {
                valuesMatch = (final.DefaultValue == "'9999/12/31 23:59:59.99999'"
                        && initial.DefaultValue == "'9999-12-31'::date")
                    || (final.DefaultValue == "getdate()"
                        && initial.DefaultValue == "('now'::text)::date");
            }
            else if (final.SystemType == typeof(double))
            {
                valuesMatch = final.DefaultValue == "(" + initial.DefaultValue + ")"
                    || initial.DefaultValue == "(" + final.DefaultValue + ")";
            }
            else if (final.DefaultValue != "newid()" || initial.DefaultValue != "uuid_generate_v4()")
            {
                valuesMatch = false;
            }

            if (valuesMatch)
            {
                initial.DefaultValue = final.DefaultValue;
                return null;
            }
            else
            {
                return string.Format("Column default value has changed. Was {0}, now {1}.", initial.DefaultValue, final.DefaultValue);
            }
        }

        protected override string MakeParameterString(ParameterAttribute param)
        {
            var dirString = "";
            var typeStr = MakeSqlTypeString(param);
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

            return string.Format("{0} _{1} {2}{3}", dirString, param.Name, typeStr, defaultString).Trim();
        }

        protected override string MakeColumnString(ColumnAttribute column, bool isReturnType)
        {
            var typeStr = MakeSqlTypeString(column);
            var nullString = "";
            var defaultString = "";
            if (!isReturnType)
            {
                nullString = column.IsOptional ? "NULL" : "NOT NULL";
                if (column.DefaultValue != null)
                {
                    var val = column.DefaultValue.ToString().ToLower();
                    if (val == "getdate()")
                    {
                        val = "current_date";
                    }
                    else if (val == "newid()")
                    {
                        val = "uuid_generate_v4()";
                    }
                    defaultString = "default " + val;
                }
                else if (column.IsIdentity)
                {
                    typeStr = typeStr.Replace("integer", "serial");
                }
            }

            return string.Format("{0} {1} {2} {3}",
                MakeIdentifier(column.Name),
                typeStr,
                nullString,
                defaultString).Trim();
        }

        public override string MakeDropRoutineScript(RoutineAttribute routine)
        {
            return string.Format(@"drop function if exists {0} cascade;", MakeRoutineIdentifier(routine));
        }

        public override string MakeRoutineIdentifier(RoutineAttribute routine)
        {
            var identifier = MakeIdentifier(routine.Schema ?? DefaultSchemaName, routine.Name);
            var parameterSection = MakeParameterSection(routine);
            return string.Format(@"{0}({1})", identifier, parameterSection);
        }

        private static readonly Regex HoistPattern = new Regex(@"declare\s+(@\w+\s+\w+(,\s+@\w+\s+\w+)*);?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override string MakeCreateRoutineScript(RoutineAttribute routine, bool createBody = true)
        {
            var queryBody = createBody ? MakeRoutineBody(routine) : routine.Query.Trim();
            var identifier = MakeIdentifier(routine.Schema ?? DefaultSchemaName, routine.Name);
            var returnType = MakeSqlTypeString(routine) ?? "void";
            if (returnType.Contains("[]"))
            {
                returnType = "setof " + returnType.Substring(0, returnType.Length - 2);
            }
            var parameterSection = MakeParameterSection(routine);
            var query = string.Format(
@"create function {0}({1})
returns {2} 
as $$
{3}
$$
language plpgsql;",
                identifier,
                parameterSection,
                returnType ?? "void",
                queryBody);
            return query;
        }

        private static readonly Regex SetVariablePattern = new Regex("select (@\\w+) = (\\w+)", RegexOptions.Compiled);
        private static readonly Regex ReplaceFuncsPattern = new Regex("(newid|getdate)\\(\\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override string MakeRoutineBody(RoutineAttribute routine)
        {
            var queryBody = routine.Query;
            queryBody = ReplaceFuncsPattern.Replace(queryBody, new MatchEvaluator(m =>
            {
                var func = m.Groups[1].Value.ToLower();
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


            var returnType = MakeSqlTypeString(routine);
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
                parts[1] = NormalizeTypeName(parts[1].ToLower());
                declarations[i] = "\n\t" + string.Join(" ", parts) + ";";
            }
            var declarationString = "";
            if (declarations.Count > 0)
            {
                declarationString = "declare " + string.Join("", declarations);
            }

            queryBody = string.Format(
@"{0}
begin
{1}
end;",
                declarationString,
                queryBody);
            queryBody = queryBody.Replace("@", "_");
            return queryBody.Trim();
        }

        public string NormalizeTypeName(string name)
        {
            var newName = name;
            if (typeMapping.ContainsKey(name) && reverseTypeMapping.ContainsKey(typeMapping[name]))
            {
                newName = reverseTypeMapping[typeMapping[name]];
            }
            return newName;
        }

        public override string MakeCreateTableScript(TableAttribute table)
        {
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
            return string.Format(@"create table {0} (
    {1}
);",
                identifier,
                columnSection);
        }

        public override string MakeDropTableScript(TableAttribute info)
        {
            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = MakeIdentifier(schema, info.Name);
            return string.Format("drop table if exists {0};", identifier);
        }

        public override string MakeCreateRelationshipScript(Relationship relation)
        {
            var fromColumns = string.Join(", ", relation.FromColumns.Select(c => MakeIdentifier(c.Name)));
            var toColumns = string.Join(", ", relation.To.PrimaryKey.KeyColumns.Select(c => MakeIdentifier(c.Name)));
            return string.Format(
@"alter table {0} add constraint {1}
    foreign key({2})
    references {3}({4});",
                    MakeIdentifier(relation.From.Schema ?? DefaultSchemaName, relation.From.Name),
                    MakeIdentifier(relation.GetName(this)),
                    fromColumns,
                    MakeIdentifier(relation.To.Schema ?? DefaultSchemaName, relation.To.Name),
                    toColumns);
        }

        public override string MakeDropRelationshipScript(Relationship relation)
        {
            return string.Format(@"alter table if exists {0} drop constraint if exists {1};",
                MakeIdentifier(relation.From.Schema ?? DefaultSchemaName, relation.From.Name),
                MakeIdentifier(relation.GetName(this)));
        }

        public override string MakeDropPrimaryKeyScript(PrimaryKey key)
        {
            return string.Format(@"alter table if exists {0} drop constraint if exists {1};
drop index if exists {2};",
                MakeIdentifier(key.Table.Schema ?? DefaultSchemaName, key.Table.Name),
                MakeIdentifier(key.Name),
                MakeIdentifier("idx_" + key.Name));
        }

        public override string MakeCreatePrimaryKeyScript(PrimaryKey key)
        {
            var keys = string.Join(", ", key.KeyColumns.Select(c => MakeIdentifier(c.Name)));
            return string.Format(
@"create unique index {0} on {1} ({2});
alter table {1} add constraint {3} primary key using index {0};",
                MakeIdentifier("idx_" + key.Name),
                MakeIdentifier(key.Table.Schema ?? DefaultSchemaName, key.Table.Name),
                keys,
                MakeIdentifier(key.Name));
        }

        public override string MakeCreateIndexScript(TableIndex idx)
        {
            var columnSection = string.Join(",", idx.Columns.Select(c => MakeIdentifier(c)));
            var tableName = MakeIdentifier(idx.Table.Schema ?? DefaultSchemaName, idx.Table.Name);
            return string.Format(
@"create index {0} on {1}({2});",
                MakeIdentifier(idx.Name),
                tableName,
                columnSection);
        }

        public override string MakeDropIndexScript(TableIndex idx)
        {
            return string.Format(@"drop index if exists {0};", idx.Name);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query = @"select usename from pg_catalog.pg_user")]
        public override List<string> GetDatabaseLogins()
        {
            return GetList<string>();
        }

        public override string MakeCreateDatabaseLoginScript(string userName, string password, string database)
        {
            return string.Format("create user {0} with superuser password '{1}';", userName, password);
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
order by table_catalog, table_schema, table_name, ordinal_position;")]
        public override List<InformationSchema.Columns> GetColumns()
        {
            return GetList<InformationSchema.Columns>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select
    n.nspname as table_schema,
    t.relname as table_name,
    i.relname as index_name,
    a.attname as column_name
from
    pg_class t
    inner join pg_namespace n on n.oid = t.relnamespace
    inner join pg_index ix on t.oid = ix.indrelid
    inner join pg_class i on i.oid = ix.indexrelid
    inner join pg_attribute a on a.attrelid = t.oid and a.attnum = ANY(ix.indkey)
where
	t.relkind = 'r'
    and t.relname like 'test%'
order by
    n.nspname,
    t.relname,
    i.relname;")]
        public override List<InformationSchema.IndexColumnUsage> GetIndexColumns()
        {
            return GetList<InformationSchema.IndexColumnUsage>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select *
from information_schema.table_constraints
where table_schema != 'information_schema'
    and table_schema != 'pg_catalog'
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
    and constraint_schema != 'pg_catalog'
order by constraint_catalog, constraint_schema, constraint_name;")]
        public override List<InformationSchema.ReferentialConstraints> GetReferentialConstraints()
        {
            return GetList<InformationSchema.ReferentialConstraints>();
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
    p.prosrc as routine_definition
from information_schema.routines as r
    inner join pg_proc as p on p.proname = r.routine_name
    left outer join (select specific_name, count(*) as argcount
	    from information_schema.parameters
	    where parameter_mode in ('IN', 'INOUT')
	    group by specific_name) as q 
		    on r.specific_name = q.specific_name
		    and p.pronargs = q.argcount
where specific_schema != 'information_schema'
    and specific_schema != 'pg_catalog'
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
    and specific_schema != 'pg_catalog'
    and parameter_mode in ('IN', 'INOUT')
order by specific_catalog, specific_schema, specific_name, ordinal_position;")]
        public override List<InformationSchema.Parameters> GetParameters()
        {
            return GetList<InformationSchema.Parameters>();
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
        internal List<pg_extension> GetExtensions()
        {
            return GetList<pg_extension>();
        }

        public override string MakeInsertScript(TableAttribute table, object value)
        {
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
                    if (DataConnector.IsTypeBarePrimitive(t))
                    {
                        val = v.ToString();
                    }
                    else if (DataConnector.IsTypeQuotedPrimitive(t))
                    {
                        val = string.Format("'{0}'", v);
                    }
                    else
                    {
                        throw new Exception("Can't insert value");
                    }
                }
                return val;
            }).ToArray();

            return string.Format("insert into {0}({1}) values({2});",
                MakeIdentifier(table.Schema ?? DefaultSchemaName, table.Name),
                string.Join(", ", columnNames),
                string.Join(", ", columnValues));
        }
    }
}