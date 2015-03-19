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

        private void Traverse<T>(
            Dictionary<string, T> final,
            Dictionary<string, T> initial,
            ScriptType dropType,
            ScriptType createType,
            Dictionary<string, string> dropScripts,
            Dictionary<string, string> createScripts,
            Dictionary<string, string> noChangeScripts,
            Func<T, T, bool> isChanged,
            Func<T, string> makeDropScript,
            Func<T, string> makeCreateScript)
        {
            Traverse(
                final,
                initial,
                (key, i) =>
                {
                    this.Scripts.Add(new ScriptStatus(dropType, key, makeDropScript(i)));
                    dropScripts.Add(key, makeDropScript(i));
                },
                (key, f) =>
                {
                    this.Scripts.Add(new ScriptStatus(createType, key, makeCreateScript(f)));
                    createScripts.Add(key, makeCreateScript(f));
                },
                (key, f, i) =>
                {
                    if (isChanged(f, i))
                    {
                        this.Scripts.Add(new ScriptStatus(dropType, key, makeDropScript(i)));
                        this.Scripts.Add(new ScriptStatus(createType, key, makeCreateScript(f)));
                        dropScripts.Add(key, makeDropScript(i));
                        createScripts.Add(key, makeCreateScript(f));
                    }
                    else
                    {
                        noChangeScripts.Add(key, "-- no change --");
                    }
                });
        }

        public List<ScriptStatus> Scripts { get; private set; }

        public Dictionary<string, string> CreateSchemaScripts { get; private set; }
        public Dictionary<string, string> DropSchemaScripts { get; private set; }
        public Dictionary<string, string> UnalteredSchemaScripts { get; private set; }
        public Dictionary<string, string> CreateTableScripts { get; private set; }
        public Dictionary<string, string> DropTableScripts { get; private set; }
        public Dictionary<string, string> UnalteredTableScripts { get; private set; }
        public Dictionary<string, string> CreateColumnScripts { get; private set; }
        public Dictionary<string, string> DropColumnScripts { get; private set; }
        public Dictionary<string, string> AlteredColumnScripts { get; private set; }
        public Dictionary<string, string> UnalteredColumnScripts { get; private set; }
        public Dictionary<string, string> CreateRelationshipScripts { get; private set; }
        public Dictionary<string, string> DropRelationshipScripts { get; private set; }
        public Dictionary<string, string> UnalteredRelationshipScripts { get; private set; }
        public Dictionary<string, string> CreateRoutineScripts { get; private set; }
        public Dictionary<string, string> DropRoutineScripts { get; private set; }
        public Dictionary<string, string> AlteredRoutineScripts { get; private set; }
        public Dictionary<string, string> UnalteredRoutineScripts { get; private set; }
        public Dictionary<string, string> CreateIndexScripts { get; private set; }
        public Dictionary<string, string> DropIndexScripts { get; private set; }
        public Dictionary<string, string> UnalteredIndexScripts { get; private set; }
        public Dictionary<string, string> OtherScripts { get; private set; }

        public DatabaseDelta(DatabaseState final, DatabaseState initial, ISqlSiphon dal)
        {
            this.Scripts = new List<ScriptStatus>();

            this.CreateSchemaScripts = new Dictionary<string, string>();
            this.DropSchemaScripts = new Dictionary<string, string>();
            this.UnalteredSchemaScripts = new Dictionary<string, string>();
            this.CreateTableScripts = new Dictionary<string, string>();
            this.DropTableScripts = new Dictionary<string, string>();
            this.UnalteredTableScripts = new Dictionary<string, string>();
            this.CreateColumnScripts = new Dictionary<string, string>();
            this.DropColumnScripts = new Dictionary<string, string>();
            this.AlteredColumnScripts = new Dictionary<string, string>();
            this.UnalteredColumnScripts = new Dictionary<string, string>();
            this.CreateRelationshipScripts = new Dictionary<string, string>();
            this.DropRelationshipScripts = new Dictionary<string, string>();
            this.UnalteredRelationshipScripts = new Dictionary<string, string>();
            this.CreateRoutineScripts = new Dictionary<string, string>();
            this.DropRoutineScripts = new Dictionary<string, string>();
            this.AlteredRoutineScripts = new Dictionary<string, string>();
            this.UnalteredRoutineScripts = new Dictionary<string, string>();
            this.OtherScripts = new Dictionary<string, string>();
            this.CreateIndexScripts = new Dictionary<string, string>();
            this.DropIndexScripts = new Dictionary<string, string>();
            this.UnalteredIndexScripts = new Dictionary<string, string>();

            ProcessSchemas(final.Schemata.ToDictionary(k => k), initial.Schemata.ToDictionary(k => k), dal);
            ProcessTables(final.Tables, initial.Tables, dal);
            ProcessIndexes(final.Indexes, initial.Indexes, dal);
            ProcessRelationships(final.Relationships, initial.Relationships, dal);
            ProcessKeys(final.PrimaryKeys, initial.PrimaryKeys, dal);
            ProcessFunctions(final.Functions, initial.Functions, dal);
        }

        private void ProcessSchemas(Dictionary<string, string> finalSchemas, Dictionary<string, string> initialSchemas, ISqlSiphon dal)
        {
            Traverse(
                finalSchemas,
                initialSchemas,
                (schemaName, initialSchema) =>
                {
                    this.Scripts.Add(new ScriptStatus(ScriptType.DropSchema, schemaName, dal.MakeDropSchemaScript(initialSchema)));
                    this.DropSchemaScripts.Add(schemaName, dal.MakeDropSchemaScript(initialSchema));
                },
                (schemaName, finalSchema) =>
                {
                    this.Scripts.Add(new ScriptStatus(ScriptType.CreateSchema, schemaName, dal.MakeCreateSchemaScript(finalSchema)));
                    this.CreateSchemaScripts.Add(schemaName, dal.MakeCreateSchemaScript(finalSchema));
                },
                (schemaName, finalSchema, initialSchema) => this.UnalteredSchemaScripts.Add(schemaName, "-- no change"));
        }

        private void ProcessFunctions(Dictionary<string, RoutineAttribute> finalRoutines, Dictionary<string, RoutineAttribute> initialRoutines, ISqlSiphon dal)
        {
            Traverse(
                finalRoutines,
                initialRoutines,
                ScriptType.DropRoutine,
                ScriptType.CreateRoutine,
                this.DropRoutineScripts,
                this.CreateRoutineScripts,
                this.UnalteredRoutineScripts,
                dal.RoutineChanged,
                dal.MakeDropRoutineScript,
                dal.MakeCreateRoutineScript);
        }

        private void ProcessKeys(Dictionary<string, PrimaryKey> finalKeys, Dictionary<string, PrimaryKey> initialKeys, ISqlSiphon dal)
        {
            Traverse(
                finalKeys,
                initialKeys,
                ScriptType.DropPrimaryKey,
                ScriptType.CreatePrimaryKey,
                this.DropRelationshipScripts,
                this.CreateRelationshipScripts,
                this.UnalteredRelationshipScripts,
                dal.KeyChanged,
                dal.MakeDropPrimaryKeyScript,
                dal.MakeCreatePrimaryKeyScript);
        }

        private void ProcessRelationships(Dictionary<string, Relationship> finalRelations, Dictionary<string, Relationship> initialRelations, ISqlSiphon dal)
        {
            Traverse(
                finalRelations,
                initialRelations,
                ScriptType.DropRelationship,
                ScriptType.CreateRelationship,
                this.DropRelationshipScripts,
                this.CreateRelationshipScripts,
                this.UnalteredRelationshipScripts,
                dal.RelationshipChanged,
                dal.MakeDropRelationshipScript,
                dal.MakeCreateRelationshipScript);
        }

        private void ProcessIndexes(Dictionary<string, Index> finalIndexes, Dictionary<string, Index> initialIndexes, ISqlSiphon dal)
        {
            Traverse(
                finalIndexes,
                initialIndexes,
                ScriptType.DropIndex,
                ScriptType.CreateIndex,
                this.DropIndexScripts,
                this.CreateIndexScripts,
                this.UnalteredIndexScripts,
                dal.IndexChanged,
                dal.MakeDropIndexScript,
                dal.MakeCreateIndexScript);
        }

        private void ProcessTables(Dictionary<string, TableAttribute> finalTables, Dictionary<string, TableAttribute> initialTables, ISqlSiphon dal)
        {
            Traverse(
                finalTables,
                initialTables,
                (tableName, initialTable) =>
                {
                    this.Scripts.Add(new ScriptStatus(ScriptType.DropTable, tableName, dal.MakeDropTableScript(initialTable)));
                    this.DropTableScripts.Add(tableName, dal.MakeDropTableScript(initialTable));
                },
                (tableName, finalTable) =>
                {
                    this.Scripts.Add(new ScriptStatus(ScriptType.CreateTable, tableName, dal.MakeCreateTableScript(finalTable)));
                    this.CreateTableScripts.Add(tableName, dal.MakeCreateTableScript(finalTable));
                },
                (tableName, finalTable, initialTable) =>
                {
                    var finalColumns = finalTable.Properties.ToDictionary(p => dal.MakeIdentifier(finalTable.Schema ?? dal.DefaultSchemaName, finalTable.Name, p.Name).ToLower());
                    var initialColumns = initialTable.Properties.ToDictionary(p => dal.MakeIdentifier(initialTable.Schema ?? dal.DefaultSchemaName, initialTable.Name, p.Name).ToLower());

                    var initialCount = this.DropColumnScripts.Count + this.CreateColumnScripts.Count + this.AlteredColumnScripts.Count;

                    Traverse(
                        finalColumns,
                        initialColumns,
                        (columnName, initialColumn) =>
                        {
                            this.Scripts.Add(new ScriptStatus(ScriptType.DropColumn, columnName, dal.MakeDropColumnScript(initialColumn)));
                            this.DropColumnScripts.Add(columnName, dal.MakeDropColumnScript(initialColumn));
                        },
                        (columnName, finalColumn) =>
                        {
                            this.Scripts.Add(new ScriptStatus(ScriptType.CreateColumn, columnName, dal.MakeCreateColumnScript(finalColumn)));
                            this.CreateColumnScripts.Add(columnName, dal.MakeCreateColumnScript(finalColumn));
                        },
                        (columnName, finalColumn, initialColumn) =>
                        {
                            if (dal.ColumnChanged(finalColumn, initialColumn))
                            {
                                this.Scripts.Add(new ScriptStatus(ScriptType.AlterColumn, columnName, dal.MakeAlterColumnScript(finalColumn, initialColumn)));
                                this.AlteredColumnScripts.Add(columnName, dal.MakeAlterColumnScript(finalColumn, initialColumn));
                            }
                            else
                            {
                                this.UnalteredColumnScripts.Add(columnName, "-- no changes");
                            }
                        });

                    var finalCount = this.DropColumnScripts.Count + this.CreateColumnScripts.Count + this.AlteredColumnScripts.Count;
                    if (finalCount == initialCount)
                    {
                        this.UnalteredTableScripts.Add(tableName, "-- no changes");
                    }
                });
        }
    }
}
