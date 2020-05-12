using System;
using System.Linq;
using System.Reflection;

namespace SqlSiphon.Mapping
{
    /// <summary>
    /// A base class for types that are mapped to the database.
    /// This attribute can be applied to classes, class properties,
    /// methods, method parameters, and enumerations. 
    /// 
    /// Only one attribute of a given type may be applied to
    /// any type of thing.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class
        | AttributeTargets.Method
        | AttributeTargets.Parameter
        | AttributeTargets.Property,
        Inherited = false,
        AllowMultiple = false)]
    public abstract class DatabaseObjectAttribute : Attribute
    {
        protected ICustomAttributeProvider SourceObject { get; set; }

        /// <summary>
        /// Get or set a schema name for objects in the database. Defaults to
        /// null, which causes the data access system to use whatever is
        /// defined as the default value for the database vendor.
        /// </summary>
        public virtual string Schema { get; set; }

        /// <summary>
        /// A property to override the default interpretation
        /// of the type's name. Usually, objects in the database
        /// are named after the type of thing that we are
        /// mapping, but in some cases we will want to override
        /// this behavior.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// A property to turn on or off the inclusion of the
        /// type as an object in the database. Defaults to
        /// "true", but can be overridden to keep from trying
        /// to process the thing. This can be useful for
        /// mapped classes that have extra attributes that we
        /// don't need in the database.
        /// </summary>
        private bool _include;
        public bool IsIncludeSet { get; private set; }
        public bool Include
        {
            get { return _include; }
            set
            {
                IsIncludeSet = true;
                _include = value;
            }
        }

        /// <summary>
        /// As this class is abstract, it can't be instantiated.
        /// </summary>
        protected DatabaseObjectAttribute()
        {
            _include = true;
        }

        /// <summary>
        /// Retrieves all attribute of a certain type from an object.
        /// 
        /// Make sure to call InferProperties after calling this method.
        /// </summary>
        /// <typeparam name="T">The type of attribute to find</typeparam>
        /// <param name="obj">The object on which to find the attribute</param>
        /// <returns>The attribute instance, or null if no such
        /// attribute exists</returns>
        private static System.Collections.Generic.IEnumerable<T> GetAttributes<T>(ICustomAttributeProvider obj)
            where T : Attribute
        {
            return obj
                .GetCustomAttributes(typeof(T), false)
                .Cast<T>();
        }

        /// <summary>
        /// Retrieve an attribute of a certain type from an object.
        /// </summary>
        /// <typeparam name="T">The type of attribute to find</typeparam>
        /// <param name="obj">The object on which to find the attribute</param>
        /// <returns>The attribute instance, or null if no such
        /// attribute exists</returns>                
        public static RoutineAttribute GetRoutine(MethodInfo obj)
        {
            return GetAttributes<RoutineAttribute>(obj)
                .Select(attr =>
                {
                    attr.InferProperties(obj);
                    return attr;
                }).FirstOrDefault();
        }

        public static ColumnAttribute GetColumn(PropertyInfo obj)
        {
            return GetAttributes<ColumnAttribute>(obj)
                .Select(attr =>
                {
                    attr.InferProperties(obj);
                    return attr;
                }).FirstOrDefault();
        }

        public static ParameterAttribute GetParameter(ParameterInfo obj)
        {
            return GetAttributes<ParameterAttribute>(obj)
                .Select(attr =>
                {
                    attr.InferProperties(obj);
                    return attr;
                }).FirstOrDefault();
        }

        public static TableAttribute GetTable(Type obj)
        {
            return GetAttributes<TableAttribute>(obj)
                .Select(attr =>
                {
                    attr.InferProperties(obj);
                    return attr;
                }).FirstOrDefault();
        }

        public static ViewAttribute GetView(Type obj)
        {
            return GetAttributes<ViewAttribute>(obj)
                .Select(attr =>
                {
                    attr.InferProperties(obj);
                    return attr;
                }).FirstOrDefault();
        }

        public static T GetAttribute<T>(ICustomAttributeProvider obj)
            where T : Attribute
        {
            return GetAttributes<T>(obj).FirstOrDefault();
        }

        public System.Collections.Generic.IEnumerable<T> GetOtherAttributes<T>()
            where T : Attribute
        {
            if (SourceObject != null)
            {
                return GetAttributes<T>(SourceObject);
            }
            return Array.Empty<T>();
        }

        public T GetOtherAttribute<T>()
            where T : Attribute
        {
            return GetOtherAttributes<T>().FirstOrDefault();
        }

        /// <summary>
        /// Usually, objects in the database are named after the 
        /// type of thing that we are mapping, but in some cases 
        /// we will want to override this behavior. 
        /// </summary>
        /// <param name="name"></param>
        private void SetName(string name)
        {
            if (Name == null)
            {
                Name = name;
            }
        }

        /// <summary>
        /// The .NET type to which this database object is going to map
        /// </summary>
        public virtual Type SystemType { get; protected set; }

        /// <summary>
        /// The Database type to which this .NET type is going to map
        /// </summary>
        public string SqlType { get; set; }

        /// <summary>
        /// Returns true if a size was specified for the database type. This
        /// doesn't mean anything for .NET types.
        /// </summary>
        public bool IsStringLengthSet { get; private set; }

        private int typeStringLength;

        /// <summary>
        /// Get or set the size of the database type. If the size is not set,
        /// (i.e. IsSizeSet returns false) then no size or precision will be
        /// included in the type specification. Use 0 to mean "MAX".
        /// </summary>
        public int StringLength
        {
            get { return typeStringLength; }
            set
            {
                IsStringLengthSet = true;
                typeStringLength = value;
            }
        }

        public int? NullableStringLength
        {
            get
            {
                if (IsStringLengthSet)
                {
                    return typeStringLength;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Returns true if the precision was specified for the database type.
        /// This doesn't mean anything for .NET types.
        /// </summary>
        public bool IsNumericPrecisionSet { get; private set; }

        private int typeNumericPrecision;

        /// <summary>
        /// Get or set the precision of the database type. If the precision
        /// is not set (i.e. IsPrecisionSet returns false), then no precision
        /// will be included in the type specification.
        /// </summary>
        public int NumericPrecision
        {
            get { return typeNumericPrecision; }
            set
            {
                IsNumericPrecisionSet = true;
                typeNumericPrecision = value;
            }
        }

        public int? NullableNumericPrecision
        {
            get
            {
                if (IsNumericPrecisionSet)
                {
                    return typeNumericPrecision;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool IsNumericScaleSet { get; private set; }
        private int typeNumericScale;
        public int NumericScale { get
            {
                return typeNumericScale;
            }
            set
            {
                typeNumericScale = value;
                IsNumericScaleSet = true;
            }
        }


        public int? NullableNumericScale
        {
            get
            {
                if (IsNumericScaleSet)
                {
                    return typeNumericScale;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Get or set the default value to be inserted provided to the 
        /// database if no value is provided by the caller. Defaults to
        /// null.
        /// </summary>
        public string DefaultValue { get; set; }

        public bool IsOptionalSet { get; set; }
        private bool isOptionalField = false;

        /// <summary>
        /// Get or set whether or not the caller is required to provide
        /// a value to the database. If set to true, includes the "NULLABLE"
        /// annotation on database types. Defaults to false.
        /// </summary>
        public bool IsOptional
        {
            get { return isOptionalField; }
            set
            {
                IsOptionalSet = true;
                isOptionalField = value;
            }
        }

        public void SetSystemType(Type type)
        {
            if (SystemType == null)
            {
                SystemType = DataConnector.CoalesceNullableValueType(type);
                if (DataConnector.IsNullableValueType(type) && !IsOptionalSet)
                {
                    IsOptional = true;
                }
            }
        }

        /// <summary>
        /// A virtual method to analyze an object and figure out the
        /// default settings for it. The attribute can't find the thing
        /// its attached to on its own, so this can't be done in a
        /// constructor, we have to do it for it.
        /// </summary>
        /// <param name="obj">The object to InferProperties</param>
        protected virtual void InferProperties(MethodInfo obj)
        {
            SourceObject = obj;
            SetName(obj.Name);
            SetSystemType(obj.ReturnType);
        }

        /// <summary>
        /// A virtual method to analyze an object and figure out the
        /// default settings for it. The attribute can't find the thing
        /// its attached to on its own, so this can't be done in a
        /// constructor, we have to do it for it.
        /// </summary>
        /// <param name="obj">The object to InferProperties</param>
        protected virtual void InferProperties(PropertyInfo obj)
        {
            SourceObject = obj;
            SetName(obj.Name);
            SetSystemType(obj.PropertyType);
        }

        /// <summary>
        /// A virtual method to analyze an object and figure out the
        /// default settings for it. The attribute can't find the thing
        /// its attached to on its own, so this can't be done in a
        /// constructor, we have to do it for it.
        /// </summary>
        /// <param name="obj">The object to InferProperties</param>
        protected virtual void InferProperties(ParameterInfo obj)
        {
            SourceObject = obj;
            SetName(obj.Name);
            SetSystemType(obj.ParameterType);
        }

        /// <summary>
        /// A virtual method to analyze an object and figure out the
        /// default settings for it. The attribute can't find the thing
        /// its attached to on its own, so this can't be done in a
        /// constructor, we have to do it for it.
        /// </summary>
        /// <param name="obj">The object to InferProperties</param>
        protected virtual void InferProperties(Type obj)
        {
            SourceObject = obj;
            SetName(obj.Name);
            SetSystemType(obj);
        }

        protected void InferTypeInfo(InformationSchema.Typed obj, string sqlType, ISqlSiphon dal)
        {
            SqlType = sqlType;
            if (SqlType is object)
            {
                if (SqlType[0] == '_')
                {
                    SqlType = SqlType.Substring(1);
                }

                SystemType = dal.GetSystemType(SqlType);
                if (SystemType != null)
                {
                    if (SystemType.IsPrimitive)
                    {
                        StringLength = System.Runtime.InteropServices.Marshal.SizeOf(SystemType);
                    }

                    if (obj.numeric_precision.HasValue
                        && obj.numeric_precision.Value != dal.GetDefaultTypePrecision(SqlType, obj.numeric_precision.Value))
                    {
                        NumericPrecision = obj.numeric_precision.Value;
                    }

                    if (obj.character_maximum_length.HasValue && obj.character_maximum_length.Value > 0)
                    {
                        StringLength = obj.character_maximum_length.Value;
                    }

                    if (obj.numeric_scale.HasValue && obj.numeric_scale.Value > 0)
                    {
                        NumericPrecision = obj.numeric_scale.Value;
                    }
                }
            }
        }

        public override string ToString()
        {
            return $"[{Schema}].[{Name}]";
        }
    }
}
