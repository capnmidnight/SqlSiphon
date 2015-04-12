using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Examples.SqlServer.Runner
{
    [Mapping.Table]
    public class MyTable : BoundObject
    {
        public int MyNumber { get { return Get<int>(); } set { Set(value); } }
        public string MyWord { get { return Get<string>(); } set { Set(value); } }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            var factories = new IDataConnectorFactory[]{
                new SqlSiphon.SqlServer.SqlServerDataConnectorFactory(),
                new SqlSiphon.Postgres.PostgresDataConnectorFactory()
            };
            var db = new BasicDAL();
            foreach (var factory in factories)
            {
                var server = "localhost";
                if(factory is SqlSiphon.SqlServer.SqlServerDataConnectorFactory){
                    server += "\\SQLEXPRESS";
                }
                using (db.Connection = factory.MakeConnector(server, "TestDB", "TestDBUser", "testpassword"))
                {
                    Console.WriteLine("Getting data from {0}", db.DatabaseType);
                    Console.WriteLine(string.Join(", ", db.GetAllRoles()));
                }
            }
        }
    }
}
