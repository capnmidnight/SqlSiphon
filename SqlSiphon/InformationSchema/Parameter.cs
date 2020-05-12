using System;
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
        Name = "parameters")]
    public class Parameter : Typed
    {
        public string specific_schema { get; set; }
        public string specific_name { get; set; }
        public string parameter_mode { get; set; }
        public string parameter_name { get; set; }
        public string user_defined_type_schema { get; set; }
        public string user_defined_type_name { get; set; }
        
        public bool IsUserDefinedType => user_defined_type_name is object || udt_name is object;
        private bool IsSqlServerUserDefinedType => IsUserDefinedType && udt_name is null;
        public bool IsArray => "ARRAY".Equals(data_type, StringComparison.InvariantCultureIgnoreCase);

        public string TypeSchema =>
            IsUserDefinedType
                ? IsSqlServerUserDefinedType
                    ? user_defined_type_schema
                    : udt_schema
                : specific_schema;

        public string TypeName =>
            IsUserDefinedType
                ? IsSqlServerUserDefinedType
                    ? user_defined_type_name
                    : udt_name
                : specific_name;

        public override string ToString()
        {
            return $"Parameter: {parameter_name}:({data_type})";
        }
    }
}
