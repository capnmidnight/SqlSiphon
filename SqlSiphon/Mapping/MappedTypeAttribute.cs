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
using System.Reflection;

namespace SqlSiphon.Mapping
{
    [AttributeUsage(
        AttributeTargets.Parameter
        | AttributeTargets.Property
        | AttributeTargets.Method
        | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class MappedTypeAttribute : MappedSchemaObjectAttribute
    {
        /// <summary>
        /// The .NET type to which this database object is going to map
        /// </summary>
        public Type SystemType { get; protected set; }

        /// <summary>
        /// The Database type to which this .NET type is going to map
        /// </summary>
        public string SqlType { get; set; }

        /// <summary>
        /// Returns true if the SystemType represents some kind of collection
        /// of multiple values.
        /// </summary>
        public bool IsCollection
        {
            get
            {
                return this.SystemType
                    .FindInterfaces(new TypeFilter((t, o) =>
                        t == typeof(System.Collections.IEnumerable)), null)
                    .Length > 0;
            }
        }

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
            get { return this.typeSize; }
            set
            {
                this.IsSizeSet = true;
                this.typeSize = value;
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
            get { return this.typePrecision; }
            set
            {
                this.IsPrecisionSet = true;
                this.typePrecision = value;
            }
        }

        /// <summary>
        /// Get or set the default value to be inserted provided to the 
        /// database if no value is provided by the caller. Defaults to
        /// null.
        /// </summary>
        public object DefaultValue { get; set; }

        protected bool optionalNotSet = true;
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
                optionalNotSet = false;
                isOptionalField = value;
            }
        }

        public MappedTypeAttribute() { }

        private void SetSystemType(Type type)
        {
            if (this.SystemType == null)
            {
                this.SystemType = type;
                if (type.IsGenericType)
                {
                    this.SystemType = type.GetGenericArguments()[0];
                    if (type.Name.StartsWith("Nullable") && this.optionalNotSet)
                        this.IsOptional = true;
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
        public override void InferProperties(ParameterInfo parameter)
        {
            base.InferProperties(parameter);
            this.SetSystemType(parameter.ParameterType);
        }

        /// <summary>
        /// A virtual method to analyze an object and figure out the
        /// default settings for it. The attribute can't find the thing
        /// its attached to on its own, so this can't be done in a
        /// constructor, we have to do it for it.
        /// </summary>
        /// <param name="obj">The object to InferProperties</param>
        public virtual void InferProperties(MethodInfo obj)
        {
            base.InferProperties(obj);
            this.SetSystemType(obj.ReturnType);
        }

        /// <summary>
        /// A virtual method to analyze an object and figure out the
        /// default settings for it. The attribute can't find the thing
        /// its attached to on its own, so this can't be done in a
        /// constructor, we have to do it for it.
        /// </summary>
        /// <param name="obj">The object to InferProperties</param>
        public virtual void InferProperties(PropertyInfo obj)
        {
            base.InferProperties(obj);
            this.SetSystemType(obj.PropertyType);
        }

        /// <summary>
        /// Creates the necessary SQL type string to represent this object.
        /// </summary>
        /// <param name="openSize">when SQL types define a size and/or precision, they are often included in parens. Defaults to '('.</param>
        /// <param name="listSeparator">when SQL types define a size and precision, the to parts are often separated by a character. Defaults to ','.</param>
        /// <param name="closeSize">when SQL types define a size and/or precision, they are often included in parens. Defaults to ')'.</param>
        /// <param name="before">defaults to empty string</param>
        /// <param name="after">defaults to empty string</param>
        /// <returns></returns>
        public string ToString(string openSize = "(", string listSeparator = ",", string closeSize = ")", string before = "", string after = "")
        {
            string output = "";
            var format = this.IsSizeSet
                ? this.IsPrecisionSet
                    ? "{0} {1} {2}{3}{4} {5}{6} {7}"
                    : "{0} {1} {2}{3}{6} {7}"
                : "{0} {1} {7}";

            output = string.Format(
                format,
                before,
                this.SqlType,
                openSize,
                this.Size == 0 ? "MAX" : this.Size.ToString(),
                listSeparator,
                this.Precision,
                closeSize,
                after);

            return output.Trim();
        }
    }
}
