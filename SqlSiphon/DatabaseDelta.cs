using System;
using System.Collections.Generic;
using System.Linq;

using SqlSiphon.Mapping;
using SqlSiphon.Model;

namespace SqlSiphon
{
    public class DatabaseDelta
    {
        public static void Traverse<T>(
            Dictionary<string, T> final,
            Dictionary<string, T> initial,
            Action<string, T> remove,
            Action<string, T> add,
            Action<string, T, T> change,
            bool makeChangeScripts)
        {
            if (initial != null)
            {
                var all = initial.Keys
                    .Union(final.Keys)
                    .Distinct()
                    .ToArray();
                foreach (var key in all)
                {
                    if (final.ContainsKey(key) && initial.ContainsKey(key))
                    {
                        if (makeChangeScripts)
                        {
                            change(key, final[key], initial[key]);
                        }
                    }
                    else if (final.ContainsKey(key))
                    {
                        add(key, final[key]);
                    }
                    else
                    {
                        remove(key, initial[key]);
                    }
                }
            }
        }

        public static void DumpAll<T>(List<ScriptStatus> outCollect, Dictionary<string, T> inCollect, ScriptType type, Func<T, string> makeScript)
        {
            if (inCollect != null)
            {
                outCollect.AddRange(inCollect.Select(i => new ScriptStatus(type, i.Key, makeScript(i.Value), null)));
            }
        }

        internal void Traverse<T>(
            string name,
            Dictionary<string, T> final,
            Dictionary<string, T> initial,
            ScriptType dropType,
            ScriptType createType,
            Func<T, T, string> isChanged,
            Func<T, string> makeDropScript,
            Func<T, string> makeCreateScript,
            Func<T, string> altMakeCreateScript,
            bool makeChangeScripts)
        {
            DumpAll(Initial, initial, createType, altMakeCreateScript ?? makeCreateScript);
            DumpAll(Final, final, createType, makeCreateScript);
            Traverse(
                final,
                initial,
                (key, i) =>
                {
                    if (makeDropScript != null)
                    {
                        var script = makeDropScript(i);
                        if (script != null && dropType != ScriptType.None)
                        {
                            Scripts.Add(new ScriptStatus(dropType, key, script, name + " no longer exists"));
                        }
                    }
                },
                (key, f) => Scripts.Add(new ScriptStatus(createType, key, makeCreateScript(f), name + " does not exist")),
                (key, f, i) =>
                {
                    var changeReason = isChanged(f, i);
                    if (changeReason != null)
                    {
                        changeReason = $"{name} has changed, {changeReason}";
                        Scripts.Add(new ScriptStatus(dropType, key, makeDropScript(i), changeReason));
                        Scripts.Add(new ScriptStatus(createType, key, makeCreateScript(f), changeReason));
                    }
                },
                makeChangeScripts);
        }

        public List<ScriptStatus> Scripts { get; private set; }
        public List<ScriptStatus> Initial { get; private set; }
        public List<ScriptStatus> Final { get; private set; }
        public List<Action<IDataConnector>> PostExecute { get; private set; }

        public DatabaseDelta(DatabaseState final, DatabaseState initial, IAssemblyStateReader asm, IDatabaseScriptGenerator gen, bool makeChangeScripts)
        {
            Scripts = new List<ScriptStatus>();
            Initial = new List<ScriptStatus>();
            Final = new List<ScriptStatus>();

            if (initial?.CatalogueExists == false)
            {
                Scripts.Add(new ScriptStatus(ScriptType.CreateCatalogue, final.CatalogueName, gen.MakeCreateCatalogueScript(final.CatalogueName), "Database doesn't exist"));
            }

            ProcessDatabaseLogins(final.DatabaseLogins, initial?.DatabaseLogins?.Keys?.ToList(), final.CatalogueName, asm, gen);
            ProcessSchemas(final.Schemata.ToDictionary(k => k), initial?.Schemata?.ToDictionary(k => k), asm, gen, makeChangeScripts);
            ProcessTables(final.Tables, initial?.Tables, asm, gen, makeChangeScripts);
            ProcessIndexes(final.Indexes, initial?.Indexes, asm, gen, makeChangeScripts);
            ProcessRelationships(final.Relationships, initial?.Relationships, asm, gen, makeChangeScripts);
            ProcessPrimaryKeys(final.PrimaryKeys, initial?.PrimaryKeys, asm, gen, makeChangeScripts);
            ProcessRoutines(final.Functions, initial?.Functions, asm, gen, makeChangeScripts);
            Scripts.AddRange(final.InitScripts
                .Where(s => initial?.InitScripts?.Contains(s) ?? true)
                .Select(s => new ScriptStatus(ScriptType.InitializeData, "init", s, "Initialize data in database")));
            Scripts.Sort();
            Initial.Sort();
            Final.Sort();
            PostExecute = final.PostExecute;
        }

        private void ProcessDatabaseLogins(Dictionary<string, string> final, List<string> initial, string databaseName, IAssemblyStateReader asm, IDatabaseScriptGenerator gen)
        {
            var names = initial?.Select(s => s.ToLowerInvariant())?.ToArray();
            Scripts.AddRange(final
                .Where(u => !names?.Contains(u.Key.ToLowerInvariant()) ?? true)
                .Select(u => new ScriptStatus(ScriptType.CreateDatabaseLogin, u.Key, gen.MakeCreateDatabaseLoginScript(u.Key, u.Value, databaseName), "Database login doesn't exist")));
        }

        private void ProcessSchemas(Dictionary<string, string> finalSchemas, Dictionary<string, string> initialSchemas, IAssemblyStateReader asm, IDatabaseScriptGenerator gen, bool makeChangeScripts)
        {
            Traverse(
                "Schema",
                finalSchemas,
                initialSchemas,
                ScriptType.DropSchema,
                ScriptType.CreateSchema,
                (a, b) => null,
                gen.MakeDropSchemaScript,
                gen.MakeCreateSchemaScript,
                null,
                makeChangeScripts);
        }

        private void ProcessRoutines(Dictionary<string, RoutineAttribute> finalRoutines, Dictionary<string, RoutineAttribute> initialRoutines, IAssemblyStateReader asm, IDatabaseScriptGenerator gen, bool makeChangeScripts)
        {
            Traverse(
                "Routine",
                finalRoutines,
                initialRoutines,
                ScriptType.DropRoutine,
                ScriptType.CreateRoutine,
                asm.RoutineChanged,
                gen.MakeDropRoutineScript,
                f => gen.MakeCreateRoutineScript(f, true),
                f => gen.MakeCreateRoutineScript(f, false),
                makeChangeScripts);
        }

        private void ProcessPrimaryKeys(Dictionary<string, PrimaryKey> finalKeys, Dictionary<string, PrimaryKey> initialKeys, IAssemblyStateReader asm, IDatabaseScriptGenerator gen, bool makeChangeScripts)
        {
            Traverse(
                "Primary key",
                finalKeys,
                initialKeys,
                ScriptType.DropPrimaryKey,
                ScriptType.CreatePrimaryKey,
                asm.KeyChanged,
                gen.MakeDropPrimaryKeyScript,
                gen.MakeCreatePrimaryKeyScript,
                null,
                makeChangeScripts);
        }

        private void ProcessRelationships(Dictionary<string, Relationship> finalRelations, Dictionary<string, Relationship> initialRelations, IAssemblyStateReader asm, IDatabaseScriptGenerator gen, bool makeChangeScripts)
        {
            Traverse(
                "Relationship",
                finalRelations,
                initialRelations,
                ScriptType.DropRelationship,
                ScriptType.CreateRelationship,
                asm.RelationshipChanged,
                gen.MakeDropRelationshipScript,
                gen.MakeCreateRelationshipScript,
                null,
                makeChangeScripts);
        }

        private void ProcessIndexes(Dictionary<string, TableIndex> finalIndexes, Dictionary<string, TableIndex> initialIndexes, IAssemblyStateReader asm, IDatabaseScriptGenerator gen, bool makeChangeScripts)
        {
            Traverse(
                "Index",
                finalIndexes,
                initialIndexes,
                ScriptType.DropIndex,
                ScriptType.CreateIndex,
                asm.IndexChanged,
                gen.MakeDropIndexScript,
                gen.MakeCreateIndexScript,
                null,
                makeChangeScripts);
        }

        private void ProcessTables(Dictionary<string, TableAttribute> finalTables, Dictionary<string, TableAttribute> initialTables, IAssemblyStateReader asm, IDatabaseScriptGenerator gen, bool makeChangeScripts)
        {
            DumpAll(Initial, initialTables, ScriptType.CreateTable, gen.MakeCreateTableScript);
            DumpAll(Final, finalTables, ScriptType.CreateTable, gen.MakeCreateTableScript);

            Traverse(
                finalTables,
                initialTables,
                (tableName, initialTable) => Scripts.Add(new ScriptStatus(ScriptType.DropTable, tableName, gen.MakeDropTableScript(initialTable), "Table no longer exists")),
                (tableName, finalTable) => Scripts.Add(new ScriptStatus(ScriptType.CreateTable, tableName, gen.MakeCreateTableScript(finalTable), "Table does not exist")),
                (tableName, finalTable, initialTable) =>
                {
                    var finalColumns = finalTable.Properties.ToDictionary(p => gen.MakeIdentifier(finalTable.Schema ?? gen.DefaultSchemaName, finalTable.Name, p.Name).ToLowerInvariant());
                    var initialColumns = initialTable?.Properties?.ToDictionary(p => gen.MakeIdentifier(initialTable.Schema ?? gen.DefaultSchemaName, initialTable.Name, p.Name).ToLowerInvariant());

                    Traverse(
                        finalColumns,
                        initialColumns,
                        (columnName, initialColumn) => Scripts.Add(new ScriptStatus(ScriptType.DropColumn, columnName, gen.MakeDropColumnScript(initialColumn), "Column no longer exists")),
                        (columnName, finalColumn) => Scripts.Add(new ScriptStatus(ScriptType.CreateColumn, columnName, gen.MakeCreateColumnScript(finalColumn), "Column doesn't exist")),
                        (columnName, finalColumn, initialColumn) =>
                        {
                            var reason = asm.ColumnChanged(finalColumn, initialColumn);
                            if (reason != null)
                            {
                                Scripts.Add(new ScriptStatus(ScriptType.AlterColumn, columnName, gen.MakeAlterColumnScript(finalColumn, initialColumn), reason));
                            }
                        }, makeChangeScripts);
                }, makeChangeScripts);
        }
    }
}
