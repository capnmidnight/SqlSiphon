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
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using Npgsql;
using SqlSiphon.Mapping;

namespace SqlSiphon.Postgres
{
    /// <summary>
    /// A base class for building Data Access Layers that connect to MySQL
    /// databases and execute store procedures stored within.
    /// </summary>
    public class PostgresDataAccessLayer : SqlSiphon<NpgsqlConnection, NpgsqlCommand, NpgsqlParameter, NpgsqlDataAdapter, NpgsqlDataReader>
    {
        private static Dictionary<string, int> defaultTypeSizes;
        private static Dictionary<string, Type> typeMapping;
        private static Dictionary<Type, string> reverseTypeMapping;

        public const string DATABASE_TYPE_NAME = "PostgreSQL";

        public override string DatabaseType
        {
            get { return DATABASE_TYPE_NAME; }
        }

        static PostgresDataAccessLayer()
        {
            typeMapping = new Dictionary<string, Type>();

            typeMapping.Add("bigint", typeof(long));
            typeMapping.Add("int8", typeof(long));
            typeMapping.Add("bigserial", typeof(long));
            typeMapping.Add("serial8", typeof(long));

            typeMapping.Add("bit", typeof(bool[]));
            typeMapping.Add("varbit", typeof(bool[]));
            typeMapping.Add("bit varying", typeof(bool[]));

            typeMapping.Add("boolean", typeof(bool));
            typeMapping.Add("bool", typeof(bool));

            typeMapping.Add("bytea", typeof(byte[]));

            typeMapping.Add("text", typeof(string));
            typeMapping.Add("character varying", typeof(string));
            typeMapping.Add("varchar", typeof(string));
            typeMapping.Add("tsquery", typeof(string));
            typeMapping.Add("xml", typeof(string));
            typeMapping.Add("json", typeof(string));
            typeMapping.Add("name", typeof(string));
            typeMapping.Add("character", typeof(string));
            typeMapping.Add("char", typeof(string));

            typeMapping.Add("inet", typeof(System.Net.IPAddress));
            typeMapping.Add("cidr", typeof(string));

            typeMapping.Add("date", typeof(DateTime));
            typeMapping.Add("datetime", typeof(DateTime)); // included for tranlating T-SQL to PG/PSQL
            typeMapping.Add("datetime2", typeof(DateTime)); // included for tranlating T-SQL to PG/PSQL

            typeMapping.Add("double precision", typeof(double));
            typeMapping.Add("float8", typeof(double));

            typeMapping.Add("integer", typeof(int));
            typeMapping.Add("int", typeof(int));
            typeMapping.Add("int4", typeof(int));
            typeMapping.Add("serial", typeof(int));
            typeMapping.Add("serial4", typeof(int));

            typeMapping.Add("interval", typeof(TimeSpan));

            typeMapping.Add("money", typeof(decimal));
            typeMapping.Add("numeric", typeof(decimal));

            typeMapping.Add("real", typeof(float));
            typeMapping.Add("float4", typeof(float));

            typeMapping.Add("smallint", typeof(short));
            typeMapping.Add("int2", typeof(short));
            typeMapping.Add("smallserial", typeof(short));
            typeMapping.Add("serial2", typeof(short));

            typeMapping.Add("time", typeof(DateTime));
            typeMapping.Add("time with time zone", typeof(DateTime));
            typeMapping.Add("timestamp", typeof(DateTime));
            typeMapping.Add("timestamp with time zone", typeof(DateTime));

            typeMapping.Add("uuid", typeof(Guid));
            typeMapping.Add("uniqueidentifier", typeof(Guid)); // included for tranlating T-SQL to PG/PSQL

            typeMapping.Add("box", typeof(NpgsqlTypes.NpgsqlBox));
            typeMapping.Add("circle", typeof(NpgsqlTypes.NpgsqlCircle));
            typeMapping.Add("lseg", typeof(NpgsqlTypes.NpgsqlLSeg));
            typeMapping.Add("macaddr", typeof(string));
            typeMapping.Add("path", typeof(NpgsqlTypes.NpgsqlPath));
            typeMapping.Add("point", typeof(NpgsqlTypes.NpgsqlPoint));
            typeMapping.Add("polygon", typeof(NpgsqlTypes.NpgsqlPolygon));
            typeMapping.Add("geometry", typeof(string));
            typeMapping.Add("geography", typeof(string));
            typeMapping.Add("tsvector", typeof(string));

            defaultTypeSizes = new Dictionary<string, int>();
            defaultTypeSizes.Add("float4", 24);
            defaultTypeSizes.Add("float8", 53);
            defaultTypeSizes.Add("integer", 32);

            reverseTypeMapping = typeMapping
                .GroupBy(kv => kv.Value, kv => kv.Key)
                .ToDictionary(g => g.Key, g => g.First());

            reverseTypeMapping.Add(typeof(int?), "int");
            reverseTypeMapping.Add(typeof(uint), "int");
            reverseTypeMapping.Add(typeof(uint?), "int");

            reverseTypeMapping.Add(typeof(long?), "bigint");
            reverseTypeMapping.Add(typeof(ulong), "bigint");
            reverseTypeMapping.Add(typeof(ulong?), "bigint");

            reverseTypeMapping.Add(typeof(short?), "smallint");
            reverseTypeMapping.Add(typeof(ushort), "smallint");
            reverseTypeMapping.Add(typeof(ushort?), "smallint");

            reverseTypeMapping.Add(typeof(byte?), "character[1]");
            reverseTypeMapping.Add(typeof(sbyte), "character[1]");
            reverseTypeMapping.Add(typeof(sbyte?), "character[1]");
            reverseTypeMapping.Add(typeof(char), "character[1]");
            reverseTypeMapping.Add(typeof(char?), "character[1]");

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

            var i = server.IndexOf(":");
            if (i > -1)
            {
                builder.Host = server.Substring(0, i);
                int port = 0;
                if (int.TryParse(server.Substring(i + 1), out port))
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

                // I prefer colon-separated address/port specifications.
                string port = null;
                int i = server.IndexOf(":");
                if (i > -1)
                {
                    port = server.Substring(i + 1);
                    server = server.Substring(0, i);
                }

                configurationPath = FindConfigurationFile(configurationPath);

                string[] originalConf = InjectUserCredentials(configurationPath, server, port, database, adminUser, adminPass);

                bool succeeded = false;
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
            int j = configurationPath.IndexOf("InitDB");
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
                int i = conf.IndexOf(lineToAdd);
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


        protected override string IdentifierPartBegin { get { return "\""; } }
        protected override string IdentifierPartEnd { get { return "\""; } }
        public override string DefaultSchemaName { get { return "public"; } }
        public override int DefaultTypeSize(string typeName, int testSize)
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
            for (int i = 0; i < goodParts.Length; ++i)
            {
                if (goodParts[i].Length > 63)
                {
                    var len = goodParts[i].Length.ToString();
                    var lengthLength = len.Length;
                    goodParts[i] = goodParts[i].Substring(0, 63 - lengthLength) + len;
                }
            }
            return base.MakeIdentifier(goodParts).ToLower();
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
                this.GetExtensions().ForEach(pgState.AddExtension);
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
                typeName = MakeBasicSqlTypeString(systemType);
                if (typeName == null)
                {
                    typeName = MakeComplexSqlTypeString(systemType);

                    if (typeName == null && systemType.Name != "Void")
                    {
                        throw new Exception(string.Format("Couldn't find type description for type: {0}", systemType != null ? systemType.FullName : "N/A"));
                    }
                }
            }

            if (size.HasValue && typeName.IndexOf("(") == -1)
            {
                if (typeName == "text")
                {
                    typeName = "varchar";
                }
                var format = precision.HasValue
                    ? "{0}({1},{2})"
                    : "{0}({1})";
                typeName = string.Format(format, typeName, size, precision);
            }
            return typeName;
        }

        private string MakeBasicSqlTypeString(Type t)
        {
            if (t.IsGenericType)
            {
                var subTypes = t.GetGenericArguments();
                if (subTypes.Length > 1)
                    throw new Exception("Type is too complex!");
                t = subTypes.First();
            }
            if (reverseTypeMapping.ContainsKey(t))
            {
                return reverseTypeMapping[t];
            }
            else
            {
                return null;
            }
        }

        public override bool DescribesIdentity(InformationSchema.Columns column)
        {
            if (column.column_default != null && column.column_default.IndexOf("nextval") == 0)
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
                this.MakeIdentifier(prop.Table.Schema ?? DefaultSchemaName, prop.Table.Name),
                prop.Name,
                this.MakeSqlTypeString(prop));
        }

        public override string MakeDropColumnScript(ColumnAttribute prop)
        {
            return string.Format("alter table if exists {0} drop column if exists {1};",
                this.MakeIdentifier(prop.Table.Schema ?? DefaultSchemaName, prop.Table.Name),
                this.MakeIdentifier(prop.Name));
        }

        private string MakeComplexSqlTypeString(Type systemType)
        {
            string name = systemType != null ? systemType.FullName : null;
            string sqlType = null;
            var isRef = systemType.Name.Last() == '&';
            var elemType = systemType.IsArray || isRef ? systemType.GetElementType() : systemType;
            var attr = DatabaseObjectAttribute.GetAttribute<TableAttribute>(elemType);
            if (attr != null)
            {
                attr.InferProperties(elemType);
                if (systemType.IsArray)
                {
                    sqlType = attr.Name + "[]";
                }
                else
                {
                    sqlType = string.Format("table ({0})", this.MakeColumnSection(attr, true));
                }
            }
            else if (reverseTypeMapping.ContainsKey(elemType))
            {
                sqlType = reverseTypeMapping[elemType];
                if (systemType.IsArray)
                {
                    sqlType += "[]";
                }
            }
            else if (systemType != typeof(void))
            {
                attr = new TableAttribute();
                attr.InferProperties(systemType);
                sqlType = string.Format("table ({0})", this.MakeColumnSection(attr, true));
            }
            else
            {
                sqlType = "void";
            }
            return sqlType;
        }

        public override string MakeAlterColumnScript(ColumnAttribute final, ColumnAttribute initial)
        {
            var preamble = string.Format(
                "alter table if exists {0}",
                this.MakeIdentifier(final.Table.Schema ?? DefaultSchemaName, final.Table.Name));

            if (final.Include != initial.Include)
            {
                return string.Format(
                    "{0} {1} column {2} {3};",
                    preamble,
                    final.Include ? "add" : "drop",
                    this.MakeIdentifier(final.Name),
                    final.Include ? this.MakeSqlTypeString(final) : "")
                    .Trim();
            }
            else if (final.DefaultValue != initial.DefaultValue)
            {
                return string.Format(
                    "{0} alter column {1} {2} default {3};",
                    preamble,
                    this.MakeIdentifier(final.Name),
                    final.DefaultValue == null ? "drop" : "set",
                    final.DefaultValue ?? "")
                    .Trim();
            }
            else if (final.IsOptional != initial.IsOptional)
            {
                return string.Format(
                    "{0} alter column {1} {2} not null;",
                    preamble,
                    this.MakeIdentifier(final.Name),
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
                    this.MakeIdentifier(final.Name),
                    this.MakeSqlTypeString(final));
            }
            else
            {
                // by this point, the columns should be identical, but we still need a base case
                return null;
            }
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
                final.IsIdentity != initial.IsIdentity,
                final.IsOptional != initial.IsOptional,
                final.Name.ToLower() != initial.Name.ToLower(),
                finalType != initial.SystemType,
                final.Table.Schema.ToLower() != initial.Table.Schema.ToLower(),
                final.Table.Name.ToLower() != initial.Table.Name.ToLower()
            };
            var changed = tests.Any(a => a);
            if (final.SystemType == initial.SystemType)
            {
                if (final.DefaultValue != null
                    && initial.DefaultValue != null
                    && final.DefaultValue != initial.DefaultValue)
                {
                    bool valuesMatch = true;
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
                        valuesMatch = final.DefaultValue == "'9999/12/31 23:59:59.99999'" && initial.DefaultValue == "'9999-12-31'::date"
                                || final.DefaultValue == "getdate()" && initial.DefaultValue == "('now'::text)::date";
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
                    }
                    changed = changed || !valuesMatch;
                }

                if (final.Size != initial.Size)
                {
                    if (final.SystemType != typeof(double) || final.IsSizeSet || initial.Size != this.DefaultTypeSize(final.SqlType, initial.Size))
                    {
                        changed = true;
                    }
                }
            }
            return changed;
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
                defaultString = " default = " + param.DefaultValue.ToString();

            return string.Format("{0} @{1} {2}{3}", dirString, param.Name, typeStr, defaultString);
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
                column.Name,
                typeStr,
                nullString,
                defaultString).Trim();
        }

        public override string MakeDropRoutineScript(RoutineAttribute routine)
        {
            return string.Format(@"drop function if exists {0} cascade;", this.MakeRoutineIdentifier(routine));
        }

        public override string MakeRoutineIdentifier(RoutineAttribute routine)
        {
            var identifier = this.MakeIdentifier(routine.Schema ?? DefaultSchemaName, routine.Name);
            var parameterSection = string.Join(", ", routine.Parameters.Select(this.MakeSqlTypeString));
            return string.Format(@"{0}({1})", identifier, parameterSection);
        }

        private static Regex HoistPattern = new Regex(@"declare\s+(@\w+\s+\w+(,\s+@\w+\s+\w+)*);?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override string MakeCreateRoutineScript(RoutineAttribute routine)
        {
            var queryBody = this.MakeRoutineBody(routine);
            var identifier = this.MakeIdentifier(routine.Schema ?? DefaultSchemaName, routine.Name);
            var parameterSection = this.MakeParameterSection(routine).Replace("@", "_"); ;
            var query = string.Format(
@"create or replace function {0}(
{1}
)
    returns {2} as $$
{3}
$$ language plpgsql;",
                identifier,
                parameterSection,
                this.MakeSqlTypeString(routine),
                queryBody);
            return query;
        }

        public override string MakeRoutineBody(RoutineAttribute routine)
        {
            var queryBody = routine.Query;
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
            for (int i = 0; i < declarations.Count; ++i)
            {
                var parts = declarations[i]
                    .Trim()
                    .Split(' ', '\t', '\n', '\r')
                    .Select(p => p.Trim())
                    .Where(p => p.Length > 0)
                    .ToArray();
                parts[1] = NormalizeTypeName(parts[1].ToLower());
                declarations[i] = "\r\n\t" + string.Join(" ", parts) + ";";
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
end;", declarationString, queryBody);
            queryBody = queryBody.Replace("@", "_");
            return queryBody;
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
            var identifier = this.MakeIdentifier(schema, table.Name);
            var reset = new List<ColumnAttribute>();
            foreach (var column in table.Properties)
            {
                if (string.IsNullOrWhiteSpace(column.SqlType)
                    && column.SystemType == typeof(int)
                    && column.IsIdentity)
                {
                    column.SqlType = "serial";
                    reset.Add(column);
                }
            }
            var columnSection = this.MakeColumnSection(table, false);
            foreach (var column in reset)
            {
                column.SqlType = null;
            }
            return string.Format(@"create table if not exists {0} (
    {1}
);",
                identifier,
                columnSection);
        }

        public override string MakeDropTableScript(TableAttribute info)
        {
            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = this.MakeIdentifier(schema, info.Name);
            return string.Format("drop table if exists {0};", identifier);
        }

        public override string MakeCreateRelationshipScript(Relationship relation)
        {
            var fromColumns = string.Join(", ", relation.FromColumns.Select(c => this.MakeIdentifier(c.Name)));
            var toColumns = string.Join(", ", relation.To.PrimaryKey.KeyColumns.Select(c => this.MakeIdentifier(c.Name)));
            return string.Format(
@"alter table {0} add constraint {1}
    foreign key({2})
    references {3}({4});",
                    this.MakeIdentifier(relation.From.Schema ?? DefaultSchemaName, relation.From.Name),
                    this.MakeIdentifier(relation.GetName(this)),
                    fromColumns,
                    this.MakeIdentifier(relation.To.Schema ?? DefaultSchemaName, relation.To.Name),
                    toColumns);
        }

        public override string MakeDropRelationshipScript(Relationship relation)
        {
            return string.Format(@"alter table if exists {0} drop constraint if exists {1};",
                this.MakeIdentifier(relation.From.Schema ?? DefaultSchemaName, relation.From.Name),
                this.MakeIdentifier(relation.GetName(this)));
        }

        public override string MakeDropPrimaryKeyScript(PrimaryKey key)
        {
            return string.Format(@"alter table if exists {0} drop constraint if exists {1};
drop index if exists {2};",
                this.MakeIdentifier(key.Table.Schema ?? DefaultSchemaName, key.Table.Name),
                this.MakeIdentifier(key.GetName(this)),
                this.MakeIdentifier("idx_" + key.GetName(this)));
        }

        public override string MakeCreatePrimaryKeyScript(PrimaryKey key)
        {
            var keys = string.Join(", ", key.KeyColumns.Select(c => this.MakeIdentifier(c.Name)));
            return string.Format(
@"create unique index {0} on {1} ({2});
alter table {1} add constraint {3} primary key using index {0};",
                this.MakeIdentifier("idx_" + key.GetName(this)),
                this.MakeIdentifier(key.Table.Schema ?? DefaultSchemaName, key.Table.Name),
                keys,
                this.MakeIdentifier(key.GetName(this)));
        }

        public override string MakeCreateIndexScript(Index idx)
        {
            var columnSection = string.Join(",", idx.Columns);
            var tableName = MakeIdentifier(idx.Table.Schema ?? DefaultSchemaName, idx.Table.Name);
            return string.Format(
@"create index {0} on {1}({2});",
                idx.Name,
                tableName,
                columnSection);
        }

        public override string MakeDropIndexScript(Index idx)
        {
            return string.Format(@"drop index if exists {0};", idx.Name);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query = @"select usename from pg_catalog.pg_user")]
        public override List<string> GetDatabaseLogins()
        {
            return this.GetList<string>("usename");
        }

        public override string MakeCreateDatabaseLoginScript(string userName, string password, string database)
        {
            return string.Format("create user {0} with password '{1}'", userName, password);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select schema_name from information_schema.schemata where schema_name not like 'pg_%' and schema_name not in ('information_schema', 'public');")]
        public override List<string> GetSchemata()
        {
            return this.GetList<string>("schema_name");
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
            return this.GetList<pg_extension>();
        }
    }
}