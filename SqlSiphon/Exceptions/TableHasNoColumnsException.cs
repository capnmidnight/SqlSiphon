using System;

namespace SqlSiphon
{
    public class TableHasNoColumnsException : Exception
    {
        public TableHasNoColumnsException(Mapping.TableAttribute table)
            : base($"The table `{table.Schema}`.`{table.Name}` defined by type `{table.SystemType.FullName}` has no properties that could be mapped to columns.")
        {
        }
    }
}
