using SqlSiphon.Mapping;

namespace SqlSiphon.InformationSchema
{
    /// <summary>
    /// This is an entity object for the SQL standard 
    /// INFORMATION_SCHEMA.PARAMETERS view.
    /// </summary>
    [View(
        Include = false,
        Schema = "information_schema",
        Name = "routines")]
    public class Routines : Typed
    {
        public string specific_schema { get; set; }
        public string specific_name { get; set; }
        public string routine_schema { get; set; }
        public string routine_name { get; set; }
        public string routine_definition { get; set; }

        public bool IsUDTT => "USER-DEFINED".Equals(data_type, System.StringComparison.InvariantCultureIgnoreCase);

        public override string ToString()
        {
            return "Routine: " + routine_name;
        }
    }
}
