using System;
using System.Data;
using System.Reflection;
using SqlSiphon.Mapping;

namespace SqlSiphon.SqlServer
{
    public class ParameterMapper : TypeMapper
    {
        private string direction;

        public ParameterMapper(MappedParameterAttribute attr, ParameterInfo param)
            : base(attr, param)
        {
            if (Name == null)
            {
                if (param == null)
                    throw new ArgumentException("Could not determine a name for a parameter");
                Name = param.Name;
            }

            if (DefaultValue == null 
                && param != null
                && param.DefaultValue != null
                && param.IsOptional)
            {
                DefaultValue = param.DefaultValue;
            }

            direction = (attr.Direction == ParameterDirection.InputOutput
                || attr.Direction == ParameterDirection.Output) ? " OUTPUT" : "";
        }

        public override string ToString()
        {
            return string.Format("@{0} {1}", base.ToString(), direction);
        }
    }
}
