using SqlSiphon.Mapping;

namespace SqlSiphon.InformationSchema
{
    /// <summary>
    /// This is an entity object for the SQL standard 
    /// INFORMATION_SCHEMA.COLUMNS view.
    /// http://www.contrib.andrew.cmu.edu/~shadow/sql/sql1992.txt
    /// (text-search "CREATE VIEW COLUMNS")
    /// </summary>
    [View(
        Include = false,
        Schema = "information_schema",
        Name = "Columns")]
    public class Columns : Typed
    {
        public string table_catalog { get; set; }
        public string table_schema { get; set; }
        public string table_name { get; set; }
        public string column_name { get; set; }
        public string column_default { get; set; }
        public string is_nullable { get; set; }
        public int? is_identity { get; set; }

        public override string ToString()
        {
            return $"Column: {table_name}.{column_name}:({data_type})";
        }
    }
}
