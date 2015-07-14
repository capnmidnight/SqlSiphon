using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace SqlSiphon
{
    public abstract class DataConnector : IDataConnector
    {
        public static string GetDatabaseTypeName(Type t)
        {
            var attr = Mapping.DatabaseObjectAttribute.GetAttribute<DatabaseVendorNameAttribute>(t);
            return attr != null ? attr.Name : null;
        }

        public static bool IsNullableValueType(Type type)
        {
            return type != null 
                && type.IsGenericType 
                && type.Namespace == "System" 
                && type.Name.StartsWith("Nullable");
        }

        public static Type CoalesceNullableValueType(Type type)
        {
            return IsNullableValueType(type) ? type.GetGenericArguments().First() : type;
        }

        public static bool IsTypeBarePrimitive(Type type)
        {
            type = CoalesceNullableValueType(type);
            return type.IsPrimitive
                || type == typeof(decimal);
        }

        public static bool IsTypeQuotedPrimitive(Type type)
        {
            type = CoalesceNullableValueType(type);
            return type == typeof(string)
                || type == typeof(DateTime)
                || type == typeof(Guid)
                || type == typeof(byte[]);
        }

        public static bool IsTypePrimitive(Type type)
        {
            return IsTypeBarePrimitive(type)
                || IsTypeQuotedPrimitive(type);
        }

        public static bool IsTypeCollection(Type type)
        {
            return type != null && type.GetInterfaces().Contains(typeof(System.Collections.IEnumerable));
        }

        public static Type CoalesceCollectionType(Type type)
        {
            type = CoalesceNullableValueType(type);
            if (IsTypeCollection(type))
            {
                if (type.IsArray)
                {
                    type = type.GetElementType();
                }
                else if (type.IsGenericType)
                {
                    type = type.GetGenericArguments().First();
                }
            }
            return type;
        }

        private IDataConnector connectionInternal;
        public IDataConnector Connection
        {
            get
            {
                return connectionInternal;
            }
            set
            {
                if (connectionInternal != null)
                {
                    connectionInternal.Dispose();
                }
                connectionInternal = value;
            }
        }

        public string DataSource
        {
            get
            {
                return this.Connection.DataSource;
            }
        }

        protected DataConnector(IDataConnector connection)
        {
            this.Connection = connection;
        }

        protected DataConnector(IDataConnectorFactory factory, string connectionString)
            : this(factory.MakeConnector(connectionString))
        {
        }

        protected DataConnector(IDataConnectorFactory factory, string server, string database, string userName, string password)
            : this(factory.MakeConnector(server, database, userName, password))
        {
        }

        protected DataConnector() : this(null) { }

        public void Dispose()
        {
            this.Connection.Dispose();
        }

        public ISqlSiphon GetSqlSiphon()
        {
            return (ISqlSiphon)Connection;
        }

        public void Execute(params object[] parameters)
        {
            this.Connection.Execute(parameters);
        }

        public EntityT Return<EntityT>(params object[] parameters)
        {
            return this.Connection.Return<EntityT>(parameters);
        }

        public EntityT Get<EntityT>(params object[] parameters)
        {
            return this.Connection.Get<EntityT>(parameters);
        }

        public List<EntityT> GetList<EntityT>(params object[] parameters)
        {
            return this.Connection.GetList<EntityT>(parameters);
        }

        public DataSet GetDataSet(params object[] parameters)
        {
            return this.Connection.GetDataSet(parameters);
        }

        public DbDataReader GetReader(params object[] parameters)
        {
            return this.Connection.GetReader(parameters);
        }

        public IEnumerable<EntityT> GetEnumerator<EntityT>(params object[] parameters)
        {
            return this.Connection.GetEnumerator<EntityT>(parameters);
        }

        public void InsertAll(Type t, System.Collections.IEnumerable data)
        {
            this.Connection.InsertAll(t, data);
        }
    }
}
