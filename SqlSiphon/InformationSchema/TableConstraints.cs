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
        Name = "table_constraints")]
    public class TableConstraints
    {
        public string constraint_catalog { get; set; }
        public string constraint_schema { get; set; }
        public string constraint_name { get; set; }
        public string table_catalog { get; set; }
        public string table_schema { get; set; }
        public string table_name { get; set; }
        public string constraint_type { get; set; }
        public string is_deferrable { get; set; }
        public string initially_deferred { get; set; }

        public override string ToString()
        {
            return $"TableConstraint: {table_name}.{constraint_name}:({constraint_type})";
        }
    }
}
