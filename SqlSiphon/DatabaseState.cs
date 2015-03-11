using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Runtime.InteropServices;
using SqlSiphon.Mapping;

namespace SqlSiphon
{
    public class DatabaseState
    {
        public Dictionary<string, TableAttribute> Tables { get; private set; }
        public Dictionary<string, Index> Indexes { get; private set; }
        public Dictionary<string, RoutineAttribute> Functions { get; private set; }
        public Dictionary<string, Relationship> Relationships { get; private set; }
        public Dictionary<string, PrimaryKey> PrimaryKeys { get; private set; }
        public List<string> Schemata { get; private set; }

        private DatabaseState()
        {
            this.Tables = new Dictionary<string, TableAttribute>();
            this.Indexes = new Dictionary<string, Index>();
            this.Functions = new Dictionary<string, RoutineAttribute>();
            this.Relationships = new Dictionary<string, Relationship>();
            this.PrimaryKeys = new Dictionary<string, PrimaryKey>();
            this.Schemata = new List<string>();
        }

        /// <summary>
        /// Scans an assembly object for types and methods that define
        /// mappings to database objects.
        /// </summary>
        /// <param name="asm"></param>
        public DatabaseState(IEnumerable<Type> types, ISqlSiphon dal)
            : this()
        {
            foreach (var type in types)
            {
                AddType(type, dal);
            }
        }

        public void AddType(Type type, ISqlSiphon dal)
        {
            var table = DatabaseObjectAttribute.GetAttribute<TableAttribute>(type);
            if (table != null)
            {
                table.InferProperties(type);
                table.Schema = table.Schema ?? dal.DefaultSchemaName;
                if (table.Include)
                {
                    this.Tables.Add(dal.MakeIdentifier(table.Schema ?? dal.DefaultSchemaName, table.Name), table);
                    if (table.Properties.Any(p => p.IncludeInPrimaryKey))
                    {
                        var key = new PrimaryKey(type);
                        this.PrimaryKeys.Add(dal.MakeIdentifier(key.Schema ?? dal.DefaultSchemaName, key.GetName(dal)), key);
                    }
                    foreach (var index in table.Indexes)
                    {
                        this.Indexes.Add(index.Key, index.Value);
                    }
                }
            }

            if (type.GetInterface("ISqlSiphon") != null)
            {
                var methods = type.GetMethods();
                foreach (var method in methods)
                {
                    var function = dal.GetCommandDescription(method);
                    if (function != null)
                    {
                        if (function.CommandType == System.Data.CommandType.StoredProcedure)
                        {
                            this.Functions.Add(dal.MakeIdentifier(function.Schema ?? dal.DefaultSchemaName, function.Name), function);
                        }
                    }
                }

                var rt = typeof(Relationship);
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var field in fields)
                {
                    if (field.FieldType == rt)
                    {
                        var r = (Relationship)field.GetValue(null);
                        var id = dal.MakeIdentifier(r.Schema ?? dal.DefaultSchemaName, r.GetName(dal));
                        this.Relationships.Add(id, r);
                    }
                }
            }

            this.Schemata.AddRange(this.Tables.Values.Select(t => t.Schema)
                .Union(this.Functions.Values.Select(f => f.Schema))
                .Union(this.Relationships.Values.Select(r => r.Schema))
                .Union(this.PrimaryKeys.Values.Select(r => r.Schema))
                .Where(s => !string.IsNullOrWhiteSpace(s) && !this.Schemata.Contains(s))
                .Distinct());
        }

        /// <summary>
        /// Scans a database for tables and stored procedures
        /// </summary>
        /// <param name="dal"></param>
        public DatabaseState(Regex filter, ISqlSiphon dal)
            : this()
        {
            try
            {
                this.Schemata.AddRange(dal.GetSchemata());
                var columns = dal.GetColumns().ToHash(col => dal.MakeIdentifier(col.table_schema, col.table_name));
                var constraints = dal.GetTableConstraints();
                var constraintsByTable = constraints.ToHash(cst => dal.MakeIdentifier(cst.table_schema, cst.table_name));
                var constraintsByName = constraints.ToDictionary(cst => dal.MakeIdentifier(cst.constraint_schema, cst.constraint_name));

                var keyColumns = dal.GetKeyColumns();
                var keyColumnsByTable = keyColumns.ToHash(col => dal.MakeIdentifier(col.table_schema, col.table_name));
                var keyColumnsByName = keyColumns.ToHash(col => dal.MakeIdentifier(col.constraint_schema, col.constraint_name));

                var xref = dal.GetReferentialConstraints()
                    .GroupBy(col => dal.MakeIdentifier(col.constraint_schema, col.constraint_name))
                    .ToDictionary(g => g.Key, g => g.Select(q => dal.MakeIdentifier(q.unique_constraint_schema, q.unique_constraint_name)).First());

                var constraintsColumns = dal.GetConstraintColumns();
                var constraintsColumnsByTable = constraintsColumns.ToHash(col => dal.MakeIdentifier(col.table_schema, col.table_name));
                var constraintsColumnsByName = constraintsColumns.ToHash(col => dal.MakeIdentifier(col.constraint_schema, col.constraint_name));

                var indexedColumns = dal.GetIndexColumns();
                var indexedColumnsByTable = indexedColumns.ToHash(col => dal.MakeIdentifier(col.table_schema, col.table_name));
                var indexedColumnsByName = indexedColumns.ToHash(col => col.index_name);

                foreach (var tableName in columns.Keys)
                {
                    var tableColumns = columns[tableName];
                    if (!filter.IsMatch(tableColumns[0].table_name))
                    {
                        var tableConstraints = constraintsByTable.ContainsKey(tableName) ? constraintsByTable[tableName] : new InformationSchema.TableConstraints[] { };
                        var tableKeyColumns = keyColumnsByTable.ContainsKey(tableName) ? keyColumnsByTable[tableName] : new InformationSchema.KeyColumnUsage[] { };
                        var tableConstraintColumns = constraintsColumnsByTable.ContainsKey(tableName) ? constraintsColumnsByTable[tableName] : new InformationSchema.ConstraintColumnUsage[] { };
                        var tableIndexedColumns = indexedColumnsByTable.ContainsKey(tableName) ? indexedColumnsByTable[tableName] : new InformationSchema.IndexColumnUsage[] { };
                        this.Tables.Add(tableName, new TableAttribute(tableColumns, tableConstraints, tableKeyColumns, tableConstraintColumns, tableIndexedColumns, dal));
                        if (tableConstraints != null)
                        {
                            foreach (var constraint in tableConstraints)
                            {
                                var constraintName = dal.MakeIdentifier(constraint.constraint_schema, constraint.constraint_name);
                                if (keyColumnsByName.ContainsKey(constraintName))
                                {
                                    var constraintColumns = keyColumnsByName[constraintName];
                                    var uniqueConstraintName = constraint.constraint_type == "FOREIGN KEY" ? xref[constraintName] : constraintName;
                                    var uniqueConstraint = constraintsByName[uniqueConstraintName];
                                    var uniqueConstraintColumns = constraintsColumnsByName[uniqueConstraintName];
                                    var uniqueTableColumns = columns[dal.MakeIdentifier(uniqueConstraint.table_schema, uniqueConstraint.table_name)];
                                    if (constraint.constraint_type == "FOREIGN KEY")
                                    {
                                        this.Relationships.Add(constraintName, new Relationship(
                                            constraint, constraintColumns, tableColumns,
                                            uniqueConstraint, uniqueConstraintColumns, uniqueTableColumns,
                                            dal));
                                    }
                                    else if (constraint.constraint_type == "PRIMARY KEY")
                                    {
                                        this.PrimaryKeys.Add(constraintName, new PrimaryKey(constraint, uniqueConstraint, uniqueConstraintColumns, uniqueTableColumns, dal));
                                    }
                                }
                            }
                        }
                    }
                }

                var routines = dal.GetRoutines()
                    .Where(r => !filter.IsMatch(r.routine_name))
                    .ToDictionary(prm => dal.MakeIdentifier(prm.specific_schema, prm.specific_name));
                var parameters = dal.GetParameters()
                    .GroupBy(prm => dal.MakeIdentifier(prm.specific_schema, prm.specific_name))
                    .ToDictionary(g => g.Key, g => g.ToArray());
                foreach (var key in routines.Keys)
                {
                    var routine = routines[key];
                    var routineParameters = parameters.ContainsKey(key) ? parameters[key] : null;
                    var ident = dal.MakeIdentifier(routine.routine_schema, routine.routine_name);
                    this.Functions.Add(ident, new RoutineAttribute(routine, routineParameters, dal));
                }

                foreach (var idxName in indexedColumnsByName.Keys)
                {
                    var idx = indexedColumnsByName[idxName];
                    var tableName = dal.MakeIdentifier(idx[0].table_schema, idx[0].table_name);
                    if (this.Tables.ContainsKey(tableName))
                    {
                        var table = this.Tables[tableName];
                        this.Indexes.Add(idxName, new Index(table, idxName));
                        foreach (var idxCol in idx)
                        {
                            this.Indexes[idxName].Columns.Add(idxCol.column_name);
                        }
                    }
                }
            }
            catch (ConnectionFailedException)
            {
                // just ignore it, it usually means the catalog hasn't been created yet.
            }
        }
    }
}
