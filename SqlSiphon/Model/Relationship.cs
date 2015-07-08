using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.Model
{
    public class Relationship : DatabaseObjectAttribute
    {
        public Type ToType { get; private set; }
        private string[] fromColumnNames;

        public TableAttribute To { get; private set; }

        public TableAttribute From { get; private set; }

        public ColumnAttribute[] FromColumns { get; private set; }

        public bool AutoCreateIndex { get; private set; }

        public string Prefix { get; private set; }

        internal Relationship(
            InformationSchema.Columns[] fromTableColumns,
            InformationSchema.TableConstraints fromTableFKConstraint,
            InformationSchema.KeyColumnUsage[] fromTableFKConstraintColumns,
            InformationSchema.Columns[] toTableColumns,
            InformationSchema.TableConstraints toTablePKConstraint,
            InformationSchema.ConstraintColumnUsage[] toTablePKConstraintColumns,
            InformationSchema.KeyColumnUsage[] toTableKeyColumns,
            IDatabaseStateReader dal)
        {
            this.Schema = fromTableFKConstraint.constraint_schema;
            this.Name = fromTableFKConstraint.constraint_name;
            this.To = new TableAttribute(toTableColumns, new InformationSchema.TableConstraints[] { toTablePKConstraint }, fromTableFKConstraintColumns, toTablePKConstraintColumns, null, dal);
            this.To.PrimaryKey = new PrimaryKey(fromTableFKConstraint, toTablePKConstraint, toTablePKConstraintColumns, toTableColumns, dal);
            this.From = new TableAttribute(fromTableColumns, new InformationSchema.TableConstraints[] { fromTableFKConstraint }, fromTableFKConstraintColumns, null, null, dal);
            var fromColumns = fromTableColumns.ToDictionary(c => dal.MakeIdentifier(c.column_name));
            this.FromColumns = fromTableFKConstraintColumns.Select(c => new ColumnAttribute(this.From, fromColumns[dal.MakeIdentifier(c.column_name)], false, dal)).ToArray();
        }

        internal Relationship(string prefix, TableAttribute from, Type toType, bool autoCreateIndex, string[] fromColumns)
        {
            this.Prefix = prefix;
            this.ToType = toType;
            this.From = from;
            this.AutoCreateIndex = autoCreateIndex;
            this.fromColumnNames = fromColumns;
        }

        public void ResolveColumns(Dictionary<Type, TableAttribute> tables, IDatabaseScriptGenerator dal)
        {
            this.To = tables[this.ToType];
            if (this.To.PrimaryKey == null)
            {
                throw new Exception(string.Format("The target table {0} does not have a primary key defined.", dal.MakeIdentifier(this.To.Schema, this.To.Name)));
            }

            if (this.fromColumnNames == null)
            {
                var pk = this.To.PrimaryKey.KeyColumns.ToDictionary(p => (this.Prefix + p.Name).ToLowerInvariant());
                this.FromColumns = this.From.Properties
                    .Where(p => pk.ContainsKey(p.Name.ToLowerInvariant()))
                    .ToArray();
            }
            else
            {
                for (var i = 0; i < this.fromColumnNames.Length; ++i)
                {
                    this.fromColumnNames[i] = this.fromColumnNames[i].ToLowerInvariant();
                }
                this.FromColumns = this.From.Properties
                    .Where(p => this.fromColumnNames.Contains(p.Name.ToLowerInvariant()))
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
            this.Name = this.GetName(dal);
            if(this.AutoCreateIndex)
            {
                var fkIndex = new TableIndex(this.From, "idx_" + this.Name);
                fkIndex.Columns.AddRange(this.FromColumns.Select(c => c.Name));
                var fkIndexNameKey = dal.MakeIdentifier(this.From.Schema ?? dal.DefaultSchemaName, fkIndex.Name).ToLowerInvariant();
                this.From.Indexes.Add(fkIndexNameKey, fkIndex);
            }
        }

        public string GetName(IDatabaseScriptGenerator dal)
        {
            return this.Name ?? string.Format(
                "fk_{0}_from_{1}_{2}_to_{3}",
                this.Prefix,
                this.From.Schema ?? dal.DefaultSchemaName,
                this.From.Name,
                this.To.PrimaryKey.Name)
                .Replace("__", "_");
        }

        public override string ToString()
        {
            return string.Format("FK: {0} from {1} to {2}", this.Name, this.From.Name, this.To.Name);
        }
    }
}
