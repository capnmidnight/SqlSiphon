using System;
using System.Linq;

using SqlSiphon.Mapping;

namespace SqlSiphon
{
    public class TableColumnRulesException : Exception
    {
        public TableColumnRulesException(TableAttribute table, ColumnAttribute[] columns, string message)
            : base(MakeMessage(table, columns, message))
        {
        }

        private static string MakeMessage(TableAttribute table, ColumnAttribute[] columns, string message)
        {
            if (table is null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            if (columns is null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var columnsDef = string.Join(",", columns.Select(c => c.Name));
            return $"The table `{table.Schema}`.`{table.Name}` defined by type `{table.SystemType.FullName}` {message}.{Environment.NewLine}{columnsDef}";
        }
    }
}
