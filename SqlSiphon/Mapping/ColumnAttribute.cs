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
        public ViewAttribute View { get; set; }

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

        public ColumnAttribute(TableAttribute table, InformationSchema.Column column, bool includeInPK, ISqlSiphon dal)
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

        public ColumnAttribute(ViewAttribute view, InformationSchema.Column column, ISqlSiphon dal)
        {
            if (column is null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            if (dal is null)
            {
                throw new ArgumentNullException(nameof(dal));
            }

            View = view ?? throw new ArgumentNullException(nameof(view));
            Name = column.column_name;
            var defVal = column.column_default;
            IsIdentity = dal.DescribesIdentity(column);
            DefaultValue = defVal;
            IncludeInPrimaryKey = false;
            Include = true;
            IsOptional = "yes".Equals(column.is_nullable, StringComparison.InvariantCultureIgnoreCase);

            InferTypeInfo(column, column.udt_name ?? column.data_type, dal);
        }

        private PropertyInfo OriginalProperty => (PropertyInfo)SourceObject;

        public void SetValue(object obj, object value)
        {
            if (value == DBNull.Value)
            {
                value = null;
            }

            var targetType = DataConnector.CoalesceNullableValueType(OriginalProperty.PropertyType);

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
                OriginalProperty.SetValue(obj, value, null);
            }
            catch (Exception exp)
            {
                throw new Exception($"Cannot set property value for property {OriginalProperty.DeclaringType.Name}.{OriginalProperty.Name}. Reason: {exp.Message}.", exp);
            }
        }

        public virtual T GetValue<T>(object obj)
        {
            return (T)OriginalProperty.GetValue(obj, null);
        }

        public virtual object GetValue(object obj)
        {
            return OriginalProperty.GetValue(obj, null);
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
            if (IsNumericPrecisionSet)
            {
                p.NumericPrecision = NumericPrecision;
            }
            if (IsStringLengthSet)
            {
                p.StringLength = StringLength;
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
