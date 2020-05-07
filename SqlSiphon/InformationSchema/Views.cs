using SqlSiphon.Mapping;

namespace SqlSiphon.InformationSchema
{
    [Table(Include = false, Schema = "information_schema")]
    public class Views
    {
        public string table_schema { get; set; }
        public string table_name { get; set; }
        public string view_definition { get; set; }
    }
}
