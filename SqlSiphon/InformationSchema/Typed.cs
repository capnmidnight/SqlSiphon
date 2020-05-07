namespace SqlSiphon.InformationSchema
{
    public class Typed
    {
        public int? ordinal_position { get; set; }
        public string data_type { get; set; }
        public string udt_catalog { get; set; }
        public string udt_schema { get; set; }
        public string udt_name { get; set; }
        public string type_udt_catalog { get; set; }
        public string type_udt_schema { get; set; }
        public string type_udt_name { get; set; }
        public bool is_array { get; set; }
        public int? character_maximum_length { get; set; }
        public int? character_octet_length { get; set; }
        public short? numeric_precision { get; set; }
        public short? numeric_precision_radix { get; set; }
        public short? numeric_scale { get; set; }
        public short? datetime_precision { get; set; }
    }
}
