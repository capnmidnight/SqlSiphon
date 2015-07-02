namespace SqlSiphon.OleDB
{
    public class OleDBDataConnectorFactory : IDataConnectorFactory
    {
        public IDataConnector MakeConnector(string fileName)
        {
            return new OleDBDataAccessLayer(fileName);
        }

        public IDataConnector MakeConnector(string server, string database, string userName, string password)
        {
            return new OleDBDataAccessLayer(server, database, userName, password);
        }
    }
}
