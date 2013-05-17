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
using System.Linq;

namespace SqlSiphon
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class MappedTypeAttribute : MappedObjectAttribute
    {
        public string SqlType;
        public int Size = -1;
        public int Precision = -1;
        public object DefaultValue;
        public bool IncludeInSynchronization = true;
        public bool IsOptional = false;
        public MappedTypeAttribute() { }

        public virtual string ToSqlString(System.Reflection.ParameterInfo methodParam, System.Data.Common.DbParameter procedureParam)
        {
            if (Name == null)
            {
                Name = methodParam.Name;
            }

            if (SqlType == null)
            {
                SqlType = MappedTypeAttribute.SqlTypes[methodParam.ParameterType];
            }

            if (this.Size > -1)
            {
                SqlType += "(" + this.Size.ToString();
                if (this.Precision > -1)
                {
                    SqlType += ", " + this.Precision.ToString();
                }
                SqlType += ")";
            }
            
            if (SqlType.Contains("var") && !SqlType.EndsWith(")"))
            {
                SqlType += "(MAX)";
            }
            
            if (DefaultValue == null && methodParam.IsOptional)
            {
                DefaultValue = methodParam.DefaultValue;
            }

            return string.Format("{0} {1} {2}", Name, SqlType, DefaultValue ?? "").Trim();
        }
        private static Dictionary<string, Type> typeMapping;
        private static Dictionary<Type, string> reverseTypeMapping;
        static MappedTypeAttribute()
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

            reverseTypeMapping = typeMapping
                .GroupBy(kv => kv.Value, kv => kv.Key)
                .ToDictionary(g => g.Key, g => g.First());

            reverseTypeMapping.Add(typeof(char[]), "nchar");

            reverseTypeMapping.Add(typeof(int?),  "int");
            reverseTypeMapping.Add(typeof(uint),  "int");
            reverseTypeMapping.Add(typeof(uint?), "int");

            reverseTypeMapping.Add(typeof(long?),  "bigint");
            reverseTypeMapping.Add(typeof(ulong),  "bigint");
            reverseTypeMapping.Add(typeof(ulong?), "bigint");

            reverseTypeMapping.Add(typeof(short?),  "smallint");
            reverseTypeMapping.Add(typeof(ushort),  "smallint");
            reverseTypeMapping.Add(typeof(ushort?), "smallint");

            reverseTypeMapping.Add(typeof(byte?),  "tinyint");
            reverseTypeMapping.Add(typeof(sbyte),  "tinyint");
            reverseTypeMapping.Add(typeof(sbyte?), "tinyint");
            reverseTypeMapping.Add(typeof(char),   "tinyint");
            reverseTypeMapping.Add(typeof(char?),  "tinyint");

            reverseTypeMapping.Add(typeof(decimal?), "decimal");
            reverseTypeMapping.Add(typeof(bool?), "bit");
            reverseTypeMapping.Add(typeof(float?), "float");
            reverseTypeMapping.Add(typeof(double?), "real");
            reverseTypeMapping.Add(typeof(DateTime?), "datetime2");
            reverseTypeMapping.Add(typeof(Guid?), "uniqueidentifier");
        }

        internal static SqlTypeAccessor SqlTypes = new SqlTypeAccessor();
        internal class SqlTypeAccessor
        {
            public string this[Type key]
            {
                get
                {
                    if (reverseTypeMapping.ContainsKey(key))
                        return reverseTypeMapping[key];
                    else
                        throw new Exception(string.Format("Data type {0} is not recognizable for a mapping to a SQL type", key.FullName));
                }
            }

            public bool ContainsKey(Type key)
            {
                return reverseTypeMapping.ContainsKey(key);
            }
        }

        internal static DataTypeAccessor DataTypes = new DataTypeAccessor();
        internal class DataTypeAccessor
        {
            public Type this[string key]
            {
                get
                {
                    if (typeMapping.ContainsKey(key))
                        return typeMapping[key];
                    else
                        throw new Exception(string.Format("Sql type {0} is not recognizable for a mapping to a .NET data type", key));
                }
            }
        }
    }
}
