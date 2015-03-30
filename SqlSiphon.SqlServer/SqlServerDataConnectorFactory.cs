namespace SqlSiphon.SqlServer
{
    public class SqlServerDataConnectorFactory : IDataConnectorFactory
    {
        public IDataConnector MakeConnector(string server, string database, string userName, string password)
        {
            return new SqlServerDataAccessLayer(server, database, userName, password);
        }
    }
}
