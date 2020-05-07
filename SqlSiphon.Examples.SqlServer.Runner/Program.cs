using System;

namespace SqlSiphon.Examples.Runner
{
    [Mapping.Table]
    public class MyTable : BoundObject
    {
        public int MyNumber { get { return Get<int>(); } set { Set(value); } }
        public string MyWord { get { return Get<string>(); } set { Set(value); } }
    }

    public static class Program
    {
        private const string Server = "localhost";
        private const string Database = "TestDB";
        private const string UserName = "TestDBUser";
        private const string Password = "TestDBPassword";

        public static void Main()
        {
            var factory = new Postgres.PostgresDataConnectorFactory();
            var dbTypeName = DataConnector.GetDatabaseVendorName(factory.GetType());

            using var connection = factory.MakeConnector(Server, Database, UserName, Password);
            using var db = new BasicDAL(connection);
            Console.WriteLine("Getting data from {0}", dbTypeName);
            Console.WriteLine(string.Join(", ", db.GetAllRoles()));
        }
    }
}
