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
    /// <summary>
    /// An attribute to tag properties in a class, for optional
    /// information about how the properties maps to a column 
    /// in a table.
    /// 
    /// Only one attribute of a given type may be applied to
    /// any type of thing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class MappedPropertyAttribute : MappedTypeAttribute
    {
        private PropertyInfo originalProperty;
        /// <summary>
        /// Gets or sets whether or not this column holds an
        /// auto-incrementing integer value. Defaults to false.
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        /// Get or set a value indicating that the column
        /// is used as part of the table's primary key.
        /// Defaults to false.
        /// </summary>
        public bool IncludeInPrimaryKey { get; set; }

        /// <summary>
        /// When a property references another mapped class,
        /// it will be used to specify a foreign key relation-
        /// ship. The mapped table for this class will have
        /// to have columns added to it that match the
        /// primary key of the foreign table. By default,
        /// those columns will be named the same name as they
        /// are named in the foreign table. This will cause
        /// a conflict in the case of one table having foreign-
        /// key references to any set of tables whose primary
        /// keys are named the same. By setting the PrefixColumnNames
        /// property to true, the columns added to this table
        /// will have the name of the property this attribute
        /// is tagged to used as a prefix on each column's name.
        /// Defaults to true.
        /// </summary>
        public bool PrefixColumnNames { get; set; }

        /// <summary>
        /// If the property this attribute is tagged to specifies
        /// a foreign key relationship, then setting the Cascade
        /// property will specify CASCADE DELETE and CASCADE UPDATE
        /// on the foreign key constraint. Defaults to true.
        /// </summary>
        public bool Cascade { get; set; }

        /// <summary>
        /// Specifies the property maps to a column in a table that
        /// is not an auto-incrementing integer field and is not 
        /// included in the primary key. If the column is part of a
        /// foreign key relationship, it cascades deletes and updates
        /// and is named based both on the property name and the 
        /// foreign table's primary key column names. These default
        /// settings can be overridden.
        /// </summary>
        public MappedPropertyAttribute()
        {
            this.IsIdentity = false;
            this.IncludeInPrimaryKey = false;
            this.PrefixColumnNames = true;
            this.Cascade = true;
        }

        internal MappedPropertyAttribute(InformationSchema.Columns column)
        {
            this.DefaultValue = column.column_default;
            this.IsOptional = column.is_nullable.ToLower() == "yes";
            this.Name = column.column_name;
            this.SqlType = column.data_type;

            if (column.data_type == "nvarchar"
                || column.data_type == "varchar")
            {
                if (column.character_maximum_length != null
                    && column.character_maximum_length != -1)
                    this.Size = column.character_maximum_length.Value;
            }
            else
            {
                if (column.numeric_precision != null
                    && !(column.data_type == "int"
                        && column.numeric_precision == 10)
                    && !(column.data_type == "real"
                        && column.numeric_precision == 24)
                    && column.numeric_precision != 0)
                    this.Precision = column.numeric_precision.Value;
                if (column.numeric_scale != null
                        && column.numeric_scale != 0)
                    this.Size = column.numeric_scale.Value;
            }
        }

        public override void InferProperties(System.Reflection.PropertyInfo obj)
        {
            base.InferProperties(obj);
            this.originalProperty = obj;
        }

        public void SetValue(object obj, object value)
        {
            if (value == DBNull.Value)
                value = null;
            try
            {
                var targetType = this.originalProperty.PropertyType;
                if (targetType.IsGenericType
                    && targetType.Name.StartsWith("Nullable"))
                {
                    targetType = targetType.GetGenericArguments()[0];
                    if (value != null)
                        value = Convert.ChangeType(value, targetType);
                }
                else if (targetType.IsEnum
                    && value is string)
                {
                    value = Enum.Parse(targetType, (string)value);
                }

                this.originalProperty.SetValue(obj, value, null);
            }
            catch (Exception exp)
            {
                throw new Exception(string.Format(
                    "Cannot set property value for property {0}.{1}. Reason: {2}.",
                    this.originalProperty.DeclaringType.Name,
                    this.originalProperty.Name,
                    exp.Message), exp);
            }
        }

        public T GetValue<T>(object obj)
        {
            return (T)this.originalProperty.GetValue(obj, null);
        }

        public MappedParameterAttribute ToParameter()
        {
            var p = new MappedParameterAttribute
            {
                DefaultValue = this.DefaultValue,
                Direction = System.Data.ParameterDirection.Input,
                Ignore = this.Ignore,
                Include = this.Include,
                IsOptional = this.IsOptional,
                Precision = this.Precision,
                Size = this.Size,
                Name = this.Name,
                Schema = this.Schema,
                SqlType = this.SqlType
            };
            return p;    
        }
    }
}
