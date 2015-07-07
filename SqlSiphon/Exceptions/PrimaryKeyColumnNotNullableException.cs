using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon
{
    public class PrimaryKeyColumnNotNullableException : TableColumnRulesException
    {
        public PrimaryKeyColumnNotNullableException(Mapping.TableAttribute table, Mapping.ColumnAttribute[] columns)
            : base(
                "The table `{0}`.`{1}` defined by type `{2}` has one or more nullable columns in the primary key.{3}{4}", 
                table,
                columns)
        {
        }
    }
}
