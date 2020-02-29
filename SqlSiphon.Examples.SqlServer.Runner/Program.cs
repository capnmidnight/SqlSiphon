using System;

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
        public static void Main()
        {
            var factories = new IDataConnectorFactory[]{
                new SqlSiphon.SqlServer.SqlServerDataConnectorFactory(),
                new Postgres.PostgresDataConnectorFactory()
            };
            using var db = new BasicDAL();
            foreach (var factory in factories)
            {
                var dbTypeName = DataConnector.GetDatabaseVendorName(factory.GetType());
                var server = "localhost";
                if (factory is SqlSiphon.SqlServer.SqlServerDataConnectorFactory)
                {
                    server += "\\SQLEXPRESS";
                }
                using (db.Connection = factory.MakeConnector(server, "TestDB", "TestDBUser", "TestDBPassword"))
                {
                    Console.WriteLine("Getting data from {0}", dbTypeName);
                    Console.WriteLine(string.Join(", ", db.GetAllRoles()));
                }
            }
        }
    }
}
