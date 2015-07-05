using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon.InformationSchema
{
    /// <summary>
    /// This is an entity object for the SQL standard 
    /// INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE view.
    /// </summary>
    [Mapping.Table(
        Include = false,
        Schema = "information_schema",
        Name = "key_column_usage")]
    public class KeyColumnUsage
    {
        public string table_catalog { get; set; }
        public string table_schema { get; set; }
        public string table_name { get; set; }
        public string column_name { get; set; }
        public string constraint_catalog { get; set; }
        public string constraint_schema { get; set; }
        public string constraint_name { get; set; }
        public int? ordinal_position { get; set; }
        public int? position_in_unique_constraint { get; set; }

        public override string ToString()
        {
            return string.Format("KeyColumnUsage: {0} {1}({2}.{3})", this.ordinal_position, this.constraint_name, this.table_name, this.column_name);
        }
    }
}
