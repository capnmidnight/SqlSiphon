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
            IDatabaseStateReader dal)
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
            Schema = table.Schema;
            Table = table;
            KeyColumns = table.Properties
                .Where(p => p.IncludeInPrimaryKey)
                .ToArray();
            var nullableColumns = KeyColumns.Where(c => c.IsOptional).ToArray();
            if (nullableColumns.Length > 0)
            {
                throw new PrimaryKeyColumnNotNullableException(Table, nullableColumns);
            }
            var tooLongStringColumns = KeyColumns.Where(c => c.SystemType == typeof(string) && !c.IsSizeSet).ToArray();
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
                return base.Name ?? (Table != null ? string.Format(
                "pk_{0}_{1}",
                Table.Schema,
                Table.Name).Replace("__", "_") : null);
            }
            set
            {
                base.Name = value;
            }
        }

        internal TableIndex ToIndex()
        {
            var idx = new TableIndex(Table, Name)
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
