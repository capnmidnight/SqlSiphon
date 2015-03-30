﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Runtime.CompilerServices;
using SqlSiphon;
using SqlSiphon.Mapping;
using SqlSiphon.SqlServer;

namespace SqlSiphon.Examples.SqlServer
{
    public class SqlServerDAL: BasicDAL<SqlServerDataConnectorFactory>
    {
        public SqlServerDAL(string server, string database, string userName, string password)
            : base(server, database, userName, password)
        {
        }
    }
}
