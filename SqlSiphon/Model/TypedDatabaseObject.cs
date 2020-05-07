using System;
using System.Reflection;

namespace SqlSiphon.Model
{
    /// <summary>
    /// A base class for types that are mapped to the database.
    /// </summary>
    public abstract class TypedDatabaseObject : DatabaseObject
    {
        /// <summary>
        /// As this class is abstract, it can't be instantiated.
        /// </summary>
        public TypedDatabaseObject()
        {
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
        public bool IsSizeSet { get; private set; }

        private int typeSize;

        /// <summary>
        /// Get or set the size of the database type. If the size is not set,
        /// (i.e. IsSizeSet returns false) then no size or precision will be
        /// included in the type specification. Use 0 to mean "MAX".
        /// </summary>
        public int Size
        {
            get { return typeSize; }
            set
            {
                IsSizeSet = true;
                typeSize = value;
            }
        }

        /// <summary>
        /// Returns true if the precision was specified for the database type.
        /// This doesn't mean anything for .NET types.
        /// </summary>
        public bool IsPrecisionSet { get; private set; }

        private int typePrecision;

        /// <summary>
        /// Get or set the precision of the database type. If the precision
        /// is not set (i.e. IsPrecisionSet returns false), then no precision
        /// will be included in the type specification.
        /// </summary>
        public int Precision
        {
            get { return typePrecision; }
            set
            {
                IsPrecisionSet = true;
                typePrecision = value;
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
                        Size = System.Runtime.InteropServices.Marshal.SizeOf(SystemType);
                    }

                    if (obj.numeric_precision.HasValue
                        && obj.numeric_precision.Value != dal.GetDefaultTypePrecision(SqlType, obj.numeric_precision.Value))
                    {
                        Precision = obj.numeric_precision.Value;
                    }

                    if (obj.character_maximum_length.HasValue && obj.character_maximum_length.Value > 0)
                    {
                        Size = obj.character_maximum_length.Value;
                    }

                    if (obj.numeric_scale.HasValue && obj.numeric_scale.Value > 0)
                    {
                        Precision = obj.numeric_scale.Value;
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
