using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;
namespace SqlSiphon
{
    public class DatabaseDelta
    {
        private static Dictionary<T, U> Clone<T, U>(Dictionary<T, U> dict)
        {
            var clone = new Dictionary<T, U>();
            foreach (var p in dict)
            {
                clone.Add(p.Key, p.Value);
            }
            return clone;
        }

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

        public Dictionary<string, string> CreateTablesScripts { get; private set; }
        public Dictionary<string, string> DropTablesScripts { get; private set; }
        public Dictionary<string, string> UnalteredTablesScripts { get; private set; }
        public Dictionary<string, string> CreateColumnsScripts { get; private set; }
        public Dictionary<string, string> DropColumnsScripts { get; private set; }
        public Dictionary<string, string> AlteredColumnsScripts { get; private set; }
        public Dictionary<string, string> UnalteredColumnsScripts { get; private set; }
        public Dictionary<string, string> CreateRelationshipsScripts { get; private set; }
        public Dictionary<string, string> DropRelationshipsScripts { get; private set; }
        public Dictionary<string, string> UnalteredRelationshipsScripts { get; private set; }
        public Dictionary<string, string> CreateRoutinesScripts { get; private set; }
        public Dictionary<string, string> DropRoutinesScripts { get; private set; }
        public Dictionary<string, string> AlteredRoutinesScripts { get; private set; }
        public Dictionary<string, string> UnalteredRoutinesScripts { get; private set; }
        public Dictionary<string, string> OtherScripts { get; private set; }

        public DatabaseDelta(DatabaseState final, DatabaseState initial, ISqlSiphon dal)
        {
            var finalTables = Clone(final.Tables);
            var finalRelations = Clone(final.Relationships);
            var finalRoutines = Clone(final.Functions);

            var initialTables = Clone(initial.Tables);
            var initialRelations = Clone(initial.Relationships);
            var initialRoutines = Clone(initial.Functions);

            this.CreateTablesScripts = new Dictionary<string, string>();
            this.DropTablesScripts = new Dictionary<string, string>();
            this.UnalteredTablesScripts = new Dictionary<string, string>();
            this.CreateColumnsScripts = new Dictionary<string, string>();
            this.DropColumnsScripts = new Dictionary<string, string>();
            this.AlteredColumnsScripts = new Dictionary<string, string>();
            this.UnalteredColumnsScripts = new Dictionary<string, string>();
            this.CreateRelationshipsScripts = new Dictionary<string, string>();
            this.DropRelationshipsScripts = new Dictionary<string, string>();
            this.UnalteredRelationshipsScripts = new Dictionary<string, string>();
            this.CreateRoutinesScripts = new Dictionary<string, string>();
            this.DropRoutinesScripts = new Dictionary<string, string>();
            this.AlteredRoutinesScripts = new Dictionary<string, string>();
            this.UnalteredRoutinesScripts = new Dictionary<string, string>();
            this.OtherScripts = new Dictionary<string, string>();

            ProcessTables(finalTables, initialTables, dal);
            ProcessRelationships(finalRelations, initialRelations, dal);
            ProcessRoutines(finalRoutines, initialRoutines, dal);
        }

        private void ProcessRoutines(Dictionary<string, MappedMethodAttribute> finalRoutines, Dictionary<string, MappedMethodAttribute> initialRoutines, ISqlSiphon dal)
        {
            Traverse(
                finalRoutines, 
                initialRoutines, 
                (routineName, initialRoutine) => this.DropRoutinesScripts.Add(routineName, dal.MakeDropRoutineScript(initialRoutine)), 
                (routineName, finalRoutine) => this.CreateRoutinesScripts.Add(routineName, dal.MakeCreateRoutineScript(finalRoutine)),
                (routineName, finalRoutine, initialRoutine) =>
                {
                    if (dal.RoutineChanged(finalRoutine, initialRoutine))
                    {
                        this.AlteredRoutinesScripts.Add(routineName, dal.MakeAlterRoutineScript(finalRoutine));
                    }
                    else
                    {
                        this.UnalteredRoutinesScripts.Add(routineName, "-- no changes");
                    }
                });
        }

        private void ProcessRelationships(Dictionary<string, Relationship> finalRelations, Dictionary<string, Relationship> initialRelations, ISqlSiphon dal)
        {
            Traverse(
                finalRelations, 
                initialRelations,
                (relationName, initialRelation) => this.DropRelationshipsScripts.Add(relationName, dal.MakeDropRelationshipScript(initialRelation)),
                (relationName, finalRelation) => this.CreateRelationshipsScripts.Add(relationName, dal.MakeCreateRelationshipScript(finalRelation)), 
                (relationName, finalRelation, initialRelation) =>
                {
                    if (dal.RelationshipChanged(finalRelation, initialRelation))
                    {
                        this.CreateRelationshipsScripts.Add(relationName, 
                            dal.MakeDropRelationshipScript(initialRelation)
                            + dal.MakeCreateRelationshipScript(finalRelation));
                    }
                    else
                    {
                        this.UnalteredRelationshipsScripts.Add(relationName, "-- no changes");
                    }
                });
        }

        private void ProcessTables(Dictionary<string, MappedClassAttribute> finalTables, Dictionary<string, MappedClassAttribute> initialTables, ISqlSiphon dal)
        {
            Traverse(
                finalTables, 
                initialTables, 
                (tableName, initialTable) => this.DropTablesScripts.Add(tableName, dal.MakeDropTableScript(initialTable)), 
                (tableName, finalTable) => this.CreateTablesScripts.Add(tableName, dal.MakeCreateTableScript(finalTable)), 
                (tableName, finalTable, initialTable) =>
                {
                    var tableAltered = false;
                    var finalColumns = finalTable.Properties.ToDictionary(p => dal.MakeIdentifier(finalTable.Schema, finalTable.Name, p.Name));
                    var initialColumns = initialTable.Properties.ToDictionary(p => dal.MakeIdentifier(initialTable.Schema, initialTable.Name, p.Name));

                    Traverse(finalColumns, initialColumns, (columnName, initialColumn) =>
                    {
                        this.DropColumnsScripts.Add(columnName, dal.MakeDropColumnScript(initialColumn));
                        tableAltered = true;
                    }, (columnName, finalColumn) =>
                    {
                        this.CreateColumnsScripts.Add(columnName, dal.MakeCreateColumnScript(finalColumn));
                        tableAltered = true;
                    }, (columnName, finalColumn, initialColumn) =>
                    {
                        if (dal.ColumnChanged(finalColumn, initialColumn))
                        {
                            this.AlteredColumnsScripts.Add(columnName, dal.MakeAlterColumnScript(finalColumn, initialColumn));
                            tableAltered = true;
                        }
                        else
                        {
                            this.UnalteredColumnsScripts.Add(columnName, "-- no changes");
                        }
                    });
                    if (!tableAltered)
                    {
                        this.UnalteredTablesScripts.Add(tableName, "-- no changes");
                    }
                });
        }
    }
}
