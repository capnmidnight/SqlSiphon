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
        Name = "referential_constraints")]
    public class ReferentialConstraint
    {
        public string constraint_schema { get; set; }
        public string constraint_name { get; set; }
        public string unique_constraint_schema { get; set; }
        public string unique_constraint_name { get; set; }

        public override string ToString()
        {
            return $"ReferentialConstraint: {constraint_name} to {unique_constraint_name}";
        }
    }
}
