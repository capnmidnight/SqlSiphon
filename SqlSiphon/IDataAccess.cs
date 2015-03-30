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
        string DatabaseType { get; }
        void Execute(params object[] parameters);
        EntityT Return<EntityT>(params object[] parameters);
        EntityT Get<EntityT>(params object[] parameters);
        List<EntityT> GetList<EntityT>(params object[] parameters);
        DataSet GetDataSet(params object[] parameters);
        DbDataReader GetReader(params object[] parameters);
        IEnumerable<EntityT> GetEnumerator<EntityT>(params object[] parameters);
    }
}
