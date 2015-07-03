using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace SqlSiphon
{
    public interface IDataConnector : IDisposable
    {
        string DataSource { get; }
        void Execute(params object[] parameters);
        EntityT Return<EntityT>(params object[] parameters);
        EntityT Get<EntityT>(params object[] parameters);
        List<EntityT> GetList<EntityT>(params object[] parameters);
        DataSet GetDataSet(params object[] parameters);
        DbDataReader GetReader(params object[] parameters);
        IEnumerable<EntityT> GetEnumerator<EntityT>(params object[] parameters);
        void InsertAll(Type t, System.Collections.IEnumerable data);
    }

    public static class IDataConnectorExt
    {
        public static void InsertAll<T>(this IDataConnector connector, IEnumerable<T> data)
        {
            connector.InsertAll(typeof(T), data);
        }

        public static void InsertOne<T>(this IDataConnector connector, T obj)
        {
            connector.InsertAll(typeof(T), new T[] { obj });
        }
    }
}
