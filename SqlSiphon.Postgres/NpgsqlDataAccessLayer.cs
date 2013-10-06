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
    public abstract class NpgsqlDataAccessLayer : DataAccessLayer<NpgsqlConnection, NpgsqlCommand, NpgsqlParameter, NpgsqlDataAdapter, NpgsqlDataReader>
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
            typeMapping.Add("bit varying", typeof(List<bool>));
            typeMapping.Add("varbit", typeof(bool[]));
            typeMapping.Add("bool", typeof(bool));
            typeMapping.Add("boolean", typeof(bool));
            //typeMapping.Add("box", typeof());
            typeMapping.Add("bytea", typeof(byte[]));
            typeMapping.Add("varchar", typeof(string));
            typeMapping.Add("character varying", typeof(string));
            typeMapping.Add("char", typeof(char[]));
            typeMapping.Add("character", typeof(char[]));
            typeMapping.Add("cidr", typeof(System.Net.IPAddress));
            //typeMapping.Add("circle", typeof());
            typeMapping.Add("date", typeof(DateTime));
            typeMapping.Add("double precision", typeof(double));
            typeMapping.Add("float8", typeof(double));
            typeMapping.Add("inet", typeof(System.Net.IPAddress));
            typeMapping.Add("int", typeof(int));
            typeMapping.Add("integer", typeof(int));
            typeMapping.Add("int4", typeof(int));
            typeMapping.Add("interval", typeof(TimeSpan));
            //typeMapping.Add("line", typeof());
            //typeMapping.Add("lseg", typeof());
            //typeMapping.Add("macaddr", typeof());
            typeMapping.Add("money", typeof(decimal));
            typeMapping.Add("numeric", typeof(decimal));
            //typeMapping.Add("path", typeof(decimal));
            //typeMapping.Add("point", typeof(decimal));
            //typeMapping.Add("polygon", typeof(decimal));
            typeMapping.Add("real", typeof(float));
            typeMapping.Add("float4", typeof(float));
            typeMapping.Add("smallint", typeof(short));
            typeMapping.Add("int2", typeof(short));
            typeMapping.Add("smallserial", typeof(short));
            typeMapping.Add("serial2", typeof(short));
            typeMapping.Add("serial", typeof(int));
            typeMapping.Add("serial4", typeof(int));
            typeMapping.Add("text", typeof(string));
            typeMapping.Add("time", typeof(DateTime));
            typeMapping.Add("time with time zone", typeof(DateTime));
            typeMapping.Add("timestamp", typeof(DateTime));
            typeMapping.Add("timestamp with time zone", typeof(DateTime));
            typeMapping.Add("tsquery", typeof(string));
            //typeMapping.Add("tsvector", typeof());
            //typeMapping.Add("txid_snapshot", typeof());
            typeMapping.Add("uuid", typeof(Guid));
            typeMapping.Add("xml", typeof(string));
            typeMapping.Add("json", typeof(string));



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

        protected override string IdentifierPartBegin { get { return "\""; } }
        protected override string IdentifierPartEnd { get { return "\""; } }
        protected override string DefaultSchemaName { get { return "public"; } }

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

        protected override string MakeSqlTypeString(MappedTypeAttribute type)
        {
            string typeName = null;
            if (type == null)
                throw new Exception("Couldn't find type description! You gave me nothing to go on.");

            if (type.SqlType != null)
                typeName = type.SqlType;
            else if (type.SystemType != null)
            {
                var t = type.SystemType;
                if (t.IsGenericType)
                {
                    var subTypes = type.SystemType.GetGenericArguments();
                    if (subTypes.Length > 1)
                        throw new Exception("Type is too complex!");
                    t = subTypes.First();
                }

                var temp = MappedObjectAttribute.GetAttribute<MappedTypeAttribute>(t);
                if (temp != null)
                    typeName = MakeSqlTypeString(temp);
                else if (reverseTypeMapping.ContainsKey(t))
                    typeName = reverseTypeMapping[t];
                else if (type.SystemType.Name != "Void")
                    throw new Exception("Couldn't find type description!");
            }

            if (type.IsSizeSet)
            {
                var format = type.IsPrecisionSet
                    ? "{0}[{1},{2}]"
                    : "{0}[{1}]";
                typeName = string.Format(format, typeName, type.Size, type.Precision);
            }
            return typeName;
        }

        protected override string MakeParameterString(MappedParameterAttribute p)
        {
            var dirString = "";
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

            return string.Format("{0} {1} {2}{3}", dirString, p.Name, p.SqlType, defaultString);
        }

        protected override string BuildDropProcedureScript(MappedMethodAttribute info)
        {
            throw new NotImplementedException();
        }

        protected override string BuildCreateProcedureScript(MappedMethodAttribute info)
        {
            var identifier = this.MakeIdentifier(info.Schema, info.Name);
            var parameterSection = this.MakeParameterSection(info);
            return string.Format(
@"create or replace function {0}({1})
    returns {2}{3} as $$
{4}
$$ language 'sql'",
                   identifier,
                   parameterSection,
                   info.ReturnsMany ? "setof " : "",
                   info.SqlType,
                   info.Query);
        }

        protected override bool ProcedureExists(MappedMethodAttribute info)
        {
            // just assume the procedure exists, because the drop and create
            // procedures will take care of it.
            return true;
        }
    }
}