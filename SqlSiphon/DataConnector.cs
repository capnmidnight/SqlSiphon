using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace SqlSiphon
{
    public abstract class DataConnector : IDataConnector
    {
        public static string GetDatabaseTypeName(Type t)
        {
            var attr = Mapping.DatabaseObjectAttribute.GetAttribute<DatabaseVendorNameAttribute>(t);
            return attr?.Name;
        }

        public static bool IsNullableValueType(Type type)
        {
            return type != null
                && type.IsGenericType
                && type.Namespace == "System"
                && type.Name.StartsWith("Nullable", StringComparison.InvariantCultureIgnoreCase);
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

        public string DataSource => Connection.DataSource;

        protected DataConnector(IDataConnector connection)
        {
            Connection = connection;
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Connection.Dispose();
            }
        }

        public ISqlSiphon GetSqlSiphon()
        {
            return (ISqlSiphon)Connection;
        }

        public void Execute(params object[] parameters)
        {
            Connection.Execute(parameters);
        }

        public EntityT Return<EntityT>(params object[] parameters)
        {
            return Connection.Return<EntityT>(parameters);
        }

        public EntityT Get<EntityT>(params object[] parameters)
        {
            return Connection.Get<EntityT>(parameters);
        }

        public List<EntityT> GetList<EntityT>(params object[] parameters)
        {
            return Connection.GetList<EntityT>(parameters);
        }

        public DataSet GetDataSet(params object[] parameters)
        {
            return Connection.GetDataSet(parameters);
        }

        public DbDataReader GetReader(params object[] parameters)
        {
            return Connection.GetReader(parameters);
        }

        public IEnumerable<EntityT> GetEnumerator<EntityT>(params object[] parameters)
        {
            return Connection.GetEnumerator<EntityT>(parameters);
        }

        public void InsertAll(Type t, System.Collections.IEnumerable data)
        {
            Connection.InsertAll(t, data);
        }
    }
}
