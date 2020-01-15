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
            InformationSchema.Columns[] fromTableColumns,
            InformationSchema.TableConstraints fromTableFKConstraint,
            InformationSchema.KeyColumnUsage[] fromTableFKConstraintColumns,
            InformationSchema.Columns[] toTableColumns,
            InformationSchema.TableConstraints toTablePKConstraint,
            InformationSchema.ConstraintColumnUsage[] toTablePKConstraintColumns,
            InformationSchema.KeyColumnUsage[] toTableKeyColumns,
            IDatabaseStateReader dal)
        {
            Schema = fromTableFKConstraint.constraint_schema;
            Name = fromTableFKConstraint.constraint_name;
            To = new TableAttribute(toTableColumns, new InformationSchema.TableConstraints[] { toTablePKConstraint }, fromTableFKConstraintColumns, toTablePKConstraintColumns, null, dal)
            {
                PrimaryKey = new PrimaryKey(fromTableFKConstraint, toTablePKConstraint, toTablePKConstraintColumns, toTableColumns, dal)
            };
            From = new TableAttribute(fromTableColumns, new InformationSchema.TableConstraints[] { fromTableFKConstraint }, fromTableFKConstraintColumns, null, null, dal);
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

        public void ResolveColumns(Dictionary<Type, TableAttribute> tables, IDatabaseScriptGenerator dal)
        {
            To = tables[ToType];
            if (To.PrimaryKey == null)
            {
                throw new Exception(string.Format("The target table {0} does not have a primary key defined.", dal.MakeIdentifier(To.Schema, To.Name)));
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
                throw new Exception(string.Format(
                    "Table {0}.{1} does not satisfy the constraints to relate to table {2}.{3}. Missing columns: {4}",
                    From.Schema, From.Name,
                    To.Schema, To.Name,
                    string.Join(", ", To.PrimaryKey.KeyColumns.Where(p => !availableColumns.Contains(Prefix + p.Name)))));
            }

            for (var i = 0; i < To.PrimaryKey.KeyColumns.Length; ++i)
            {
                var a = To.PrimaryKey.KeyColumns[i];
                var b = FromColumns[i];
                if (a.SystemType != b.SystemType)
                {
                    throw new Exception(string.Format(
                        "FK column {0} in {1}.{2} does not match column {3} in {4}.{5}. Expected: {6}. Received: {7}.",
                        b.Name, From.Schema, From.Name,
                        a.Name, To.Schema, To.Name,
                        a.SystemType.Name, b.SystemType.Name));
                }
            }
            Name = GetName(dal);
            if (AutoCreateIndex)
            {
                var fkIndex = new TableIndex(From, "idx_" + Name);
                fkIndex.Columns.AddRange(FromColumns.Select(c => c.Name));
                var fkIndexNameKey = dal.MakeIdentifier(From.Schema ?? dal.DefaultSchemaName, fkIndex.Name).ToLowerInvariant();
                From.Indexes.Add(fkIndexNameKey, fkIndex);
            }
        }

        public string GetName(IDatabaseScriptGenerator dal)
        {
            return Name ?? string.Format(
                "fk_{0}_from_{1}_{2}_to_{3}",
                Prefix,
                From.Schema ?? dal.DefaultSchemaName,
                From.Name,
                To.PrimaryKey.Name)
                .Replace("__", "_");
        }

        public override string ToString()
        {
            return string.Format("FK: {0} from {1} to {2}", Name, From.Name, To.Name);
        }
    }
}
