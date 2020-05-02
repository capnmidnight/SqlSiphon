using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using SqlSiphon.Mapping;
using SqlSiphon.Model;

namespace SqlSiphon
{
    public class DatabaseState
    {
        public bool? CatalogueExists { get; private set; }
        public string CatalogueName { get; private set; }
        public Dictionary<string, TableAttribute> Tables { get; private set; }
        public Dictionary<string, TableIndex> Indexes { get; private set; }
        public Dictionary<string, RoutineAttribute> Functions { get; private set; }
        public Dictionary<string, Relationship> Relationships { get; private set; }
        public Dictionary<string, PrimaryKey> PrimaryKeys { get; private set; }
        public List<string> Schemata { get; private set; }
        public Dictionary<string, string> DatabaseLogins { get; private set; }
        public List<string> InitScripts { get; private set; }
        public List<Action<IDataConnector>> PostExecute { get; private set; }

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
        private DatabaseState(Dictionary<string, TableAttribute> tables, Dictionary<string, TableIndex> indexes,
            Dictionary<string, RoutineAttribute> routines, Dictionary<string, Relationship> relationships,
            Dictionary<string, PrimaryKey> pks, List<string> schemata, Dictionary<string, string> logins,
            List<string> initScripts, List<Action<IDataConnector>> postExecute, string catalogueName, bool? catalogueExists)
        {
            Tables = tables;
            Indexes = indexes;
            Functions = routines;
            Relationships = relationships;
            PrimaryKeys = pks;
            Schemata = schemata;
            DatabaseLogins = logins;
            InitScripts = initScripts;
            PostExecute = postExecute;
            CatalogueExists = catalogueExists;
            CatalogueName = catalogueName;
        }

        private DatabaseState()
            : this(new Dictionary<string, TableAttribute>(), new Dictionary<string, TableIndex>(), new Dictionary<string, RoutineAttribute>(),
            new Dictionary<string, Relationship>(), new Dictionary<string, PrimaryKey>(), new List<string>(), new Dictionary<string, string>(),
            new List<string>(), null, null, null)
        {
        }

        public DatabaseState(DatabaseState copy)
            : this(copy.Tables, copy.Indexes, copy.Functions, copy.Relationships, copy.PrimaryKeys, copy.Schemata, copy.DatabaseLogins, copy.InitScripts, copy.PostExecute, copy.CatalogueName, copy.CatalogueExists)
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
            if (types is null)
            {
                throw new ArgumentNullException(nameof(types));
            }

            if (asm is null)
            {
                throw new ArgumentNullException(nameof(asm));
            }

            if (dal is null)
            {
                throw new ArgumentNullException(nameof(dal));
            }

            PostExecute = new List<Action<IDataConnector>>();
            if (!string.IsNullOrWhiteSpace(userName))
            {
                DatabaseLogins.Add(userName.ToLowerInvariant(), password);
            }

            var routineDefTypes = new List<Type>();

            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();
                if (interfaces.Contains(typeof(IDataConnector)))
                {
                    routineDefTypes.Add(type);
                }

                FindTables(type, dal);

                var publicStatic = BindingFlags.Public | BindingFlags.Static;

                var methods = type.GetMethods(publicStatic);
                foreach (var method in methods)
                {
                    if (method.ReturnType == typeof(void))
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType.GetInterfaces().Contains(typeof(IDataConnector)))
                        {
                            PostExecute.Add((db) => method.Invoke(null, new object[] { db }));
                        }
                    }
                }
            }

            CreateRelationships(dal);

            CreateRoutines(routineDefTypes, dal);

            Schemata.AddRange(GetSchemata(dal));
        }

        /// <summary>
        /// Scans a database for tables and stored procedures
        /// </summary>
        /// <param name="dal"></param>
        public DatabaseState(string catalogueName, Regex filter, ISqlSiphon dal)
            : this()
        {
            if (dal != null)
            {
                var hasScriptStatusTable = false;
                var scriptStatusName = dal.MakeIdentifier(dal.DefaultSchemaName, "ScriptStatus");
                CatalogueName = catalogueName;
                try
                {
                    foreach (var name in dal.GetDatabaseLogins())
                    {
                        DatabaseLogins.Add(name.ToLowerInvariant(), null);
                    }
                    CatalogueExists = true;
                    var schemas = dal.GetSchemata();
                    if (schemas.Count > 0)
                    {
                        Schemata.AddRange(schemas);
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
                        if (tableName == scriptStatusName)
                        {
                            hasScriptStatusTable = true;
                        }
                        var tableColumns = columns[tableName];
                        if (filter == null || !filter.IsMatch(tableColumns[0].table_name))
                        {
                            var tableConstraints = constraintsByTable.ContainsKey(tableName) ? constraintsByTable[tableName] : Array.Empty<InformationSchema.TableConstraints>();
                            var tableKeyColumns = keyColumnsByTable.ContainsKey(tableName) ? keyColumnsByTable[tableName] : Array.Empty<InformationSchema.KeyColumnUsage>();
                            var tableConstraintColumns = constraintsColumnsByTable.ContainsKey(tableName) ? constraintsColumnsByTable[tableName] : Array.Empty<InformationSchema.ConstraintColumnUsage>();
                            var tableIndexedColumns = indexedColumnsByTable.ContainsKey(tableName) ? indexedColumnsByTable[tableName] : Array.Empty<InformationSchema.IndexColumnUsage>();
                            var table = new TableAttribute(tableColumns, tableConstraints, tableKeyColumns, tableConstraintColumns, tableIndexedColumns, dal);
                            Tables.Add(tableName.ToLowerInvariant(), table);
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
                                        var uniqueTableName = dal.MakeIdentifier(uniqueConstraint.table_schema, uniqueConstraint.table_name);
                                        var uniqueTableColumns = columns[uniqueTableName];
                                        var uniqueTableKeyColumns = keyColumnsByTable[uniqueTableName];
                                        if (constraint.constraint_type == "FOREIGN KEY")
                                        {
                                            Relationships.Add(
                                                constraintName.ToLowerInvariant(),
                                                new Relationship(
                                                    tableColumns,
                                                    constraint,
                                                    constraintColumns,
                                                    uniqueTableColumns,
                                                    uniqueConstraint,
                                                    uniqueConstraintColumns,
                                                    uniqueTableKeyColumns,
                                                    dal));
                                        }
                                        else if (constraint.constraint_type == "PRIMARY KEY")
                                        {
                                            table.PrimaryKey = new PrimaryKey(
                                                constraint,
                                                uniqueConstraint,
                                                uniqueConstraintColumns,
                                                uniqueTableColumns,
                                                dal);
                                            PrimaryKeys.Add(constraintName.ToLowerInvariant(), table.PrimaryKey);
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
                    foreach (var prm in xxx)
                    {
                        var ident = dal.MakeIdentifier(prm.specific_schema, prm.specific_name);
                        if (!routines.ContainsKey(ident))
                        {
                            routines.Add(ident.ToLowerInvariant(), prm);
                        }
                    }
                    var parameters = dal.GetParameters()
                        .GroupBy(prm => dal.MakeIdentifier(prm.specific_schema, prm.specific_name).ToLowerInvariant())
                        .ToDictionary(g => g.Key, g => g.ToArray());
                    foreach (var key in routines.Keys)
                    {
                        var routine = routines[key];
                        var routineParameters = parameters.ContainsKey(key) ? parameters[key] : null;
                        var function = new RoutineAttribute(routine, routineParameters, dal);
                        var ident = dal.MakeRoutineIdentifier(function);
                        if (!Functions.ContainsKey(ident))
                        {
                            Functions.Add(ident.ToLowerInvariant(), function);
                        }
                    }

                    foreach (var idxName in indexedColumnsByName.Keys)
                    {
                        var idxKey = dal.MakeIdentifier(dal.DefaultSchemaName, idxName).ToLowerInvariant();
                        var idx = indexedColumnsByName[idxName];
                        var tableName = dal.MakeIdentifier(idx[0].table_schema, idx[0].table_name).ToLowerInvariant();
                        if (Tables.ContainsKey(tableName))
                        {
                            var table = Tables[tableName];
                            Indexes.Add(idxKey, new TableIndex(table, idxName));
                            foreach (var idxCol in idx)
                            {
                                Indexes[idxKey].Columns.Add(idxCol.column_name);
                            }
                        }
                    }

                    if (hasScriptStatusTable)
                    {
                        var scripts = dal.GetScriptStatus();
                        if (scripts.Count > 0)
                        {
                            InitScripts.AddRange(scripts);
                        }
                    }
                }
                catch (ConnectionFailedException)
                {
                    CatalogueExists = false;
                }
            }
        }

        private List<string> GetSchemata(IDatabaseScriptGenerator dal)
        {
            var schemata = Tables.Values.Select(t => t.Schema)
                .Union(Functions.Values.Select(f => f.Schema))
                .Union(Relationships.Values.Select(r => r.Schema))
                .Union(PrimaryKeys.Values.Select(r => r.Schema))
                .Where(s => !string.IsNullOrWhiteSpace(s) && !Schemata.Contains(s) && s != dal.DefaultSchemaName)
                .Distinct()
                .ToList();
            return schemata;
        }

        private void CreateRoutines(List<Type> routineDefTypes, IDatabaseScriptGenerator dal)
        {
            foreach (var type in routineDefTypes)
            {
                var methods = type.GetMethods();
                foreach (var method in methods)
                {
                    var function = RoutineAttribute.GetCommandDescription(method);
                    if (function != null && function.CommandType == System.Data.CommandType.StoredProcedure)
                    {
                        var functionName = dal.MakeRoutineIdentifier(function);
                        if (Functions.ContainsKey(functionName))
                        {

                        }
                        else
                        {
                            Functions.Add(functionName.ToLowerInvariant(), function);
                        }
                    }
                }
            }
        }

        private void CreateRelationships(IDatabaseScriptGenerator dal)
        {
            var tablesBySystemType = Tables.Values.ToDictionary(t => t.SystemType);
            foreach (var table in Tables.Values)
            {
                var relationships = table.GetRelationships();
                foreach (var relationship in relationships)
                {
                    relationship.ResolveColumns(tablesBySystemType, dal);
                    var id = dal.MakeIdentifier(relationship.Schema ?? dal.DefaultSchemaName, relationship.Name);
                    if (Relationships.ContainsKey(id))
                    {
                        throw new RelationshipExistsException(id);
                    }
                    else
                    {
                        Relationships.Add(id.ToLowerInvariant(), relationship);
                        if (relationship.AutoCreateIndex)
                        {
                            var fkIndex = new TableIndex(relationship.From, "IDX_" + relationship.Name);
                            fkIndex.Columns.AddRange(relationship.FromColumns.Select(c => c.Name));
                            var fkIndexNameKey = dal.MakeIdentifier(relationship.From.Schema ?? dal.DefaultSchemaName, fkIndex.Name).ToLowerInvariant();
                            Indexes.Add(fkIndexNameKey, fkIndex);
                        }
                    }
                }
            }
        }

        private void FindTables(Type type, IDatabaseScriptGenerator dal)
        {
            var queue = new Queue<Type>();
            queue.Enqueue(type);
            type.Assembly.GetTypes().Distinct().ToList().ForEach(queue.Enqueue);
            while (queue.Count > 0)
            {
                type = queue.Dequeue();
                var table = DatabaseObjectAttribute.GetAttribute(type);
                if (table != null)
                {
                    AddTable(Tables, type, dal, table);
                }
            }
        }

        public void AddTable(Dictionary<string, TableAttribute> tableCollection, Type type, IDatabaseScriptGenerator dal, TableAttribute table)
        {
            if (tableCollection is null)
            {
                throw new ArgumentNullException(nameof(tableCollection));
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (dal is null)
            {
                throw new ArgumentNullException(nameof(dal));
            }

            if (table is null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            table.Schema = table.Schema ?? dal.DefaultSchemaName;
            if (table.Include)
            {
                var tableKey = dal.MakeIdentifier(table.Schema ?? dal.DefaultSchemaName, table.Name).ToLowerInvariant();
                if (!tableCollection.ContainsKey(tableKey))
                {
                    tableCollection.Add(tableKey, table);
                    if (type != null && table.Properties.Any(p => p.IncludeInPrimaryKey))
                    {
                        var pkNameKey = dal.MakeIdentifier(table.PrimaryKey.Schema ?? dal.DefaultSchemaName, table.PrimaryKey.Name).ToLowerInvariant();
                        PrimaryKeys.Add(pkNameKey, table.PrimaryKey);
                        Indexes.Add(pkNameKey, table.PrimaryKey.ToIndex());
                    }

                    foreach (var index in table.Indexes)
                    {
                        if (Indexes.ContainsKey(index.Key))
                        {
                            throw new IndexExistsException(index.Value.Name, index.Value.Table.Name, Indexes[index.Key].Table.Name);
                        }
                        else
                        {
                            var idxNameKey = dal.MakeIdentifier(table.PrimaryKey?.Schema ?? dal.DefaultSchemaName, index.Key).ToLowerInvariant();
                            if (!Indexes.ContainsKey(idxNameKey))
                            {
                                Indexes.Add(idxNameKey, index.Value);
                            }
                        }
                    }

                    InitScripts.AddRange(table.EnumValues.Select(val => dal.MakeInsertScript(table, val)));
                }
            }
        }


        public virtual DatabaseDelta Diff(DatabaseState initial, IAssemblyStateReader assembly, IDatabaseScriptGenerator gen)
        {
            return new DatabaseDelta(this, initial, assembly, gen, true);
        }

        public virtual DatabaseDelta MakeScripts(DatabaseState initial, IAssemblyStateReader assembly, IDatabaseScriptGenerator gen)
        {
            return new DatabaseDelta(this, initial, assembly, gen, false);
        }

        public bool TypeExists<T>()
        {
            var type = typeof(T);
            return Functions.Values.Any(f => f.SystemType == type || f.Parameters.Any(p => p.SystemType == type))
                || Tables.Values.SelectMany(t => t.Properties).Any(p => p.SystemType == type);
        }

        private static readonly Dictionary<Type, string> TYPE_NAMES = new Dictionary<Type, string>();
        private static void AddType<T>(string name) { TYPE_NAMES.Add(typeof(T), name); }
        static DatabaseState()
        {
            AddType<short>("short");
            AddType<ushort>("ushort");
            AddType<int>("int");
            AddType<uint>("uint");
            AddType<long>("long");
            AddType<ulong>("ulong");
            AddType<char>("char");
            AddType<byte>("byte");
            AddType<sbyte>("sbyte");
            AddType<bool>("bool");
            AddType<float>("float");
            AddType<double>("double");
            AddType<decimal>("decimal");
            AddType<string>("string");
        }

        private static string TypeName(Type t)
        {
            if (t != null)
            {
                if (TYPE_NAMES.ContainsKey(t))
                {
                    return TYPE_NAMES[t];
                }
                else
                {
                    return t.Name;
                }
            }
            else
            {
                return null;
            }
        }

        public void WriteCodeFiles(string directory, string nameSpace, string name)
        {
            if (Directory.Exists(directory))
            {
                foreach (var table in Tables.Values)
                {
                    var columnStrings = new List<string>();
                    var indexes = Indexes.Values.Where(i => i.Table.Schema == table.Schema && i.Table.Name == table.Name);
                    foreach (var column in table.Properties)
                    {
                        if (column.Include)
                        {
                            var columnAttrString = "";
                            var hasOptions = column.DefaultValue != null
                                || column.IsPrecisionSet
                                || column.IsSizeSet
                                || (column.IsOptional
                                    && !column.SystemType.IsValueType);
                            if (column.IncludeInPrimaryKey)
                            {
                                if (column.IsIdentity)
                                {
                                    columnAttrString = @"
        [AutoPK";
                                }
                                else
                                {
                                    columnAttrString = @"
        [PK";
                                }
                            }
                            else if (hasOptions)
                            {
                                columnAttrString = "[Column";
                            }

                            if (hasOptions)
                            {
                                columnAttrString += "(";
                                var sep = "";
                                if (column.DefaultValue != null)
                                {
                                    columnAttrString += $@"{sep}DefaultValue = ""{sep}""";
                                    sep = ", ";
                                }
                                if (column.IsSizeSet)
                                {
                                    columnAttrString += $@"{sep}Size = {column.Size}";
                                    sep = ", ";
                                }
                                if (column.IsPrecisionSet)
                                {
                                    columnAttrString += $@"{sep}Precision = {column.Precision}";
                                    sep = ", ";
                                }
                                if (column.IsOptional && !column.SystemType.IsValueType)
                                {
                                    var condition = column.IsOptional ? "true" : "false";
                                    columnAttrString += $@"{sep}IsOptional = {condition}";
                                    sep = ", ";
                                }
                                columnAttrString += ")";
                            }

                            if (columnAttrString.Length > 0)
                            {
                                columnAttrString += "]";
                            }

                            var indexString = string.Join("", indexes
                                .Where(i => i.Columns.Any(c => c == column.Name))
                                .Select(i => $@"
        [IncludeInIndex(""{i.Name}"")]"));

                            var typeName = TypeName(column.SystemType);

                            var nullableFlag = column.IsOptional && column.SystemType.IsValueType ? "?" : "";
                            columnStrings.Add($@"
        {indexString}{columnAttrString}
        public {typeName}{nullableFlag} {column.Name} {{ get; set; }}
");
                        }
                    }

                    var fkAttrString = "";
                    var fks = Relationships.Values.Where(r => r.From.Schema == table.Schema && r.From.Name == table.Name);
                    foreach (var fk in fks)
                    {
                        fkAttrString += @"
    [FK(";
                        var prefix = fk.FromColumns.SelectMany(c =>
                            (from c2 in table.Properties
                             where c.Name.EndsWith(c2.Name, StringComparison.InvariantCulture)
                             select c.Name.Substring(0, c.Name.IndexOf(c2.Name, StringComparison.InvariantCulture))))
                            .FirstOrDefault();
                        if (prefix != null && prefix.Length > 0)
                        {
                            fkAttrString += $@"Prefix = ""{prefix}"", ";
                        }

                        fkAttrString += $"typeof({fk.To.Name}))]";
                    }
                    var fieldDefs = string.Join("", columnStrings);
                    var codeFile = $@"using System;
using SqlSiphon.Mapping;

namespace {nameSpace}
{{
    [Table]{fkAttrString}
    public class {table.Name}
    {{
        {fieldDefs}
    }}
}}";
                    File.WriteAllText(Path.Combine(directory, table.Name + ".cs"), codeFile);
                }
                var routineSectionStr = string.Join("", Functions.Values.Select(f =>
                {
                    var isCollection = DataConnector.IsTypeCollection(f.SystemType);
                    var type = DataConnector.CoalesceCollectionType(f.SystemType);
                    var retTypeStr = TypeName(type) ?? "void";
                    if (isCollection)
                    {
                        retTypeStr = $"List<{retTypeStr}>";
                    }
                    var paramSection = "";
                    var retKey = retTypeStr == "void" ? "" : "return ";
                    var collectionT = isCollection ? "List" : "";
                    var command = retTypeStr == "void" ? "Execute" : $"Get{collectionT}<{retTypeStr}>";
                    var callSection = "";
                    return $@"

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query =
@""{f.Query}"")]
        public {retTypeStr} {f.Name}({paramSection})
        {{
            {retKey}this.{command}({callSection});
        }}";
                }));
                var routineFile = $@"using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using SqlSiphon.Mapping;


namespace {nameSpace}
{{
    public class {name} : DataConnector
    {{
        public {name}(IDataConnectorFactory factory, string server, string database, string userName, string password) :
            base(factory, server, database, userName, password)
        {{
        }}

        public static void FirstTimeSetup({name} db)
        {{
        }}

        {routineSectionStr}
    }}
}}";
                File.WriteAllText(Path.Combine(directory, name + ".cs"), routineFile);
            }
        }
    }
}
