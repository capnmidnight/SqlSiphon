using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon.Mapping
{
    public class PrimaryKey : MappedObjectAttribute
    {
        public static MappedClassAttribute GetAttribute(Type type)
        {
            var attr = MappedObjectAttribute.GetAttribute<MappedClassAttribute>(type);
            if (attr == null)
            {
                throw new Exception(string.Format("Class {0}.{1} is not mapped to a table", type.Namespace, type.Name));
            }
            attr.InferProperties(type);
            return attr;
        }

        public MappedClassAttribute Table { get; private set; }
        public MappedPropertyAttribute[] KeyColumns { get; private set; }

        public PrimaryKey(
            InformationSchema.TableConstraints constraint,
            InformationSchema.TableConstraints uniqueConstraint,
            InformationSchema.ConstraintColumnUsage[] uniqueConstraintColumns,
            InformationSchema.Columns[] uniqueTableColumns,
            ISqlSiphon dal)
        {
            this.Schema = constraint.constraint_schema;
            this.Name = constraint.constraint_name;
            this.Table = new MappedClassAttribute(uniqueTableColumns, new InformationSchema.TableConstraints[] { uniqueConstraint }, null, uniqueConstraintColumns, dal);
            var uniqueColumns = uniqueTableColumns.ToDictionary(c => dal.MakeIdentifier(c.column_name));
            this.KeyColumns = uniqueConstraintColumns.Select(c => new MappedPropertyAttribute(this.Table, uniqueColumns[dal.MakeIdentifier(c.column_name)], true, dal)).ToArray();
        }

        public PrimaryKey(Type toType)
        {
            this.Table = GetAttribute(toType);
            this.Schema = this.Table.Schema;
            this.KeyColumns = this.Table.Properties
                .Where(p => p.IncludeInPrimaryKey)
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
