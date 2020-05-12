using System;
using Npgsql;

namespace SqlSiphon.Postgres
{
    [DatabaseVendorInfo("PostgreSQL", "PSQL", @"*\*\PostgreSQL\*\bin\psql.exe")]
    public class PostgresDataConnectorFactory : IDataConnectorFactory
    {
        public IDataConnector MakeConnector(string connectionString)
        {
            return new PostgresDataAccessLayer(connectionString);
        }

        public IDataConnector MakeConnector(string server, string database, string userName, string password)
        {
            if (server is null)
            {
                throw new ArgumentNullException(nameof(server));
            }

            var builder = new NpgsqlConnectionStringBuilder();
            if (database is object)
            {
                builder.Database = database;
            }

            if (!string.IsNullOrWhiteSpace(userName)
                && !string.IsNullOrWhiteSpace(password))
            {
                builder.Add("Username", userName.Trim());
                builder.Add("Password", password.Trim());
            }

            var i = server.IndexOf(":", StringComparison.InvariantCultureIgnoreCase);
            if (i > -1)
            {
                builder.Host = server.Substring(0, i);
                if (int.TryParse(server.Substring(i + 1), out var port))
                {
                    builder.Port = port;
                }
            }
            else
            {
                builder.Host = server;
            }

            return MakeConnector(builder.ConnectionString);
        }
    }
}
