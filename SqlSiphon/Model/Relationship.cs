using System;
using System.Collections.Generic;
using System.Linq;

using SqlSiphon.Mapping;

namespace SqlSiphon.Model
{
    public class Relationship : DatabaseObjectAttribute
    {
        public Type ToType { get; private set; }
        private readonly string[] fromColumnNames;

        public TableAttribute To { get; private set; }

        public TableAttribute From { get; private set; }

        public ColumnAttribute[] FromColumns { get; private set; }

        public bool AutoCreateIndex { get; private set; }

        public string Prefix { get; private set; }

        internal Relationship(
            InformationSchema.Column[] fromTableColumns,
            InformationSchema.TableConstraint fromTableFKConstraint,
            InformationSchema.KeyColumnUsage[] fromTableFKConstraintColumns,
            InformationSchema.Column[] toTableColumns,
            InformationSchema.TableConstraint toTablePKConstraint,
            InformationSchema.ConstraintColumnUsage[] toTablePKConstraintColumns,
            InformationSchema.KeyColumnUsage[] toTableKeyColumns,
            ISqlSiphon dal)
        {
            Schema = fromTableFKConstraint.constraint_schema;
            Name = fromTableFKConstraint.constraint_name;
            To = new TableAttribute(toTableColumns, new InformationSchema.TableConstraint[] { toTablePKConstraint }, fromTableFKConstraintColumns, toTablePKConstraintColumns, null, dal)
            {
                PrimaryKey = new PrimaryKey(fromTableFKConstraint, toTablePKConstraint, toTablePKConstraintColumns, toTableColumns, dal)
            };
            From = new TableAttribute(fromTableColumns, new InformationSchema.TableConstraint[] { fromTableFKConstraint }, fromTableFKConstraintColumns, null, null, dal);
            var fromColumns = fromTableColumns.ToDictionary(c => dal.MakeIdentifier(c.column_name));
            FromColumns = fromTableFKConstraintColumns.Select(c => new ColumnAttribute(From, fromColumns[dal.MakeIdentifier(c.column_name)], false, dal)).ToArray();
        }

        internal Relationship(string prefix, TableAttribute from, Type toType, bool autoCreateIndex, string[] fromColumns)
        {
            Prefix = prefix;
            ToType = toType;
            From = from;
            AutoCreateIndex = autoCreateIndex;
            fromColumnNames = fromColumns;
        }

        public void ResolveColumns(Dictionary<Type, TableAttribute> tables, ISqlSiphon dal)
        {
            if (tables is null)
            {
                throw new ArgumentNullException(nameof(tables));
            }

            if (dal is null)
            {
                throw new ArgumentNullException(nameof(dal));
            }

            To = tables[ToType];
            if (To.PrimaryKey == null)
            {
                var toTableName = dal.MakeIdentifier(To.Schema, To.Name);
                throw new Exception($"The target table {toTableName} does not have a primary key defined.");
            }

            if (fromColumnNames == null)
            {
                var pk = To.PrimaryKey.KeyColumns.ToDictionary(p => (Prefix + p.Name).ToLowerInvariant());
                FromColumns = From.Properties
                    .Where(p => pk.ContainsKey(p.Name.ToLowerInvariant()))
                    .ToArray();
            }
            else
            {
                for (var i = 0; i < fromColumnNames.Length; ++i)
                {
                    fromColumnNames[i] = fromColumnNames[i].ToLowerInvariant();
                }
                FromColumns = From.Properties
                    .Where(p => fromColumnNames.Contains(p.Name.ToLowerInvariant()))
                    .ToArray();
            }

            if (To.PrimaryKey.KeyColumns.Length != FromColumns.Length)
            {
                var availableColumns = FromColumns.Select(p => p.Name).ToList();
                var availableKeyColumns = To.PrimaryKey.KeyColumns.Where(p => !availableColumns.Contains(Prefix + p.Name));
                var availableKeyColumnsStr = string.Join(", ", availableKeyColumns);
                throw new Exception($"Table {From.Schema}.{From.Name} does not satisfy the constraints to relate to table {To.Schema}.{To.Name}. Missing columns: {availableKeyColumnsStr}");
            }

            for (var i = 0; i < To.PrimaryKey.KeyColumns.Length; ++i)
            {
                var a = To.PrimaryKey.KeyColumns[i];
                var b = FromColumns[i];
                if (a.SystemType != b.SystemType)
                {
                    throw new Exception($"FK column {b.Name} in {From.Schema}.{From.Name} does not match column {a.Name} in {To.Schema}.{To.Name}. Expected: {a.SystemType.Name}. Received: {b.SystemType.Name}.");
                }
            }
            Name = GetRelationshipName(dal);
            if (AutoCreateIndex)
            {
                var fkIndex = new TableIndex(From, Schema, "idx_" + Name);
                fkIndex.Columns.AddRange(FromColumns.Select(c => c.Name));
                var fkIndexNameKey = dal.MakeIdentifier(From.Schema ?? dal.DefaultSchemaName, fkIndex.Name).ToLowerInvariant();
                From.Indexes.Add(fkIndexNameKey, fkIndex);
            }
        }

        public string GetRelationshipName(ISqlSiphon dal)
        {
            if (dal is null)
            {
                throw new ArgumentNullException(nameof(dal));
            }

            var fromSchemaName = From.Schema ?? dal.DefaultSchemaName;
            return base.Name ?? $"fk_{Prefix}_from_{fromSchemaName}_{From.Name}_to_{To.PrimaryKey.Name}"
                .Replace("__", "_");
        }

        public override string ToString()
        {
            return $"FK: {Name} from {From.Name} to {To.Name}";
        }
    }
}
