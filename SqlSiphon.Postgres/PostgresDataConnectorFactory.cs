namespace SqlSiphon.Postgres
{
    public class PostgresDataConnectorFactory : IDataConnectorFactory
    {
        public IDataConnector MakeConnector(string server, string database, string userName, string password)
        {
            return new PostgresDataAccessLayer(server, database, userName, password);
        }
    }
}
