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
            Func<T, T, bool> isChanged,
            Func<T, string> makeDropScript,
            Func<T, string> makeCreateScript)
        {
            Traverse(
                final,
                initial,
                (key, i) =>
                {
                    var script = makeDropScript(i);
                    if (script != null)
                    {
                        this.Scripts.Add(new ScriptStatus(dropType, key, script));
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

        public DatabaseDelta(DatabaseState final, DatabaseState initial, ISqlSiphon dal)
        {
            this.Scripts = new List<ScriptStatus>();
            if (!initial.CatalogueExists)
            {
                this.Scripts.Add(new ScriptStatus(ScriptType.CreateCatalogue, initial.CatalogueName, dal.MakeCreateCatalogueScript(initial.CatalogueName)));
            }
            else
            {
                ProcessSchemas(final.Schemata.ToDictionary(k => k), initial.Schemata.ToDictionary(k => k), dal);
                ProcessTables(final.Tables, initial.Tables, dal);
                ProcessIndexes(final.Indexes, initial.Indexes, dal);
                ProcessRelationships(final.Relationships, initial.Relationships, dal);
                ProcessKeys(final.PrimaryKeys, initial.PrimaryKeys, dal);
                ProcessFunctions(final.Functions, initial.Functions, dal);
            }
        }

        private void ProcessSchemas(Dictionary<string, string> finalSchemas, Dictionary<string, string> initialSchemas, ISqlSiphon dal)
        {
            Traverse(
                finalSchemas, 
                initialSchemas, 
                ScriptType.DropSchema, 
                ScriptType.CreateSchema, 
                (a, b) => false,
                dal.MakeDropSchemaScript,
                dal.MakeCreateSchemaScript);
        }

        private void ProcessFunctions(Dictionary<string, RoutineAttribute> finalRoutines, Dictionary<string, RoutineAttribute> initialRoutines, ISqlSiphon dal)
        {
            Traverse(
                finalRoutines,
                initialRoutines,
                ScriptType.DropRoutine,
                ScriptType.CreateRoutine,
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
                },
                (tableName, finalTable) =>
                {
                    this.Scripts.Add(new ScriptStatus(ScriptType.CreateTable, tableName, dal.MakeCreateTableScript(finalTable)));
                },
                (tableName, finalTable, initialTable) =>
                {
                    var finalColumns = finalTable.Properties.ToDictionary(p => dal.MakeIdentifier(finalTable.Schema ?? dal.DefaultSchemaName, finalTable.Name, p.Name).ToLower());
                    var initialColumns = initialTable.Properties.ToDictionary(p => dal.MakeIdentifier(initialTable.Schema ?? dal.DefaultSchemaName, initialTable.Name, p.Name).ToLower());

                    Traverse(
                        finalColumns,
                        initialColumns,
                        (columnName, initialColumn) =>
                        {
                            this.Scripts.Add(new ScriptStatus(ScriptType.DropColumn, columnName, dal.MakeDropColumnScript(initialColumn)));
                        },
                        (columnName, finalColumn) =>
                        {
                            this.Scripts.Add(new ScriptStatus(ScriptType.CreateColumn, columnName, dal.MakeCreateColumnScript(finalColumn)));
                        },
                        (columnName, finalColumn, initialColumn) =>
                        {
                            if (dal.ColumnChanged(finalColumn, initialColumn))
                            {
                                this.Scripts.Add(new ScriptStatus(ScriptType.AlterColumn, columnName, dal.MakeAlterColumnScript(finalColumn, initialColumn)));
                            }
                        });
                });
        }
    }
}
