using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;
namespace SqlSiphon
{
    public class DatabaseDelta
    {
        private static void Traverse<T>(
            Dictionary<string, T> final,
            Dictionary<string, T> initial,
            Action<string, T> remove,
            Action<string, T> add,
            Action<string, T, T> change)
        {
            var all = initial.Keys
                .Union(final.Keys)
                .Distinct()
                .ToArray();
            foreach (var key in all)
            {
                if (final.ContainsKey(key) && initial.ContainsKey(key))
                {
                    change(key, final[key], initial[key]);
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

        private static void DumpAll<T>(List<ScriptStatus> outCollect, Dictionary<string, T> inCollect, ScriptType type, Func<T, string> makeScript)
        {
            outCollect.AddRange(inCollect.Select(i => new ScriptStatus(type, i.Key, makeScript(i.Value))));
        }

        internal void Traverse<T>(
            Dictionary<string, T> final,
            Dictionary<string, T> initial,
            ScriptType dropType,
            ScriptType createType,
            Func<T, T, bool> isChanged,
            Func<T, string> makeDropScript,
            Func<T, string> makeCreateScript)
        {
            DumpAll(this.Initial, initial, createType, makeCreateScript);
            DumpAll(this.Final, final, createType, makeCreateScript);
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
                            this.Scripts.Add(new ScriptStatus(dropType, key, script));
                        }
                    }
                },
                (key, f) =>
                {
                    this.Scripts.Add(new ScriptStatus(createType, key, makeCreateScript(f)));
                },
                (key, f, i) =>
                {
                    if (isChanged(f, i))
                    {
                        this.Scripts.Add(new ScriptStatus(dropType, key, makeDropScript(i)));
                        this.Scripts.Add(new ScriptStatus(createType, key, makeCreateScript(f)));
                    }
                });
        }

        public List<ScriptStatus> Scripts { get; private set; }
        public List<ScriptStatus> Initial { get; private set; }
        public List<ScriptStatus> Final { get; private set; }

        public DatabaseDelta(DatabaseState final, DatabaseState initial, IAssemblyStateReader asm, IDatabaseScriptGenerator gen)
        {
            this.Scripts = new List<ScriptStatus>();
            this.Initial = new List<ScriptStatus>();
            this.Final = new List<ScriptStatus>();

            if (initial.CatalogueExists == false)
            {
                this.Scripts.Add(new ScriptStatus(ScriptType.CreateCatalogue, initial.CatalogueName, gen.MakeCreateCatalogueScript(initial.CatalogueName)));
            }
            ProcessDatabaseLogins(final.DatabaseLogins, initial.DatabaseLogins.Keys.ToList(), initial.CatalogueName, asm, gen);
            ProcessSchemas(final.Schemata.ToDictionary(k => k), initial.Schemata.ToDictionary(k => k), asm, gen);
            ProcessTables(final.Tables, initial.Tables, asm, gen);
            ProcessIndexes(final.Indexes, initial.Indexes, asm, gen);
            ProcessRelationships(final.Relationships, initial.Relationships, asm, gen);
            ProcessKeys(final.PrimaryKeys, initial.PrimaryKeys, asm, gen);
            ProcessFunctions(final.Functions, initial.Functions, asm, gen);
            this.Scripts.Sort();
            this.Initial.Sort();
            this.Final.Sort();
        }

        private void ProcessDatabaseLogins(Dictionary<string, string> final, List<string> initial, string databaseName, IAssemblyStateReader asm, IDatabaseScriptGenerator gen)
        {
            var names = initial.Select(s => s.ToLower()).ToArray();
            this.Scripts.AddRange(final
                .Where(u => !names.Contains(u.Key.ToLower()))
                .Select(u => new ScriptStatus(ScriptType.CreateDatabaseLogin, u.Key, gen.MakeCreateDatabaseLoginScript(u.Key, u.Value, databaseName))));
        }

        private void ProcessSchemas(Dictionary<string, string> finalSchemas, Dictionary<string, string> initialSchemas, IAssemblyStateReader asm, IDatabaseScriptGenerator gen)
        {
            Traverse(
                finalSchemas,
                initialSchemas,
                ScriptType.DropSchema,
                ScriptType.CreateSchema,
                (a, b) => false,
                gen.MakeDropSchemaScript,
                gen.MakeCreateSchemaScript);
        }

        private void ProcessFunctions(Dictionary<string, RoutineAttribute> finalRoutines, Dictionary<string, RoutineAttribute> initialRoutines, IAssemblyStateReader asm, IDatabaseScriptGenerator gen)
        {
            Traverse(
                finalRoutines,
                initialRoutines,
                ScriptType.DropRoutine,
                ScriptType.CreateRoutine,
                asm.RoutineChanged,
                gen.MakeDropRoutineScript,
                gen.MakeCreateRoutineScript);
        }

        private void ProcessKeys(Dictionary<string, PrimaryKey> finalKeys, Dictionary<string, PrimaryKey> initialKeys, IAssemblyStateReader asm, IDatabaseScriptGenerator gen)
        {
            Traverse(
                finalKeys,
                initialKeys,
                ScriptType.DropPrimaryKey,
                ScriptType.CreatePrimaryKey,
                asm.KeyChanged,
                gen.MakeDropPrimaryKeyScript,
                gen.MakeCreatePrimaryKeyScript);
        }

        private void ProcessRelationships(Dictionary<string, Relationship> finalRelations, Dictionary<string, Relationship> initialRelations, IAssemblyStateReader asm, IDatabaseScriptGenerator gen)
        {
            Traverse(
                finalRelations,
                initialRelations,
                ScriptType.DropRelationship,
                ScriptType.CreateRelationship,
                asm.RelationshipChanged,
                gen.MakeDropRelationshipScript,
                gen.MakeCreateRelationshipScript);
        }

        private void ProcessIndexes(Dictionary<string, Index> finalIndexes, Dictionary<string, Index> initialIndexes, IAssemblyStateReader asm, IDatabaseScriptGenerator gen)
        {
            Traverse(
                finalIndexes,
                initialIndexes,
                ScriptType.DropIndex,
                ScriptType.CreateIndex,
                asm.IndexChanged,
                gen.MakeDropIndexScript,
                gen.MakeCreateIndexScript);
        }

        private void ProcessTables(Dictionary<string, TableAttribute> finalTables, Dictionary<string, TableAttribute> initialTables, IAssemblyStateReader asm, IDatabaseScriptGenerator gen)
        {
            DumpAll(this.Initial, initialTables, ScriptType.CreateTable, gen.MakeCreateTableScript);
            DumpAll(this.Final, finalTables, ScriptType.CreateTable, gen.MakeCreateTableScript);
            Traverse(
                finalTables,
                initialTables,
                (tableName, initialTable) =>
                {
                    this.Scripts.Add(new ScriptStatus(ScriptType.DropTable, tableName, gen.MakeDropTableScript(initialTable)));
                },
                (tableName, finalTable) =>
                {
                    this.Scripts.Add(new ScriptStatus(ScriptType.CreateTable, tableName, gen.MakeCreateTableScript(finalTable)));
                },
                (tableName, finalTable, initialTable) =>
                {
                    var finalColumns = finalTable.Properties.ToDictionary(p => gen.MakeIdentifier(finalTable.Schema ?? gen.DefaultSchemaName, finalTable.Name, p.Name).ToLower());
                    var initialColumns = initialTable.Properties.ToDictionary(p => gen.MakeIdentifier(initialTable.Schema ?? gen.DefaultSchemaName, initialTable.Name, p.Name).ToLower());

                    Traverse(
                        finalColumns,
                        initialColumns,
                        (columnName, initialColumn) =>
                        {
                            this.Scripts.Add(new ScriptStatus(ScriptType.DropColumn, columnName, gen.MakeDropColumnScript(initialColumn)));
                        },
                        (columnName, finalColumn) =>
                        {
                            this.Scripts.Add(new ScriptStatus(ScriptType.CreateColumn, columnName, gen.MakeCreateColumnScript(finalColumn)));
                        },
                        (columnName, finalColumn, initialColumn) =>
                        {
                            if (asm.ColumnChanged(finalColumn, initialColumn))
                            {
                                this.Scripts.Add(new ScriptStatus(ScriptType.AlterColumn, columnName, gen.MakeAlterColumnScript(finalColumn, initialColumn)));
                            }
                        });
                });
        }
    }
}
