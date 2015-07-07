using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            this.Schema = constraint.constraint_schema;
            this.Name = constraint.constraint_name;
            this.Table = new TableAttribute(uniqueTableColumns, new InformationSchema.TableConstraints[] { uniqueConstraint }, null, uniqueConstraintColumns, null, dal);
            this.KeyColumns = this.Table.Properties
                .Where(p => p.IncludeInPrimaryKey)
                .ToArray();
        }

        internal PrimaryKey(TableAttribute table)
        {
            this.Schema = table.Schema;
            this.Table = table;
            this.KeyColumns = table.Properties
                .Where(p => p.IncludeInPrimaryKey)
                .ToArray();
        }

        public override string Schema
        {
            get
            {
                return base.Schema ?? (Table != null ? Table.Schema : null);
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
                this.Table.Schema,
                this.Table.Name).Replace("__", "_") : null);
            }
            set
            {
                base.Name = value;
            }
        }

        internal TableIndex ToIndex()
        {
            var idx = new TableIndex(this.Table, this.Name);
            idx.IsClustered = true;
            foreach (var column in this.KeyColumns)
            {
                idx.Columns.Add(column.Name);
            }
            return idx;
        }
    }
}
