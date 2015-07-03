namespace SqlSiphon.SqlServer
{
    [DatabaseVendorName("Microsoft SQL Server")]
    public class SqlServerDataConnectorFactory : IDataConnectorFactory
    {
        public IDataConnector MakeConnector(string connectionString)
        {
            return new SqlServerDataAccessLayer(connectionString);
        }
        public IDataConnector MakeConnector(string server, string database, string userName, string password)
        {
            return new SqlServerDataAccessLayer(server, database, userName, password);
        }
    }
}
