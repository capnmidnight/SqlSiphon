using System;
using System.Linq;

using SqlSiphon.Mapping;

namespace SqlSiphon.Model
{
    public class PrimaryKey : DatabaseObject
    {
        public TableAttribute Table { get; private set; }
        public ColumnAttribute[] KeyColumns { get; private set; }

        internal PrimaryKey(
            InformationSchema.TableConstraint constraint,
            InformationSchema.TableConstraint uniqueConstraint,
            InformationSchema.ConstraintColumnUsage[] uniqueConstraintColumns,
            InformationSchema.Column[] uniqueTableColumns,
            ISqlSiphon dal)
            : base(constraint.constraint_schema, constraint.constraint_name)
        {
            Table = new TableAttribute(uniqueTableColumns, new InformationSchema.TableConstraint[] { uniqueConstraint }, null, uniqueConstraintColumns, null, dal);
            KeyColumns = Table.Properties
                .Where(p => p.IncludeInPrimaryKey)
                .ToArray();
        }

        internal PrimaryKey(TableAttribute table, string name)
            : base(table.Schema, name)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
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

        internal PrimaryKey(ISqlSiphon dal, TableAttribute table)
            : this(table, $"pk_{table.Schema ?? dal?.DefaultSchemaName}_{table.Name}".Replace("__", "_"))
        {
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

        public override string ToString()
        {
            return $"[{Schema}].[{Name}]";
        }
    }
}
