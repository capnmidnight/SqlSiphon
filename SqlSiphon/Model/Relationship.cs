using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon.Mapping
{
    public class Relationship : DatabaseObjectAttribute
    {
        private Type toType, fromType;
        private string[] fromColumnNames;

        public TableAttribute To { get; private set; }
        public TableAttribute From { get; private set; }
        public ColumnAttribute[] FromColumns { get; private set; }

        public string Prefix { get; private set; }

        internal Relationship(
            InformationSchema.TableConstraints constraint,
            InformationSchema.KeyColumnUsage[] constraintColumns,
            InformationSchema.Columns[] tableColumns,
            InformationSchema.TableConstraints uniqueConstraint,
            InformationSchema.ConstraintColumnUsage[] uniqueConstraintColumns,
            InformationSchema.Columns[] uniqueTableColumns,
            IDatabaseStateReader dal)
        {
            this.Schema = constraint.constraint_schema;
            this.Name = constraint.constraint_name;
            this.To = new TableAttribute(uniqueTableColumns, new InformationSchema.TableConstraints[]{uniqueConstraint}, constraintColumns, uniqueConstraintColumns, null, dal);
            this.To.PrimaryKey = new PrimaryKey(constraint, uniqueConstraint, uniqueConstraintColumns, uniqueTableColumns, dal);
            this.From = new TableAttribute(tableColumns, new InformationSchema.TableConstraints[] { constraint }, constraintColumns, null, null, dal);
            var columns = tableColumns.ToDictionary(c => dal.MakeIdentifier(c.column_name));
            var uniqueColumns = uniqueTableColumns.ToDictionary(c => dal.MakeIdentifier(c.column_name));
            this.FromColumns = constraintColumns.Select(c => new ColumnAttribute(this.From, columns[dal.MakeIdentifier(c.column_name)], false, dal)).ToArray();
        }

        internal Relationship(string prefix, Type fromType, Type toType, params string[] fromColumns)
        {
            this.Prefix = prefix ?? "";
            this.toType = toType;
            this.fromType = fromType;
            if (fromColumns != null && fromColumns.Length > 0)
            {
                this.fromColumnNames = fromColumns;
            }
        }

        public void ResolveColumns(Dictionary<string, TableAttribute> tables, IDatabaseScriptGenerator dal)
        {
            this.To = tables.Values.Where(t => t.SystemType == this.toType).FirstOrDefault();
            this.From = tables.Values.Where(t => t.SystemType == this.fromType).FirstOrDefault();
            if (this.To.PrimaryKey == null)
            {
                throw new Exception(string.Format("The target table {0} does not have a primary key defined.", dal.MakeIdentifier(this.To.Schema, this.To.Name)));
            }

            if (this.fromColumnNames == null)
            {
                var pk = this.To.PrimaryKey.KeyColumns.ToDictionary(p => (this.Prefix + p.Name).ToLower());
                this.FromColumns = this.From.Properties
                    .Where(p => pk.ContainsKey(p.Name.ToLower()))
                    .ToArray();
            }
            else
            {
                for (var i = 0; i < this.fromColumnNames.Length; ++i)
                {
                    this.fromColumnNames[i] = this.fromColumnNames[i].ToLower();
                }
                this.FromColumns = this.From.Properties
                    .Where(p => this.fromColumnNames.Contains(p.Name.ToLower()))
                    .ToArray();
            }

            if (this.To.PrimaryKey.KeyColumns.Length != this.FromColumns.Length)
            {
                var availableColumns = this.FromColumns.Select(p => p.Name).ToList();
                throw new Exception(string.Format(
                    "Table {0}.{1} does not satisfy the constraints to relate to table {2}.{3}. Missing columns: {4}",
                    this.From.Schema, this.From.Name,
                    this.To.Schema, this.To.Name,
                    string.Join(", ", this.To.PrimaryKey.KeyColumns.Where(p => !availableColumns.Contains(this.Prefix + p.Name)))));
            }

            for (int i = 0; i < this.To.PrimaryKey.KeyColumns.Length; ++i)
            {
                var a = this.To.PrimaryKey.KeyColumns[i];
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

        public string GetName(IDatabaseScriptGenerator dal)
        {
            return this.Name ?? string.Format(
                "fk_{0}_from_{1}_{2}_{3}",
                this.Prefix,
                this.From.Schema ?? dal.DefaultSchemaName,
                this.From.Name,
                this.To.PrimaryKey.GetName(dal))
                .Replace("__", "_");
        }
    }
}
