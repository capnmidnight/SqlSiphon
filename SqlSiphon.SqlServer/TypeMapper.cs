using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.SqlServer
{
    public class TypeMapper : MappedTypeAttribute
    {
        private static Dictionary<string, Type> typeMapping;
        private static Dictionary<Type, string> reverseTypeMapping;
        static TypeMapper()
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

        private TypeMapper(MappedTypeAttribute attr)
        {
            this.Name = attr.Name;
            this.DefaultValue = attr.DefaultValue;
            this.IncludeInSynchronization = attr.IncludeInSynchronization;
            this.IsOptional = attr.IsOptional;
            this.Precision = attr.Precision;
            this.Size = attr.Size;
            this.SqlType = attr.SqlType;
        }

        protected TypeMapper(MappedTypeAttribute attr, ParameterInfo member)
            : this(attr)
        {
            this.Name = this.Name ?? member.Name;
            this.SqlType = this.SqlType ?? SqlTypes[member.ParameterType];
        }

        public TypeMapper(MappedTypeAttribute attr, PropertyInfo member)
            : this(attr)
        {
            this.Name = attr.Name ?? member.Name;
            this.SqlType = this.SqlType ?? SqlTypes[member.PropertyType];
        }

        public TypeMapper(MappedTypeAttribute attr, FieldInfo member)
            : this(attr)
        {
            this.Name = this.Name ?? member.Name;
            this.SqlType = this.SqlType ?? SqlTypes[member.FieldType];
        }

        public object Convert(string sqlType, object value)
        {
            return System.Convert.ChangeType(value, DataTypes[sqlType]);
        }

        public override string ToString()
        {
            var typeStr = new StringBuilder(this.SqlType);
            if (this.Size > -1)
            {
                typeStr.AppendFormat("({0}", this.Size);
                if (this.Precision > -1)
                {
                    typeStr.AppendFormat(", {0}", this.Precision);
                }
                typeStr.Append(")");
            }

            if (SqlType.Contains("var") && !SqlType.EndsWith(")"))
            {
                typeStr.Append("(MAX)");
            }
            return string.Format("{0} {1} {2}", Name, typeStr, DefaultValue ?? "").Trim();
        }
    }
}
