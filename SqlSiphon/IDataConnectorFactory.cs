using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon
{
    public interface IDataConnectorFactory
    {
        IDataConnector MakeConnector(string server, string database, string userName, string password);
    }
}
