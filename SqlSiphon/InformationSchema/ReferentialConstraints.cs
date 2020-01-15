namespace SqlSiphon.InformationSchema
{
    /// <summary>
    /// This is an entity object for the SQL standard 
    /// INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE view.
    /// </summary>
    [Mapping.Table(
        Include = false,
        Schema = "information_schema",
        Name = "referential_constraints")]
    public class ReferentialConstraints
    {
        public string constraint_schema { get; set; }
        public string constraint_name { get; set; }
        public string unique_constraint_schema { get; set; }
        public string unique_constraint_name { get; set; }
        /*
        public string constraint_catalog { get; set; }
        public string unique_constraint_catalog { get; set; }        
        public string match_option { get; set; }
        public string update_rule { get; set; }
        public string delete_rule { get; set; }
        */
        public override string ToString()
        {
            return string.Format("ReferentialConstraint: {0} to {2}", constraint_name, unique_constraint_name);
        }
    }
}
