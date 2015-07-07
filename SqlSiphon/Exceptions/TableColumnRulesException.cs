using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon
{
    public class TableColumnRulesException : Exception
    {
        public TableColumnRulesException(string template, Mapping.TableAttribute table, Mapping.ColumnAttribute[] columns)
            : base(string.Format(
                template, 
                table.Schema, 
                table.Name,
                ((Type)table.SourceObject).FullName,
                Environment.NewLine,
                string.Join(",", columns.Select(c=>c.Name))))
        {
        }
    }
}
