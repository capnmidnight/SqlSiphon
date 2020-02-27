namespace SqlSiphon.Postgres
{
    [DatabaseVendorInfo("PostgreSQL", "PSQL", @"C:\Program Files\PostgreSQL\9.3\bin\psql.exe")]
    public class PostgresDataConnectorFactory : IDataConnectorFactory
    {
        public IDataConnector MakeConnector(string connectionString)
        {
            return new PostgresDataAccessLayer(connectionString);
        }

        public IDataConnector MakeConnector(string server, string database, string userName, string password)
        {
            return new PostgresDataAccessLayer(server, database, userName, password);
        }
    }
}
