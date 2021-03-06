using SqlSiphon.Mapping;

namespace SqlSiphon.InformationSchema
{
    /// <summary>
    /// This is an entity object for the SQL standard 
    /// INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE view.
    /// </summary>
    [View(
        Include = false,
        Schema = "information_schema",
        Name = "key_column_usage")]
    public class KeyColumnUsage
    {
        public string table_schema { get; set; }
        public string table_name { get; set; }
        public string column_name { get; set; }
        public string constraint_schema { get; set; }
        public string constraint_name { get; set; }
        public int? ordinal_position { get; set; }
        
        public override string ToString()
        {
            return $"KeyColumnUsage: {ordinal_position} {constraint_name}({table_name}.{column_name})";
        }
    }
}
