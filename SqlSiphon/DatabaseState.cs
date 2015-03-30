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
        public bool? CatalogueExists { get; private set; }
        public string CatalogueName { get; private set; }
        public Dictionary<string, TableAttribute> Tables { get; private set; }
        public Dictionary<string, Index> Indexes { get; private set; }
        public Dictionary<string, RoutineAttribute> Functions { get; private set; }
        public Dictionary<string, Relationship> Relationships { get; private set; }
        public Dictionary<string, PrimaryKey> PrimaryKeys { get; private set; }
        public List<string> Schemata { get; private set; }
        public Dictionary<string, string> DatabaseLogins { get; private set; }

        /// <summary>
        /// True-base constructor, forces the other constructors onto a path of correct initialization
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="indexes"></param>
        /// <param name="routines"></param>
        /// <param name="relationships"></param>
        /// <param name="pks"></param>
        /// <param name="schemata"></param>
        /// <param name="logins"></param>
        private DatabaseState(Dictionary<string, TableAttribute> tables, Dictionary<string, Index> indexes,
            Dictionary<string, RoutineAttribute> routines, Dictionary<string, Relationship> relationships,
            Dictionary<string, PrimaryKey> pks, List<string> schemata, Dictionary<string, string> logins,
            string catalogueName, bool? catalogueExists)
        {
            this.Tables = tables;
            this.Indexes = indexes;
            this.Functions = routines;
            this.Relationships = relationships;
            this.PrimaryKeys = pks;
            this.Schemata = schemata;
            this.DatabaseLogins = logins;
            this.CatalogueExists = catalogueExists;
            this.CatalogueName = catalogueName;
        }

        private DatabaseState()
            : this(new Dictionary<string, TableAttribute>(), new Dictionary<string, Index>(), new Dictionary<string, RoutineAttribute>(),
            new Dictionary<string, Relationship>(), new Dictionary<string, PrimaryKey>(), new List<string>(), new Dictionary<string, string>(),
            null, null)
        {
        }

        public DatabaseState(DatabaseState copy)
            : this(copy.Tables, copy.Indexes, copy.Functions, copy.Relationships, copy.PrimaryKeys, copy.Schemata, copy.DatabaseLogins, copy.CatalogueName, copy.CatalogueExists)
        {
        }

        /// <summary>
        /// Scans an assembly object for types and methods that define
        /// mappings to database objects.
        /// </summary>
        /// <param name="asm"></param>
        public DatabaseState(IEnumerable<Type> types, IAssemblyStateReader asm, IDatabaseScriptGenerator dal, string userName, string password)
            : this()
        {
            if (!string.IsNullOrWhiteSpace(userName))
            {
                this.DatabaseLogins.Add(userName, password);
            }

            var relationships = new List<Relationship>();
            var methodDefTypes = new List<Type>();

            foreach (var type in types)
            {
                if (type.GetInterface("ISqlSiphon") != null)
                {
                    methodDefTypes.Add(type);
                }

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
                            table.PrimaryKey = new PrimaryKey(type);
                            this.PrimaryKeys.Add(dal.MakeIdentifier(table.PrimaryKey.Schema ?? dal.DefaultSchemaName, table.PrimaryKey.GetName(dal)), table.PrimaryKey);
                        }
                        foreach (var index in table.Indexes)
                        {
                            this.Indexes.Add(index.Key, index.Value);
                        }
                    }
                }

                var fks = DatabaseObjectAttribute.GetAttributes<FKAttribute>(type);
                if (fks != null)
                {
                    foreach (var fk in fks)
                    {
                        relationships.Add(fk.MakeRelationshipObject(type));
                    }
                }

                var rt = typeof(Relationship);
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var field in fields)
                {
                    if (field.FieldType == rt)
                    {
                        relationships.Add((Relationship)field.GetValue(null));
                    }
                }
            }

            foreach (var relationship in relationships)
            {
                relationship.ResolveColumns(this.Tables, dal);
                var id = dal.MakeIdentifier(relationship.Schema ?? dal.DefaultSchemaName, relationship.GetName(dal));
                this.Relationships.Add(id, relationship);
            }

            foreach (var type in methodDefTypes)
            {
                var methods = type.GetMethods();
                foreach (var method in methods)
                {
                    var function = RoutineAttribute.GetCommandDescription(method);
                    if (function != null && function.CommandType == System.Data.CommandType.StoredProcedure)
                    {
                        this.Functions.Add(dal.MakeRoutineIdentifier(function), function);
                    }
                }
            }

            this.Schemata.AddRange(this.GetSchemata(dal));
        }

        private List<string> GetSchemata(IDatabaseScriptGenerator dal)
        {
            var schemata = this.Tables.Values.Select(t => t.Schema)
                .Union(this.Functions.Values.Select(f => f.Schema))
                .Union(this.Relationships.Values.Select(r => r.Schema))
                .Union(this.PrimaryKeys.Values.Select(r => r.Schema))
                .Where(s => !string.IsNullOrWhiteSpace(s) && !this.Schemata.Contains(s) && s != dal.DefaultSchemaName)
                .Distinct()
                .ToList();
            return schemata;
        }

        /// <summary>
        /// Scans a database for tables and stored procedures
        /// </summary>
        /// <param name="dal"></param>
        public DatabaseState(string catalogueName, Regex filter, ISqlSiphon dal)
            : this()
        {
            this.CatalogueName = catalogueName;
            try
            {
                foreach (var name in dal.GetDatabaseLogins())
                {
                    this.DatabaseLogins.Add(name, null);
                }
                this.CatalogueExists = true;
                var schemas = dal.GetSchemata();
                if (schemas.Count > 0)
                {
                    this.Schemata.AddRange(schemas);
                }
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
                    if (filter == null || !filter.IsMatch(tableColumns[0].table_name))
                    {
                        var tableConstraints = constraintsByTable.ContainsKey(tableName) ? constraintsByTable[tableName] : new InformationSchema.TableConstraints[] { };
                        var tableKeyColumns = keyColumnsByTable.ContainsKey(tableName) ? keyColumnsByTable[tableName] : new InformationSchema.KeyColumnUsage[] { };
                        var tableConstraintColumns = constraintsColumnsByTable.ContainsKey(tableName) ? constraintsColumnsByTable[tableName] : new InformationSchema.ConstraintColumnUsage[] { };
                        var tableIndexedColumns = indexedColumnsByTable.ContainsKey(tableName) ? indexedColumnsByTable[tableName] : new InformationSchema.IndexColumnUsage[] { };
                        var table = new TableAttribute(tableColumns, tableConstraints, tableKeyColumns, tableConstraintColumns, tableIndexedColumns, dal);
                        this.Tables.Add(tableName, table);
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
                                        table.PrimaryKey = new PrimaryKey(constraint, uniqueConstraint, uniqueConstraintColumns, uniqueTableColumns, dal);
                                        this.PrimaryKeys.Add(constraintName, table.PrimaryKey);
                                    }
                                }
                            }
                        }
                    }
                }

                var xxx = dal.GetRoutines()
                    .Where(r => filter == null || !filter.IsMatch(r.routine_name))
                    .ToList();
                var routines = new Dictionary<string, InformationSchema.Routines>();
                var exists = new List<InformationSchema.Routines>();
                foreach (var prm in xxx)
                {
                    var ident = dal.MakeIdentifier(prm.specific_schema, prm.specific_name);
                    if (routines.ContainsKey(ident))
                    {
                        exists.Add(prm);
                    }
                    else
                    {
                        routines.Add(ident, prm);
                    }
                }
                var parameters = dal.GetParameters()
                    .GroupBy(prm => dal.MakeIdentifier(prm.specific_schema, prm.specific_name))
                    .ToDictionary(g => g.Key, g => g.ToArray());
                foreach (var key in routines.Keys)
                {
                    var routine = routines[key];
                    var routineParameters = parameters.ContainsKey(key) ? parameters[key] : null;
                    var function = new RoutineAttribute(routine, routineParameters, dal);
                    var ident = dal.MakeRoutineIdentifier(function);
                    if (!this.Functions.ContainsKey(ident))
                    {
                        this.Functions.Add(ident, function);
                    }
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
                this.CatalogueExists = false;
            }
        }

        public virtual DatabaseDelta Diff(DatabaseState initial, IAssemblyStateReader asm, IDatabaseScriptGenerator gen)
        {
            return new DatabaseDelta(this, initial, asm, gen);
        }

        public bool TypeExists<T>()
        {
            var type = typeof(T);
            return this.Functions.Values.Any(f => f.SystemType == type || f.Parameters.Any(p => p.SystemType == type))
                || this.Tables.Values.SelectMany(t => t.Properties).Any(p => p.SystemType == type);
        }
    }
}
