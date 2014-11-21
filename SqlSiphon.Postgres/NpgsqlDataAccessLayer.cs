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

            typeMapping.Add("character", typeof(char[]));
            typeMapping.Add("char", typeof(char[]));

            typeMapping.Add("inet", typeof(System.Net.IPAddress));
            typeMapping.Add("cidr", typeof(System.Net.IPAddress));

            typeMapping.Add("date", typeof(DateTime));

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

            //typeMapping.Add("box", typeof());
            //typeMapping.Add("circle", typeof());
            //typeMapping.Add("line", typeof());
            //typeMapping.Add("lseg", typeof());
            //typeMapping.Add("macaddr", typeof());
            //typeMapping.Add("path", typeof(decimal));
            //typeMapping.Add("point", typeof(decimal));
            //typeMapping.Add("polygon", typeof(decimal));
            //typeMapping.Add("tsvector", typeof());
            //typeMapping.Add("txid_snapshot", typeof());

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
            return base.MakeIdentifier(parts).ToLower();
        }

        protected override void ModifyQuery(MappedMethodAttribute info)
        {
            base.ModifyQuery(info);
            //convert a parameterized query likely written for SQL Server 
            // or MySQL to a format useable on Postgres
            var parameters = info.Parameters
                .OrderBy(p => p.Name.Length)
                .Reverse();
            foreach (var param in parameters)
                info.Query = info.Query.Replace("@" + param.Name, ":" + param.Name);
        }

        public override Type GetSystemType(string sqlType)
        {
            return typeMapping.ContainsKey(sqlType) ? typeMapping[sqlType] : null;
        }

        protected override string MakeSqlTypeString(string sqlType, Type systemType, int? size, int? precision)
        {
            if (systemType.BaseType == typeof(SqlSiphon.InformationSchema.Columns))
            {
                systemType = typeof(NpgsqlDataAccessLayer).BaseType.GetGenericArguments()[systemType.GenericParameterPosition];
            }
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
                        throw new Exception("Couldn't find type description!");
                    }
                }
            }

            if (size.HasValue && typeName.IndexOf("[") == -1)
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

            var temp = MappedObjectAttribute.GetAttribute<MappedClassAttribute>(t);
            if (temp != null)
            {
                return MakeSqlTypeString(temp);
            }
            else if (reverseTypeMapping.ContainsKey(t))
            {
                return reverseTypeMapping[t];
            }
            else
            {
                return null;
            }
        }

        private string MakeComplexSqlTypeString(Type systemType)
        {
            string sqlType = null;
            var isRef = systemType.Name.Last() == '&';
            var elemType = systemType.IsArray || isRef ? systemType.GetElementType() : systemType;
            var attr = MappedObjectAttribute.GetAttribute<MappedClassAttribute>(elemType);
            if (attr != null)
            {
                attr.InferProperties(elemType);
                if(systemType.IsArray)
                {
                    sqlType = attr.Name + "[]";
                }
                else
                {
                    sqlType = string.Format("TABLE ({0})", this.MakeColumnSection(attr));
                }
            }
            else if(reverseTypeMapping.ContainsKey(elemType))
            {
                sqlType = reverseTypeMapping[elemType];
                if (systemType.IsArray)
                {
                    sqlType += "[]";
                }
                else if (isRef)
                {
                    //sqlType = attr.Name;
                }
                else
                {
                }
            }
            return sqlType;
        }

        public override bool DescribesIdentity(ref string defaultValue)
        {
            if (defaultValue != null && defaultValue.IndexOf("nextval") == 0)
            {
                defaultValue = null;
                return true;
            }
            return false;
        }

        protected override string MakeIndexScript(string indexName, string tableSchema, string tableName, string[] tableColumns)
        {
            throw new NotImplementedException();
        }

        public override string MakeCreateColumnScript(MappedPropertyAttribute prop)
        {
            return string.Format("alter table if exists {0} add column {1} {2}",
                this.MakeIdentifier(prop.Table.Schema, prop.Table.Name),
                prop.Name,
                this.MakeSqlTypeString(prop));
        }

        public override string MakeDropColumnScript(MappedPropertyAttribute prop)
        {
            return string.Format("alter table if exists {0} drop column if exists {1};",
                this.MakeIdentifier(prop.Table.Schema, prop.Table.Name),
                this.MakeIdentifier(prop.Name));
        }

        public override string MakeAlterColumnScript(MappedPropertyAttribute final, MappedPropertyAttribute initial)
        {
            var preamble = string.Format(
                "alter table if exists {0}",
                this.MakeIdentifier(final.Table.Schema ?? DefaultSchemaName, final.Table.Name));

            if (final.Include != initial.Include)
            {
                return string.Format(
                    "{0} {1} column {2} {3}",
                    preamble,
                    final.Include ? "add" : "drop",
                    this.MakeIdentifier(final.Name),
                    final.Include ? this.MakeSqlTypeString(final) : "")
                    .Trim();
            }
            else if (final.Name.ToLower() != initial.Name.ToLower())
            {
                // this shouldn't happen, but maybe some day we will develop a way to keep track
                // of a column changing its name over time. Maybe we can generate some kind of
                // unique id for a column, and then always know what that column refers to, even
                // if the name changes, but for now, we can't.
                return string.Format(
                    "{0} rename column {1} to {2}",
                    preamble,
                    this.MakeIdentifier(initial.Name),
                    this.MakeIdentifier(final.Name));
            }
            else if (final.DefaultValue != initial.DefaultValue)
            {
                return string.Format(
                    "{0} alter column {1} {2} default {3}",
                    preamble,
                    this.MakeIdentifier(final.Name),
                    final.DefaultValue == null ? "drop" : "set",
                    final.DefaultValue ?? "")
                    .Trim();
            }
            else if (final.IsOptional != initial.IsOptional)
            {
                return string.Format(
                    "{0} alter column {1} {2} not null",
                    preamble,
                    this.MakeIdentifier(final.Name),
                    final.IsOptional ? "drop" : "set");
            }
            else if (final.SystemType != initial.SystemType
                || final.Size != initial.Size
                || final.Precision != initial.Precision)
            {
                return string.Format(
                    "{0} alter column {1} set data type {2}",
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

        public override bool ColumnChanged(MappedPropertyAttribute final, MappedPropertyAttribute intial)
        {
            var tests = new bool[]{
                final.Include == intial.Include,
                final.IncludeInPrimaryKey == intial.IncludeInPrimaryKey,
                final.IsIdentity == intial.IsIdentity,
                final.IsOptional == intial.IsOptional,
                final.Name.ToLower() == intial.Name.ToLower(),
                final.SystemType == intial.SystemType,
                final.Table != null,
                intial.Table != null,
                final.Table.Schema.ToLower() == intial.Table.Schema.ToLower(),
                final.Table.Name.ToLower() == intial.Table.Name.ToLower()
            };
            var unchanged = tests.Aggregate((a, b) => a && b);
            if (final.SystemType == intial.SystemType){
                if(final.DefaultValue != null 
                    && intial.DefaultValue != null 
                    && final.DefaultValue != intial.DefaultValue)
                {
                    bool valuesMatch;
                    if (final.SystemType == typeof(bool))
                    {
                        var xb = final.DefaultValue.Replace("'", "").ToLower();
                        var xbb = xb == "true" || xb == "1";
                        var yb = intial.DefaultValue.Replace("'", "").ToLower();
                        var ybb = yb == "true" || yb == "1";
                        valuesMatch = xbb == ybb;
                    }
                    else if (final.SystemType == typeof(DateTime))
                    {
                        valuesMatch = final.DefaultValue == "'9999/12/31 23:59:59.99999'" && intial.DefaultValue == "'9999-12-31'::date"
                                || final.DefaultValue == "getdate()" && intial.DefaultValue == "('now'::text)::date";
                    }
                    else if (final.SystemType == typeof(double))
                    {
                        valuesMatch = final.DefaultValue == "(" + intial.DefaultValue + ")" 
                            || intial.DefaultValue == "(" + final.DefaultValue + ")";
                    }
                    else
                    {
                        valuesMatch = false;
                    }

                    if (valuesMatch)
                    {
                        intial.DefaultValue = final.DefaultValue;
                    }
                    unchanged = unchanged && valuesMatch;
                }
                
                if (final.Size != intial.Size)
                {
                    if (final.SystemType != typeof(double) || final.IsSizeSet || intial.Size != 53)
                    {
                        unchanged = false;
                    }
                }

                if (final.Precision != intial.Precision)
                {
                }
            }
            return !unchanged;
        }

        protected override string MakeParameterString(MappedParameterAttribute p)
        {
            var dirString = "";
            var typeStr = MakeSqlTypeString(p);
            switch (p.Direction)
            {
                case ParameterDirection.Input:
                    dirString = "IN";
                    break;
                case ParameterDirection.InputOutput:
                    dirString = "INOUT";
                    break;
                case ParameterDirection.Output:
                    dirString = "OUT";
                    break;
            }

            var defaultString = "";
            if (p.DefaultValue != null)
                defaultString = " DEFAULT = " + p.DefaultValue.ToString();

            return string.Format("{0} {1} {2}{3}", dirString, p.Name, typeStr, defaultString);
        }

        protected override string MakeColumnString(MappedPropertyAttribute p)
        {
            var typeStr = MakeSqlTypeString(p);
            var defaultString = "";
            if (p.DefaultValue != null)
            {
                var val = p.DefaultValue.ToString();
                if (val.ToLower() == "getdate()")
                {
                    val = "current_date";
                }
                defaultString = "DEFAULT " + val;
            }
            else if (p.IsIdentity)
            {
                typeStr = typeStr.Replace("integer", "serial");
            }

            return string.Format("{0} {1} {2} {3}",
                p.Name,
                typeStr,
                p.IsOptional ? "NULL" : "NOT NULL",
                defaultString);
        }

        public override string MakeDropRoutineScript(MappedMethodAttribute info)
        {
            var identifier = this.MakeIdentifier(info.Schema, info.Name);
            var parameterSection = string.Join(", ", info.Parameters.Select(p => p.SqlType));
            return string.Format(@"drop function if exists {0}({1}) cascade;",
                identifier,
                parameterSection);
        }

        public override string MakeCreateRoutineScript(MappedMethodAttribute info)
        {
            var identifier = this.MakeIdentifier(info.Schema ?? DefaultSchemaName, info.Name);
            var parameterSection = this.MakeParameterSection(info);
            return string.Format(
@"create or replace function {0}({1})
    returns {2} as $$
{3}
$$ language 'sql'",
                identifier,
                parameterSection,
                info.SqlType ?? "void",
                info.Query);
        }

        public override string MakeCreateTableScript(MappedClassAttribute info)
        {
            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = this.MakeIdentifier(schema, info.Name);
            var columnSection = this.MakeColumnSection(info);
            var pk = info.Properties.Where(p => p.IncludeInPrimaryKey).ToArray();
            var pkString = "";
            if (pk.Length > 0)
            {
                pkString = string.Format(",{0}    constraint PK_{1}_{2} primary key({3}){0}",
                    Environment.NewLine,
                    schema,
                    info.Name,
                    string.Join(",", pk.Select(c => c.Name)));
            }
            return string.Format(@"create table if not exists {0} (
    {1}{2})",
                identifier,
                columnSection,
                pkString);
        }

        public override string MakeDropTableScript(MappedClassAttribute info)
        {
            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = this.MakeIdentifier(schema, info.Name);
            return "drop table if exists " + identifier;
        }

        public override string MakeCreateRelationshipScript(Relationship relation)
        {
            var fromColumns = string.Join(", ", relation.FromColumns.Select(c => this.MakeIdentifier(c.Name)));
            var toColumns = string.Join(", ", relation.ToColumns.Select(c => this.MakeIdentifier(c.Name)));

            return string.Format(
@"alter table {0} add constraint {1}
    foreign key({2})
    references {3}({4})",
                    this.MakeIdentifier(relation.From.Schema, relation.From.Name),
                    this.MakeIdentifier(relation.Name),
                    fromColumns,
                    this.MakeIdentifier(relation.To.Schema, relation.To.Name),
                    toColumns);
        }

        public override string MakeDropRelationshipScript(Relationship relation)
        {
            return string.Format(@"alter table {0} drop constraint {1}",
                    this.MakeIdentifier(relation.From.Schema, relation.From.Name),
                    this.MakeIdentifier(relation.Name));
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(CommandType = CommandType.Text, Query =
@"select *
from information_schema.columns
where table_schema != 'information_schema'
    and table_schema != 'pg_catalog'
order by table_catalog, table_schema, table_name, ordinal_position")]
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
order by table_catalog, table_schema, table_name")]
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
order by constraint_catalog, constraint_schema, constraint_name")]
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
order by specific_catalog, specific_schema, specific_name")]
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
order by specific_catalog, specific_schema, specific_name, ordinal_position")]
        public override List<InformationSchema.Parameters> GetParameters()
        {
            return GetList<InformationSchema.Parameters>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(CommandType = CommandType.Text, Query =
@"select * 
from information_schema.constraint_column_usage
where constraint_schema != 'information_schema'
    and constraint_schema != 'pg_catalog'")]
        public override List<InformationSchema.ConstraintColumnUsage> GetColumnConstraints()
        {
            return GetList<InformationSchema.ConstraintColumnUsage>();
        }
    }
}