using System;

namespace SqlSiphon
{
    public class TableHasNoColumnsException : Exception
    {
        public TableHasNoColumnsException(Mapping.TableAttribute table)
            : base(string.Format("The table `{0}`.`{1}` defined by type `{2}` has no properties that could be mapped to columns.",
                table.Schema,
                table.Name,
                table.SystemType.FullName))
        {
        }
    }
}
