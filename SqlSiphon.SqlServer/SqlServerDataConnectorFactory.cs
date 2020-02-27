namespace SqlSiphon.SqlServer
{
    [DatabaseVendorInfo("Microsoft SQL Server", "SQLCMD", @"C:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd.exe")]
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
