using System.Data.Odbc;

namespace SqlSiphon.ODBC
{
    public abstract class DataAccessLayer : SqlSiphon<OdbcConnection, OdbcCommand, OdbcParameter, OdbcDataAdapter, OdbcDataReader>
    {
        /// <summary>
        /// creates a new connection to a Odbc database and automatically
        /// opens the connection. 
        /// </summary>
        /// <param name="connectionString">a standard MS SQL Server connection string</param>
        public DataAccessLayer(string connectionString)
            : base(connectionString)
        {
        }

        public DataAccessLayer(OdbcConnection connection)
            : base(connection)
        {
        }
    }
}