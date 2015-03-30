using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Runtime.CompilerServices;
using SqlSiphon;
using SqlSiphon.Mapping;
using SqlSiphon.Postgres;

namespace SqlSiphon.Examples.Postgres
{
    public class PostgresDAL : BasicDAL
    {
        public PostgresDAL(string server, string database, string userName, string password)
            : base(new PostgresDataConnectorFactory(), server, database, userName, password)
        {
        }
    }
}
