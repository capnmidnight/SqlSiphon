using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon.Mapping
{
    public class PrimaryKey : DatabaseObjectAttribute
    {
        public static TableAttribute GetAttribute(Type type)
        {
            var attr = DatabaseObjectAttribute.GetAttribute<TableAttribute>(type);
            if (attr == null)
            {
                throw new Exception(string.Format("Class {0}.{1} is not mapped to a table", type.Namespace, type.Name));
            }
            attr.InferProperties(type);
            return attr;
        }

        public TableAttribute Table { get; private set; }
        public ColumnAttribute[] KeyColumns { get; private set; }

        public PrimaryKey(
            InformationSchema.TableConstraints constraint,
            InformationSchema.TableConstraints uniqueConstraint,
            InformationSchema.ConstraintColumnUsage[] uniqueConstraintColumns,
            InformationSchema.Columns[] uniqueTableColumns,
            ISqlSiphon dal)
        {
            this.Schema = constraint.constraint_schema;
            this.Name = constraint.constraint_name;
            this.Table = new TableAttribute(uniqueTableColumns, new InformationSchema.TableConstraints[] { uniqueConstraint }, null, uniqueConstraintColumns, null, dal);
            var uniqueColumns = uniqueTableColumns.ToDictionary(c => dal.MakeIdentifier(c.column_name));
            this.KeyColumns = uniqueConstraintColumns
                .Select(c => new ColumnAttribute(this.Table, uniqueColumns[dal.MakeIdentifier(c.column_name)], true, dal))
                .OrderBy(c => c.Name)
                .ToArray();
        }

        public PrimaryKey(Type toType)
        {
            this.Table = GetAttribute(toType);
            this.KeyColumns = this.Table.Properties
                .Where(p => p.IncludeInPrimaryKey)
                .OrderBy(c => c.Name)
                .ToArray();
        }

        public string GetName(ISqlSiphon dal)
        {
            return this.Name ?? string.Format(
                "pk_{0}_{1}",
                this.Table.Schema ?? dal.DefaultSchemaName,
                this.Table.Name)
                .Replace("__", "_");
        }
    }
}
