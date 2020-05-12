using System;
using System.Linq;

using SqlSiphon.Mapping;

namespace SqlSiphon.Model
{
    public class PrimaryKey : DatabaseObjectAttribute
    {
        public TableAttribute Table { get; private set; }
        public ColumnAttribute[] KeyColumns { get; private set; }

        internal PrimaryKey(
            InformationSchema.TableConstraints constraint,
            InformationSchema.TableConstraints uniqueConstraint,
            InformationSchema.ConstraintColumnUsage[] uniqueConstraintColumns,
            InformationSchema.Columns[] uniqueTableColumns,
            ISqlSiphon dal)
        {
            Schema = constraint.constraint_schema;
            Name = constraint.constraint_name;
            Table = new TableAttribute(uniqueTableColumns, new InformationSchema.TableConstraints[] { uniqueConstraint }, null, uniqueConstraintColumns, null, dal);
            KeyColumns = Table.Properties
                .Where(p => p.IncludeInPrimaryKey)
                .ToArray();
        }

        internal PrimaryKey(TableAttribute table)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            Schema = table.Schema;
            KeyColumns = table.Properties
                .Where(p => p.IncludeInPrimaryKey)
                .ToArray();
            var nullableColumns = KeyColumns.Where(c => c.IsOptional).ToArray();
            if (nullableColumns.Length > 0)
            {
                throw new PrimaryKeyColumnNotNullableException(Table, nullableColumns);
            }
            var tooLongStringColumns = KeyColumns.Where(c => c.SystemType == typeof(string) && !c.IsStringLengthSet).ToArray();
            if (tooLongStringColumns.Length > 0)
            {
                throw new MustSetStringSizeInPrimaryKeyException(Table, tooLongStringColumns);
            }
        }

        public override string Schema
        {
            get
            {
                return base.Schema ?? Table?.Schema;
            }
            set
            {
                base.Schema = value;
            }
        }

        public override string Name
        {
            get
            {
                if (base.Name is object)
                {
                    return base.Name;
                }
                else if (Table is object)
                {
                    return $"pk_{Table.Schema}_{Table.Name}".Replace("__", "_");
                }
                else
                {
                    return null;
                }
            }
            set
            {
                base.Name = value;
            }
        }

        internal TableIndex ToIndex()
        {
            var idx = new TableIndex(Table, Schema, Name)
            {
                IsClustered = true
            };
            foreach (var column in KeyColumns)
            {
                idx.Columns.Add(column.Name);
            }
            return idx;
        }
    }
}
