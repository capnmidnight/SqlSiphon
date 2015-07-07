using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon
{
    public class TableHasNoColumnsException : Exception
    {
        public TableHasNoColumnsException(Mapping.TableAttribute table)
            : base(string.Format("The table `{0}`.`{1}` defined by type `{2}` has no properties that could be mapped to columns.", 
                table.Schema, 
                table.Name,
                ((Type)table.SourceObject).FullName))
        {
        }
    }
}
