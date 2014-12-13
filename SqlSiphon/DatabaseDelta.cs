using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;
namespace SqlSiphon
{
    public class DatabaseDelta
    {
        private static void Traverse<T>(Dictionary<string, T> final, Dictionary<string, T> initial, Action<string, T> remove, Action<string, T> add, Action<string, T, T> change)
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
            ProcessRelationships(final.Relationships, initial.Relationships, dal);
            ProcessKeys(final.PrimaryKeys, initial.PrimaryKeys, dal);
            ProcessFunctions(final.Functions, initial.Functions, dal);
        }

        private void ProcessSchemas(Dictionary<string, string> finalSchemas, Dictionary<string, string> initialSchemas, ISqlSiphon dal)
        {
            Traverse(
                finalSchemas,
                initialSchemas,
                (schemaName, initialSchema) => this.DropSchemaScripts.Add(schemaName, dal.MakeDropSchemaScript(initialSchema)),
                (schemaName, finalSchema) => this.CreateSchemaScripts.Add(schemaName, dal.MakeCreateSchemaScript(finalSchema)),
                (schemaName, finalSchema, initialSchema) => this.UnalteredSchemaScripts.Add(schemaName, "-- no change"));
        }

        private void ProcessFunctions(Dictionary<string, MappedMethodAttribute> finalRoutines, Dictionary<string, MappedMethodAttribute> initialRoutines, ISqlSiphon dal)
        {
            Traverse(
                finalRoutines, 
                initialRoutines, 
                (routineName, initialRoutine) => this.DropRoutineScripts.Add(routineName, dal.MakeDropRoutineScript(initialRoutine)), 
                (routineName, finalRoutine) => this.CreateRoutineScripts.Add(routineName, dal.MakeCreateRoutineScript(finalRoutine)),
                (routineName, finalRoutine, initialRoutine) =>
                {
                    if (dal.RoutineChanged(finalRoutine, initialRoutine))
                    {
                        this.AlteredRoutineScripts.Add(routineName, dal.MakeAlterRoutineScript(finalRoutine, initialRoutine));
                    }
                    else
                    {
                        this.UnalteredRoutineScripts.Add(routineName, "-- no changes");
                    }
                });
        }

        private void ProcessKeys(Dictionary<string, PrimaryKey> finalKeys, Dictionary<string, PrimaryKey> initialKeys, ISqlSiphon dal)
        {
            Traverse(
                finalKeys,
                initialKeys,
                (keyName, initialKey) => this.DropRelationshipScripts.Add(keyName, dal.MakeDropPrimaryKeyScript(initialKey)),
                (keyName, finalKey) => this.CreateRelationshipScripts.Add(keyName, dal.MakeCreatePrimaryKeyScript(finalKey)),
                (keyName, finalKey, initialKey) =>
                {
                    if (dal.KeyChanged(finalKey, initialKey))
                    {
                        this.CreateRelationshipScripts.Add(keyName,
                            dal.MakeDropPrimaryKeyScript(initialKey)
                            + dal.MakeCreatePrimaryKeyScript(finalKey));
                    }
                    else
                    {
                        this.UnalteredRelationshipScripts.Add(keyName, "-- no changes");
                    }
                });
        }

        private void ProcessRelationships(Dictionary<string, Relationship> finalRelations, Dictionary<string, Relationship> initialRelations, ISqlSiphon dal)
        {
            Traverse(
                finalRelations, 
                initialRelations,
                (relationName, initialRelation) => this.DropRelationshipScripts.Add(relationName, dal.MakeDropRelationshipScript(initialRelation)),
                (relationName, finalRelation) => this.CreateRelationshipScripts.Add(relationName, dal.MakeCreateRelationshipScript(finalRelation)), 
                (relationName, finalRelation, initialRelation) =>
                {
                    if (dal.RelationshipChanged(finalRelation, initialRelation))
                    {
                        this.CreateRelationshipScripts.Add(relationName, 
                            dal.MakeDropRelationshipScript(initialRelation)
                            + dal.MakeCreateRelationshipScript(finalRelation));
                    }
                    else
                    {
                        this.UnalteredRelationshipScripts.Add(relationName, "-- no changes");
                    }
                });
        }

        private void ProcessTables(Dictionary<string, MappedClassAttribute> finalTables, Dictionary<string, MappedClassAttribute> initialTables, ISqlSiphon dal)
        {
            Traverse(
                finalTables, 
                initialTables, 
                (tableName, initialTable) => this.DropTableScripts.Add(tableName, dal.MakeDropTableScript(initialTable)), 
                (tableName, finalTable) => this.CreateTableScripts.Add(tableName, dal.MakeCreateTableScript(finalTable)), 
                (tableName, finalTable, initialTable) =>
                {
                    var tableAltered = false;
                    var finalColumns = finalTable.Properties.ToDictionary(p => dal.MakeIdentifier(finalTable.Schema ?? dal.DefaultSchemaName, finalTable.Name, p.Name).ToLower());
                    var initialColumns = initialTable.Properties.ToDictionary(p => dal.MakeIdentifier(initialTable.Schema ?? dal.DefaultSchemaName, initialTable.Name, p.Name).ToLower());

                    Traverse(finalColumns, initialColumns, (columnName, initialColumn) =>
                    {
                        this.DropColumnScripts.Add(columnName, dal.MakeDropColumnScript(initialColumn));
                        tableAltered = true;
                    }, (columnName, finalColumn) =>
                    {
                        this.CreateColumnScripts.Add(columnName, dal.MakeCreateColumnScript(finalColumn));
                        tableAltered = true;
                    }, (columnName, finalColumn, initialColumn) =>
                    {
                        if (dal.ColumnChanged(finalColumn, initialColumn))
                        {
                            this.AlteredColumnScripts.Add(columnName, dal.MakeAlterColumnScript(finalColumn, initialColumn));
                            tableAltered = true;
                        }
                        else
                        {
                            this.UnalteredColumnScripts.Add(columnName, "-- no changes");
                        }
                    });
                    if (!tableAltered)
                    {
                        this.UnalteredTableScripts.Add(tableName, "-- no changes");
                    }
                });
        }
    }
}
