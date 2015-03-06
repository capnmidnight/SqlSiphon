using SqlSiphon.Mapping;

namespace SqlSiphon.InformationSchema
{
    /// <summary>
    /// This is an entity object for the SQL standard 
    /// INFORMATION_SCHEMA.PARAMETERS view.
    /// </summary>
    [Table(Include = false, Schema = "information_schema")]
    public class Parameters
    {
        public string specific_catalog { get; set; }
        public string specific_schema { get; set; }
        public string specific_name { get; set; }
        public int? ordinal_position { get; set; }
        public string parameter_mode { get; set; }
        public string is_result { get; set; }
        public string as_locator { get; set; }
        public string parameter_name { get; set; }
        public string data_type { get; set; }
        public int? character_maximum_length { get; set; }
        public int? character_octet_length { get; set; }
        public string collation_catalog { get; set; }
        public string collation_schema { get; set; }
        public string collation_name { get; set; }
        public string character_set_catalog { get; set; }
        public string character_set_schema { get; set; }
        public string character_set_name { get; set; }
        public short? numeric_precision { get; set; }
        public short? numeric_precision_radix { get; set; }
        public short? numeric_scale { get; set; }
        public short? datetime_precision { get; set; }
        public string interval_type { get; set; }
        public short? interval_precision { get; set; }
        public string user_defined_type_catalog { get; set; }
        public string user_defined_type_schema { get; set; }
        public string user_defined_type_name { get; set; }
        public string scope_catalog { get; set; }
        public string scope_schema { get; set; }
        public string scope_name { get; set; }
    }
}
