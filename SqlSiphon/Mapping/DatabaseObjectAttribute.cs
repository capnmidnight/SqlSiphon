/*
https://www.github.com/capnmidnight/SqlSiphon
Copyright (c) 2009 - 2014 Sean T. McBeth
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this 
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or 
  other materials provided with the distribution.

* Neither the name of McBeth Software Systems nor the names of its contributors
  may be used to endorse or promote products derived from this software without 
  specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE 
OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED 
OF THE POSSIBILITY OF SUCH DAMAGE.
*/

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
        public DatabaseObjectAttribute()
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
        public static RoutineAttribute GetAttribute(MethodInfo obj)
        {
            return GetAttributes<RoutineAttribute>(obj)
                .Select(attr =>
                {
                    attr.InferProperties(obj);
                    return attr;
                }).FirstOrDefault();
        }

        public static ColumnAttribute GetAttribute(PropertyInfo obj)
        {
            return GetAttributes<ColumnAttribute>(obj)
                .Select(attr =>
                {
                    attr.InferProperties(obj);
                    return attr;
                }).FirstOrDefault();
        }

        public static ParameterAttribute GetAttribute(ParameterInfo obj)
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
                return DatabaseObjectAttribute.GetAttributes<T>(SourceObject);
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
