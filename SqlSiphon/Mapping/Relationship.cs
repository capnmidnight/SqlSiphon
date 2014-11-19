using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon.Mapping
{
    public class Relationship : MappedObjectAttribute
    {
        private static MappedClassAttribute GetAttribute(Type type)
        {
            var attr = MappedObjectAttribute.GetAttribute<MappedClassAttribute>(type);
            if (attr == null)
            {
                throw new Exception(string.Format("Class {0}.{1} is not mapped to a table", type.Namespace, type.Name));
            }
            attr.InferProperties(type);
            return attr;
        }

        public MappedClassAttribute To { get; private set; }
        public MappedClassAttribute From { get; private set; }
        public MappedPropertyAttribute[] ToColumns { get; private set; }
        public MappedPropertyAttribute[] FromColumns { get; private set; }

        public Relationship(
            InformationSchema.TableConstraints constraint,
            InformationSchema.ConstraintColumnUsage[] constraintColumns,
            InformationSchema.Columns[] tableColumns,
            InformationSchema.TableConstraints uniqueConstraint,
            InformationSchema.ConstraintColumnUsage[] uniqueConstraintColumns,
            InformationSchema.Columns[] uniqueTableColumns,
            ISqlSiphon dal)
        {
            this.Schema = constraint.constraint_schema;
            this.Name = constraint.constraint_name;
            this.To = new MappedClassAttribute(uniqueTableColumns, new InformationSchema.TableConstraints[] { uniqueConstraint }, uniqueConstraintColumns, dal);
            this.From = new MappedClassAttribute(tableColumns, new InformationSchema.TableConstraints[] { constraint }, constraintColumns, dal);
            var columns = tableColumns.ToDictionary(c => dal.MakeIdentifier(c.column_name));
            var uniqueColumns = uniqueTableColumns.ToDictionary(c => dal.MakeIdentifier(c.column_name));
            this.ToColumns = uniqueConstraintColumns.Select(c => new MappedPropertyAttribute(this.To, uniqueColumns[c.column_name], true, dal)).ToArray();
            this.FromColumns = constraintColumns.Select(c => new MappedPropertyAttribute(this.From, columns[c.column_name], false, dal)).ToArray();
        }

        public Relationship(Type fromType, Type toType)
            : this("", fromType, toType, null)
        {
        }

        public Relationship(string prefix, Type fromType, Type toType)
            : this(prefix, fromType, toType, null)
        {
        }

        public Relationship(Type fromType, Type toType, params string[] fromColumns)
            : this("", fromType, toType, fromColumns)
        {
        }

        public Relationship(string prefix, Type fromType, Type toType, params string[] fromColumns)
        {
            this.To = GetAttribute(toType);
            this.From = GetAttribute(fromType);

            this.Schema = this.From.Schema;
            this.Name = string.Format(
                "fk_{0}_to_{1}_{2}_from_{3}_{4}",
                prefix,
                this.To.Schema,
                this.To.Name,
                this.From.Schema,
                this.From.Name)
                .Replace("__", "_");

            this.ToColumns = this.To.Properties
                .Where(p => p.IncludeInPrimaryKey)
                .ToArray();


            if (fromColumns == null)
            {
                var pk = this.ToColumns.ToDictionary(p => (prefix + p.Name).ToLower());
                this.FromColumns = this.From.Properties
                    .Where(p => pk.ContainsKey(p.Name.ToLower()))
                    .ToArray();
            }
            else
            {
                for (var i = 0; i < fromColumns.Length; ++i)
                {
                    fromColumns[i] = fromColumns[i].ToLower();
                }
                this.FromColumns = this.From.Properties
                    .Where(p => fromColumns.Contains(p.Name.ToLower()))
                    .ToArray();
            }

            if (this.ToColumns.Length != this.FromColumns.Length)
            {
                var availableColumns = this.FromColumns.Select(p => p.Name).ToList();
                throw new Exception(string.Format(
                    "Table {0}.{1} does not satisfy the constraints to relate to table {2}.{3}. Missing columns: {4}",
                    this.From.Schema, this.From.Name,
                    this.To.Schema, this.To.Name,
                    string.Join(", ", this.ToColumns.Where(p => !availableColumns.Contains(prefix + p.Name)))));
            }

            for (int i = 0; i < this.ToColumns.Length; ++i)
            {
                var a = this.ToColumns[i];
                var b = this.FromColumns[i];
                if (a.SystemType != b.SystemType)
                {
                    throw new Exception(string.Format(
                        "FK column {0} in {1}.{2} does not match column {3} in {4}.{5}. Expected: {6}. Received: {7}.",
                        b.Name, this.From.Schema, this.From.Name,
                        a.Name, this.To.Schema, this.To.Name,
                        a.SystemType.Name, b.SystemType.Name));
                }
            }
        }
    }
}
