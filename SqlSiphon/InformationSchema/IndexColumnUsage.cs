using SqlSiphon.Mapping;

namespace SqlSiphon.InformationSchema
{
    /// <summary>
    /// This is an entity object for the SQL standard 
    /// INFORMATION_SCHEMA.COLUMNS view.
    /// http://www.contrib.andrew.cmu.edu/~shadow/sql/sql1992.txt
    /// (text-search "CREATE VIEW COLUMNS")
    /// </summary>
    [Table(Include = false, Schema = "information_schema")]
    public class IndexColumnUsage
    {
        public string table_schema { get; set; }
        public string table_name { get; set; }
        public string index_schema { get; set; }
        public string index_name { get; set; }
        public string column_name { get; set; }
        public bool is_primary_key { get; set; }
        public bool is_unique { get; set; }
        public bool is_unique_constraint { get; set; }

        public override string ToString()
        {
            return $"IndexColumnUsage: {index_schema}::{index_name}({table_schema}::{table_name}.{column_name})";
        }
    }
}
