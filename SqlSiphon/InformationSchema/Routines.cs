using System;
using SqlSiphon.Mapping;

namespace SqlSiphon.InformationSchema
{
    /// <summary>
    /// This is an entity object for the SQL standard 
    /// INFORMATION_SCHEMA.PARAMETERS view.
    /// </summary>
    [Table(Include = false, Schema = "information_schema")]
    public class Routines
    {
        public string specific_catalog { get; set; }
        public string specific_schema { get; set; }
        public string specific_name { get; set; }
        public string routine_catalog { get; set; }
        public string routine_schema { get; set; }
        public string routine_name { get; set; }
        public string routine_definition { get; set; }
        /*
        public string routine_body { get; set; }
        public string routine_type { get; set; }
        public string module_catalog { get; set; }
        public string module_schema { get; set; }
        public string module_name { get; set; }
        public string udt_catalog { get; set; }
        public string udt_schema { get; set; }
        public string udt_name { get; set; }

        public string data_type { get; set; }
        public int? character_maximum_length { get; set; }
        public int? character_octet_length { get; set; }
        public string collation_catalog { get; set; }
        public string collation_schema { get; set; }
        public string collation_name { get; set; }
        public string character_set_catalog { get; set; }
        public string character_set_schema { get; set; }
        public string character_set_name { get; set; }
        public short numeric_precision { get; set; }
        public short numeric_precision_radix { get; set; }
        public short numeric_scale { get; set; }
        public short datetime_precision { get; set; }
        public string interval_type { get; set; }
        public short interval_precision { get; set; }
        public string type_udt_catalog { get; set; }
        public string type_udt_schema { get; set; }
        public string type_udt_name { get; set; }
        public string scope_catalog { get; set; }
        public string scope_schema { get; set; }
        public string scope_name { get; set; }

        public long maximum_cardinality { get; set; }
        public string dtd_identifier { get; set; }

        public string external_name { get; set; }
        public string external_language { get; set; }
        public string parameter_style { get; set; }
        public string is_deterministic { get; set; }
        public string sql_data_access { get; set; }
        public string is_null_call { get; set; }
        public string sql_path { get; set; }
        public string schema_level_routine { get; set; }
        public short? max_dynamic_result_sets { get; set; }
        public string is_user_defined_cast { get; set; }
        public string is_implicitly_invocable { get; set; }
        public DateTime created { get; set; }
        public DateTime last_altered { get; set; }
*/
    }
}
