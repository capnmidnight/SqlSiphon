using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace SqlSiphon
{
    public abstract class DataConnector : IDataConnector
    {
        private IDataConnector connector;
        public string DatabaseType
        {
            get
            {
                return this.connector.DatabaseType;
            }
        }

        protected DataConnector(IDataConnectorFactory factory, string server, string database, string userName, string password)
        {
            this.connector = factory.MakeConnector(server, database, userName, password);
        }

        public void Dispose()
        {
            this.connector.Dispose();
        }

        public ISqlSiphon GetSqlSiphon()
        {
            return (ISqlSiphon)connector;
        }

        public void Execute(params object[] parameters)
        {
            this.connector.Execute(parameters);
        }

        public EntityT Return<EntityT>(params object[] parameters)
        {
            return this.connector.Return<EntityT>(parameters);
        }

        public EntityT Get<EntityT>(params object[] parameters)
        {
            return this.connector.Get<EntityT>(parameters);
        }

        public List<EntityT> GetList<EntityT>(params object[] parameters)
        {
            return this.connector.GetList<EntityT>(parameters);
        }

        public DataSet GetDataSet(params object[] parameters)
        {
            return this.connector.GetDataSet(parameters);
        }

        public DbDataReader GetReader(params object[] parameters)
        {
            return this.connector.GetReader(parameters);
        }

        public IEnumerable<EntityT> GetEnumerator<EntityT>(params object[] parameters)
        {
            return this.connector.GetEnumerator<EntityT>(parameters);
        }
    }
}
