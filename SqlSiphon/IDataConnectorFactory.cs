namespace SqlSiphon
{
    public interface IDataConnectorFactory
    {
        IDataConnector MakeConnector(string connectionString);
        IDataConnector MakeConnector(string server, string database, string userName, string password);
    }
}
