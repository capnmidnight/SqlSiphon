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
    public partial class NpgsqlDataAccessLayer : DataAccessLayer<NpgsqlConnection, NpgsqlCommand, NpgsqlParameter, NpgsqlDataAdapter, NpgsqlDataReader>
    {
        private static Dictionary<string, Type> typeMapping;
        private static Dictionary<Type, string> reverseTypeMapping;
        static NpgsqlDataAccessLayer()
        {
            typeMapping = new Dictionary<string, Type>();

            typeMapping.Add("bigint", typeof(long));
            typeMapping.Add("int8", typeof(long));
            typeMapping.Add("bigserial", typeof(long));
            typeMapping.Add("serial8", typeof(long));

            typeMapping.Add("bit", typeof(bool[]));
            typeMapping.Add("varbit", typeof(bool[]));

            typeMapping.Add("bit varying", typeof(List<bool>));

            typeMapping.Add("boolean", typeof(bool));
            typeMapping.Add("bool", typeof(bool));

            typeMapping.Add("bytea", typeof(byte[]));

            typeMapping.Add("text", typeof(string));
            typeMapping.Add("character varying", typeof(string));
            typeMapping.Add("varchar", typeof(string));
            typeMapping.Add("_varchar", typeof(string));
            typeMapping.Add("tsquery", typeof(string));
            typeMapping.Add("xml", typeof(string));
            typeMapping.Add("json", typeof(string));
            typeMapping.Add("name", typeof(string));

            typeMapping.Add("character", typeof(char[]));
            typeMapping.Add("char", typeof(char[]));

            typeMapping.Add("inet", typeof(System.Net.IPAddress));
            typeMapping.Add("cidr", typeof(System.Net.IPAddress));

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

            typeMapping.Add("box", typeof(string));
            typeMapping.Add("circle", typeof(string));
            typeMapping.Add("line", typeof(string));
            typeMapping.Add("lseg", typeof(string));
            typeMapping.Add("macaddr", typeof(string));
            typeMapping.Add("path", typeof(string));
            typeMapping.Add("point", typeof(string));
            typeMapping.Add("polygon", typeof(string));
            typeMapping.Add("geometry", typeof(string));
            typeMapping.Add("geography", typeof(string));
            typeMapping.Add("tsvector", typeof(string));

            //typeMapping.Add("txid_snapshot", typeof(string));

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
        public NpgsqlDataAccessLayer(string connectionString)
            : base(connectionString)
        {
        }

        public NpgsqlDataAccessLayer(NpgsqlConnection connection)
            : base(connection)
        {
        }

        public NpgsqlDataAccessLayer(NpgsqlDataAccessLayer dal)
            : base(dal.Connection)
        {
        }


        protected override string IdentifierPartBegin { get { return "\""; } }
        protected override string IdentifierPartEnd { get { return "\""; } }
        public override string DefaultSchemaName { get { return "public"; } }

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

        protected override DatabaseState GetFinalState()
        {
            var state = base.GetFinalState();
            state.AddType(typeof(Memberships.aspnet_Applications), this);
            state.AddType(typeof(Memberships.aspnet_Membership), this);
            state.AddType(typeof(Memberships.aspnet_Roles), this);
            state.AddType(typeof(Memberships.aspnet_SchemaVersions), this);
            state.AddType(typeof(Memberships.aspnet_Users), this);
            state.AddType(typeof(Memberships.aspnet_UsersInRoles), this);
            return state;
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

        public override string MakeCreateColumnScript(MappedPropertyAttribute prop)
        {
            return string.Format("alter table if exists {0} add column {1} {2};",
                this.MakeIdentifier(prop.Table.Schema ?? DefaultSchemaName, prop.Table.Name),
                prop.Name,
                this.MakeSqlTypeString(prop));
        }

        public override string MakeDropColumnScript(MappedPropertyAttribute prop)
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
            var attr = MappedObjectAttribute.GetAttribute<MappedClassAttribute>(elemType);
            if (attr != null)
            {
                attr.InferProperties(elemType);
                if (systemType.IsArray)
                {
                    sqlType = attr.Name + "[]";
                }
                else
                {
                    sqlType = string.Format("TABLE ({0})", this.MakeColumnSection(attr, true));
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
                attr = new MappedClassAttribute();
                attr.InferProperties(systemType);
                sqlType = string.Format("TABLE ({0})", this.MakeColumnSection(attr, true));
            }
            else
            {
                sqlType = "void";
            }
            return sqlType;
        }

        public override string MakeAlterColumnScript(MappedPropertyAttribute final, MappedPropertyAttribute initial)
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

        public override bool ColumnChanged(MappedPropertyAttribute final, MappedPropertyAttribute initial)
        {
            var tests = new bool[]{
                final.Include == initial.Include,
                final.IsIdentity == initial.IsIdentity,
                final.IsOptional == initial.IsOptional,
                final.Name.ToLower() == initial.Name.ToLower(),
                final.SystemType == initial.SystemType,
                final.Table != null,
                initial.Table != null,
                final.Table.Schema.ToLower() == initial.Table.Schema.ToLower(),
                final.Table.Name.ToLower() == initial.Table.Name.ToLower()
            };
            var unchanged = tests.Aggregate((a, b) => a && b);
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
                    unchanged = unchanged && valuesMatch;
                }

                if (final.Size != initial.Size)
                {
                    if (final.SystemType != typeof(double) || final.IsSizeSet || initial.Size != 53)
                    {
                        unchanged = false;
                    }
                }
            }
            return !unchanged;
        }

        protected override string MakeParameterString(MappedParameterAttribute param)
        {
            var dirString = "";
            var typeStr = MakeSqlTypeString(param);
            switch (param.Direction)
            {
                case ParameterDirection.InputOutput:
                    dirString = "INOUT";
                    break;
                case ParameterDirection.Output:
                    dirString = "OUT";
                    break;
            }

            var defaultString = "";
            if (param.DefaultValue != null)
                defaultString = " DEFAULT = " + param.DefaultValue.ToString();

            return string.Format("{0} @{1} {2}{3}", dirString, param.Name, typeStr, defaultString);
        }

        protected override string MakeColumnString(MappedPropertyAttribute column, bool isReturnType)
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
                    defaultString = "DEFAULT " + val;
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

        public override string MakeDropRoutineScript(MappedMethodAttribute routine)
        {
            var identifier = this.MakeIdentifier(routine.Schema ?? DefaultSchemaName, routine.Name);
            var parameterSection = string.Join(", ", routine.Parameters.Select(this.MakeSqlTypeString));
            return string.Format(@"drop function if exists {0}({1}) cascade;",
                identifier,
                parameterSection);
        }

        private static Regex HoistPattern = new Regex(@"declare\s+(@\w+\s+\w+(,\s+@\w+\s+\w+)*);?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override string MakeCreateRoutineScript(MappedMethodAttribute routine)
        {
            var query = routine.Query;
            var declarations = new List<string>();
            query = HoistPattern.Replace(query, new MatchEvaluator(m =>
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
                parts[1] = parts[1].ToLower();
                if (typeMapping.ContainsKey(parts[1]) && reverseTypeMapping.ContainsKey(typeMapping[parts[1]]))
                {
                    parts[1] = reverseTypeMapping[typeMapping[parts[1]]];
                }
                declarations[i] = "\r\n\t" + string.Join(" ", parts) + ";";
            }
            var declarationString = "";
            if (declarations.Count > 0)
            {
                declarationString = "declare " + string.Join("", declarations);
            }
            
            var identifier = this.MakeIdentifier(routine.Schema ?? DefaultSchemaName, routine.Name);
            var parameterSection = this.MakeParameterSection(routine);
            query = this.MakeDropRoutineScript(routine) + string.Format(
@"create or replace function {0}(
{1}
)
    returns {2} as $$
{3}
begin
{4}
end;
$$ language plpgsql;",
                identifier,
                parameterSection,
                this.MakeSqlTypeString(routine),
                declarationString,
                query);
            query = query.Replace("@", "P_");
            return query;
        }

        public override string MakeAlterRoutineScript(MappedMethodAttribute routine)
        {
            return this.MakeDropRoutineScript(routine) + "\n" + this.MakeCreateRoutineScript(routine);
        }

        public override string MakeCreateTableScript(MappedClassAttribute table)
        {
            var schema = table.Schema ?? DefaultSchemaName;
            var identifier = this.MakeIdentifier(schema, table.Name);
            var reset = new List<MappedPropertyAttribute>();
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

        public override string MakeDropTableScript(MappedClassAttribute info)
        {
            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = this.MakeIdentifier(schema, info.Name);
            return string.Format("drop table if exists {0};", identifier);
        }

        public override string MakeCreateRelationshipScript(Relationship relation)
        {
            var fromColumns = string.Join(", ", relation.FromColumns.Select(c => this.MakeIdentifier(c.Name)));
            var toColumns = string.Join(", ", relation.To.KeyColumns.Select(c => this.MakeIdentifier(c.Name)));
            return string.Format(
@"alter table {0} add constraint {1}
    foreign key({2})
    references {3}({4});",
                    this.MakeIdentifier(relation.From.Schema ?? DefaultSchemaName, relation.From.Name),
                    this.MakeIdentifier(relation.GetName(this)),
                    fromColumns,
                    this.MakeIdentifier(relation.To.Table.Schema ?? DefaultSchemaName, relation.To.Table.Name),
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

        protected override string MakeCreateIndexScript(string indexName, string tableSchema, string tableName, string[] tableColumns)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(CommandType = CommandType.Text, Query =
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
        [MappedMethod(CommandType = CommandType.Text, Query =
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
        [MappedMethod(CommandType = CommandType.Text, Query =
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
        [MappedMethod(CommandType = CommandType.Text, Query =
@"select *
from information_schema.routines
where specific_schema != 'information_schema'
    and specific_schema != 'pg_catalog'
order by specific_catalog, specific_schema, specific_name;")]
        public override List<InformationSchema.Routines> GetRoutines()
        {
            return GetList<InformationSchema.Routines>();
        }


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(CommandType = CommandType.Text, Query =
@"select *
from information_schema.parameters
where specific_schema != 'information_schema'
    and specific_schema != 'pg_catalog'
order by specific_catalog, specific_schema, specific_name, ordinal_position;")]
        public override List<InformationSchema.Parameters> GetParameters()
        {
            return GetList<InformationSchema.Parameters>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(CommandType = CommandType.Text, Query =
@"select * 
from information_schema.constraint_column_usage
where constraint_schema != 'information_schema'
    and constraint_schema != 'pg_catalog';")]
        public override List<InformationSchema.ConstraintColumnUsage> GetConstraintColumns()
        {
            return GetList<InformationSchema.ConstraintColumnUsage>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(CommandType = CommandType.Text, Query =
@"select * 
from information_schema.key_column_usage
where constraint_schema != 'information_schema'
    and constraint_schema != 'pg_catalog';")]
        public override List<InformationSchema.KeyColumnUsage> GetKeyColumns()
        {
            return GetList<InformationSchema.KeyColumnUsage>();
        }
    }
}