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
using System.Globalization;
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
    public class ColumnAttribute : DatabaseObjectAttribute
    {
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

        public TableAttribute Table { get; set; }

        /// <summary>
        /// Specifies the property maps to a column in a table that
        /// is not an auto-incrementing integer field and is not 
        /// included in the primary key. If the column is part of a
        /// foreign key relationship, it cascades deletes and updates
        /// and is named based both on the property name and the 
        /// foreign table's primary key column names. These default
        /// settings can be overridden.
        /// </summary>
        public ColumnAttribute()
        {
            IsIdentity = false;
            IncludeInPrimaryKey = false;
        }

        public ColumnAttribute(PropertyInfo obj)
            : this()
        {
            InferProperties(obj);
        }

        public ColumnAttribute(TableAttribute table, InformationSchema.Columns column, bool includeInPK, IDatabaseStateReader dal)
        {
            if (column is null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            if (dal is null)
            {
                throw new ArgumentNullException(nameof(dal));
            }

            Table = table ?? throw new ArgumentNullException(nameof(table));
            Name = column.column_name;
            var defVal = column.column_default;
            IsIdentity = dal.DescribesIdentity(column);
            DefaultValue = defVal;
            IncludeInPrimaryKey = includeInPK;
            Include = true;
            IsOptional = "yes".Equals(column.is_nullable, StringComparison.InvariantCultureIgnoreCase);

            InferTypeInfo(column, column.udt_name ?? column.data_type, dal);
        }

        private PropertyInfo originalProperty => (PropertyInfo)SourceObject;

        public void SetValue(object obj, object value)
        {
            if (value == DBNull.Value)
            {
                value = null;
            }

            var targetType = DataConnector.CoalesceNullableValueType(originalProperty.PropertyType);

            if (value != null)
            {
                if (targetType.IsEnum && value is string)
                {
                    value = Enum.Parse(targetType, (string)value);
                }
                else if (targetType.IsEnum && value is int)
                {
                    value = Enum.ToObject(targetType, (int)value);
                }
                else
                {
                    value = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                }
            }

            try
            {
                originalProperty.SetValue(obj, value, null);
            }
            catch (Exception exp)
            {
                throw new Exception($"Cannot set property value for property {originalProperty.DeclaringType.Name}.{originalProperty.Name}. Reason: {exp.Message}.", exp);
            }
        }

        public virtual T GetValue<T>(object obj)
        {
            return (T)originalProperty.GetValue(obj, null);
        }

        public virtual object GetValue(object obj)
        {
            return originalProperty.GetValue(obj, null);
        }

        public ParameterAttribute ToParameter()
        {
            var p = new ParameterAttribute
            {
                DefaultValue = DefaultValue,
                Direction = System.Data.ParameterDirection.Input,
                Name = Name,
                Schema = Schema,
                SqlType = SqlType
            };
            if (IsPrecisionSet)
            {
                p.Precision = Precision;
            }
            if (IsSizeSet)
            {
                p.Size = Size;
            }
            if (IsOptionalSet)
            {
                p.IsOptional = IsOptional;
            }
            if (IsIncludeSet)
            {
                p.Include = Include;
            }
            return p;
        }

        public override string ToString()
        {
            return $"COLUMN [{Table?.Schema}].[{Table?.Name}].[{Name}] {SystemType?.FullName}";
        }
    }
}
