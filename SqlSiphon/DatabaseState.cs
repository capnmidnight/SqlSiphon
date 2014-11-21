using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using SqlSiphon.Mapping;

namespace SqlSiphon
{
    public class DatabaseState
    {
        public Dictionary<string, MappedClassAttribute> Tables { get; private set; }
        public Dictionary<string, MappedMethodAttribute> Functions { get; private set; }
        public Dictionary<string, Relationship> Relationships { get; private set; }

        private DatabaseState()
        {
            this.Tables = new Dictionary<string, MappedClassAttribute>();
            this.Functions = new Dictionary<string, MappedMethodAttribute>();
            this.Relationships = new Dictionary<string, Relationship>();
        }

        /// <summary>
        /// Scans an assembly object for types and methods that define
        /// mappings to database objects.
        /// </summary>
        /// <param name="asm"></param>
        public DatabaseState(Assembly asm, ISqlSiphon dal)
            : this()
        {
            var types = asm.GetTypes();
            foreach (var type in types)
            {
                AddType(type, dal);
            }
        }

        public void AddType(Type type, ISqlSiphon dal)
        {
            var table = MappedObjectAttribute.GetAttribute<MappedClassAttribute>(type);
            if (table != null)
            {
                table.InferProperties(type);
                table.Schema = table.Schema ?? dal.DefaultSchemaName;
                if (table.Include)
                {
                    this.Tables.Add(dal.MakeIdentifier(table.Schema, table.Name), table);
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
                            this.Functions.Add(dal.MakeIdentifier(function.Schema, function.Name), function);
                        }
                    }
                }

                var rt = typeof(Relationship);
                var props = type.GetProperties(BindingFlags.Static);
                foreach (var prop in props)
                {
                    if (prop.PropertyType == rt)
                    {
                        var r = (Relationship)prop.GetValue(null, null);
                        r.Schema = r.Schema ?? dal.DefaultSchemaName;
                        this.Relationships.Add(dal.MakeIdentifier(r.Schema, r.Name), r);
                    }
                }
            }
        }

        /// <summary>
        /// Scans a database for tables and stored procedures
        /// </summary>
        /// <param name="dal"></param>
        public DatabaseState(ISqlSiphon dal)
            : this()
        {
            try
            {
                var columns = dal.GetColumns()
                    .GroupBy(col => dal.MakeIdentifier(col.table_schema, col.table_name))
                    .ToDictionary(g => g.Key, g => g.ToArray());
                var constraints = dal.GetTableConstraints();
                var constraintsByTable = constraints.GroupBy(cst => dal.MakeIdentifier(cst.table_schema, cst.table_name))
                    .ToDictionary(g => g.Key, g => g.ToArray());
                var constraintsByName = constraints.ToDictionary(cst => dal.MakeIdentifier(cst.constraint_schema, cst.constraint_name));
                var constraintsColumns = dal.GetColumnConstraints();
                var constraintsColumnsByTable = constraintsColumns.GroupBy(col => dal.MakeIdentifier(col.table_schema, col.table_name))
                    .ToDictionary(g => g.Key, g => g.ToArray());
                var constraintsColumnsByName = constraintsColumns.GroupBy(col => dal.MakeIdentifier(col.constraint_schema, col.constraint_name))
                    .ToDictionary(g => g.Key, g => g.ToArray());
                var xref = dal.GetReferentialConstraints()
                    .GroupBy(col => dal.MakeIdentifier(col.constraint_schema, col.constraint_name))
                    .ToDictionary(g => g.Key, g => g.Select(q => dal.MakeIdentifier(q.unique_constraint_schema, q.unique_constraint_name)).First());
                foreach (var tableName in columns.Keys)
                {
                    var tableColumns = columns[tableName];
                    var tableConstraints = constraintsByTable.ContainsKey(tableName) ? constraintsByTable[tableName] : new InformationSchema.TableConstraints[] { };
                    var tableConstraintColumns = constraintsColumnsByTable.ContainsKey(tableName) ? constraintsColumnsByTable[tableName] : new InformationSchema.ConstraintColumnUsage[] { };
                    this.Tables.Add(tableName, new MappedClassAttribute(tableColumns, tableConstraints, tableConstraintColumns, dal));
                    if (tableConstraints != null)
                    {
                        foreach (var constraint in tableConstraints)
                        {
                            if (constraint.constraint_type == "FOREIGN KEY")
                            {
                                var constraintName = dal.MakeIdentifier(constraint.constraint_schema, constraint.constraint_name);
                                var constraintColumns = constraintsColumnsByName[constraintName];
                                var uniqueConstraintName = xref[constraintName];
                                var uniqueConstraint = constraintsByName[uniqueConstraintName];
                                var uniqueConstraintColumns = constraintsColumnsByName[uniqueConstraintName];
                                var uniqueTableColumns = columns[dal.MakeIdentifier(uniqueConstraint.table_schema, uniqueConstraint.table_name)];
                                this.Relationships.Add(constraintName, new Relationship(
                                    constraint, constraintColumns, tableColumns,
                                    uniqueConstraint, uniqueConstraintColumns, uniqueTableColumns,
                                    dal));
                            }
                        }
                    }
                }

                var routines = dal.GetRoutines()
                    .ToDictionary(prm => dal.MakeIdentifier(prm.routine_schema, prm.routine_name));
                var parameters = dal.GetParameters()
                    .GroupBy(prm => dal.MakeIdentifier(prm.specific_schema, prm.specific_name))
                    .ToDictionary(g => g.Key, g => g.ToArray());
                foreach (var routineName in routines.Keys)
                {
                    var routine = routines[routineName];
                    var routineParameters = parameters.ContainsKey(routineName) ? parameters[routineName] : null;
                    this.Functions.Add(routineName, new MappedMethodAttribute(routine, routineParameters, dal));
                }
            }
            catch (ConnectionFailedException)
            {
                // just ignore it, it usually means the catalog hasn't been created yet.
            }
        }

        public DatabaseDelta Diff(DatabaseState initial, ISqlSiphon dal)
        {
            return new DatabaseDelta(this, initial, dal);
        }
    }
}
