using SqlSiphon.Mapping;

namespace SqlSiphon.InformationSchema
{
    /// <summary>
    /// This is an entity object for the SQL standard 
    /// INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE view.
    /// </summary>
    [Table(
        Include = false,
        Schema = "information_schema",
        Name = "constraint_column_usage")]
    public class ConstraintColumnUsage
    {
        public string table_catalog { get; set; }
        public string table_schema { get; set; }
        public string table_name { get; set; }
        public string column_name { get; set; }
        public string constraint_catalog { get; set; }
        public string constraint_schema { get; set; }
        public string constraint_name { get; set; }

        public override string ToString()
        {
            return $"ConstraintColumnUsage: {constraint_name}({table_name}.{column_name})";
        }
    }
}
