using System;
using System.Data;
using System.Reflection;

namespace SqlSiphon.Mapping
{
    /// <summary>
    /// An attribute to tag to parameters of a method to provide optional information
    /// about how the method parameter maps to a stored procedure parameter.
    /// 
    /// Only one attribute of a given type may be applied to
    /// any type of thing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class ParameterAttribute : DatabaseObjectAttribute
    {
        private bool directionNotSet = true;
        private ParameterDirection paramDirection = ParameterDirection.Input;

        /// <summary>
        /// Gets or sets the stored procedure parameter's "direction", i.e.
        /// whether or not it's an input parameter, output parameter, or both.
        /// </summary>
        public ParameterDirection Direction
        {
            get
            {
                return paramDirection;
            }
            set
            {
                directionNotSet = false;
                paramDirection = value;
            }
        }

        public bool IsUDTT { get; private set; }

        public bool IsArray { get; private set; }

        public ParameterAttribute() { }

        public ParameterAttribute(ParameterInfo param)
        {
            InferProperties(param);
        }

        public ParameterAttribute(InformationSchema.Parameters parameter, ISqlSiphon dal)
        {
            Include = true;

            if (!string.IsNullOrEmpty(parameter.parameter_name))
            {
                Name = parameter.parameter_name.Substring(1);
            }

            InferTypeInfo(parameter, parameter.TypeName, dal);

            switch (parameter.parameter_mode)
            {
                case "OUT":
                Direction = ParameterDirection.Output;
                break;
                case "INOUT":
                Direction = ParameterDirection.InputOutput;
                break;
            }

            Schema = parameter.TypeSchema;
            IsUDTT = parameter.IsUDTT;
            IsArray = parameter.IsArray;
        }

        /// <summary>
        /// A virtual method to analyze an object and figure out the
        /// default settings for it. The attribute can't find the thing
        /// its attached to on its own, so this can't be done in a
        /// constructor, we have to do it for it.
        /// 
        /// This method is not called from the DatabaseObjectAttribute.GetAttribute(s)
        /// methods because those methods aren't overloaded for different types
        /// of ICustomAttributeProvider types, but InferProperties is.
        /// </summary>
        /// <param name="obj">The object to InferProperties</param>
        protected override void InferProperties(ParameterInfo parameter)
        {
            base.InferProperties(parameter);

            // If the parameter direction was not set explicitly,
            // then infer it from the method parameter's direction.
            if (directionNotSet)
            {
                Direction = ParameterDirection.Input;
                if (parameter.IsIn && parameter.IsOut)
                {
                    Direction = ParameterDirection.InputOutput;
                }
                else if (parameter.IsOut)
                {
                    Direction = ParameterDirection.Output;
                }
                else if (parameter.IsRetval)
                {
                    Direction = ParameterDirection.ReturnValue;
                }
            }

            // Infer optionalness of the stored procedure's parameter
            // from whether or not the method's parameter is optional, 
            // but only if the IsOptional property of the attribute 
            // was not set explicitly.
            if (!IsOptionalSet)
            {
                IsOptional = parameter.IsOptional;
            }

            // Infer the default value for the stored procedure's 
            // parameter from the method parameter's default value,
            // but only if the DefaultValue property of the attribute
            // was not set to a specific value.
            if (DefaultValue == null
                && parameter.DefaultValue != null
                && parameter.DefaultValue != DBNull.Value)
            {
                DefaultValue = parameter.DefaultValue.ToString();
            }
        }
    }
}
