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
            var factory = new Postgres.PostgresDataConnectorFactory();
            using var db = new BasicDAL();
            var dbTypeName = DataConnector.GetDatabaseVendorName(factory.GetType());
            var server = "localhost";
            using (db.Connection = factory.MakeConnector(server, "TestDB", "TestDBUser", "TestDBPassword"))
            {
                Console.WriteLine("Getting data from {0}", dbTypeName);
                Console.WriteLine(string.Join(", ", db.GetAllRoles()));
            }
        }
    }
}
