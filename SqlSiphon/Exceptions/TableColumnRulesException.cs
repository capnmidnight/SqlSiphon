using System;
using System.Linq;

namespace SqlSiphon
{
    public class TableColumnRulesException : Exception
    {
        public TableColumnRulesException(string template, Mapping.TableAttribute table, Mapping.ColumnAttribute[] columns)
            : base(string.Format(
                template,
                table.Schema,
                table.Name,
                table.SystemType.FullName,
                Environment.NewLine,
                string.Join(",", columns.Select(c => c.Name))))
        {
        }
    }
}
