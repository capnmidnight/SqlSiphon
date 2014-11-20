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
            var startTables = Clone(initial.Tables);
            var endTables = Clone(final.Tables);
            var startRelations = Clone(initial.Relationships);
            var endRelations = Clone(final.Relationships);
            var startFunctions = Clone(initial.Functions);
            var endFunctions = Clone(final.Functions);

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

            var allTables = endTables.Keys
                .Union(startTables.Keys)
                .Distinct()
                .ToArray();
            foreach (var tableName in allTables)
            {
                if (endTables.ContainsKey(tableName) && startTables.ContainsKey(tableName))
                {
                    var f = endTables[tableName];
                    var i = startTables[tableName];
                    endTables.Remove(tableName);
                    startTables.Remove(tableName);
                    var fColumns = f.Properties.ToDictionary(p => dal.MakeIdentifier(f.Schema, f.Name, p.Name));
                    var iColumns = i.Properties.ToDictionary(p => dal.MakeIdentifier(i.Schema, i.Name, p.Name));
                    var allColumns = fColumns.Keys
                        .Union(iColumns.Keys)
                        .Distinct()
                        .ToArray();
                    var altered = false;
                    foreach (var columnName in allColumns)
                    {
                        if (fColumns.ContainsKey(columnName) && iColumns.ContainsKey(columnName))
                        {
                            var fc = fColumns[columnName];
                            var ic = iColumns[columnName];
                            fColumns.Remove(columnName);
                            iColumns.Remove(columnName);
                            if (dal.ColumnChanged(fc, ic))
                            {
                                this.AlteredColumnsScripts.Add(columnName, dal.MakeAlterColumnScript(fc, ic));
                                altered = true;
                            }
                            else
                            {
                                this.UnalteredColumnsScripts.Add(columnName, "-- no changes");
                            }
                        }
                        else if (fColumns.ContainsKey(columnName))
                        {
                            this.CreateColumnsScripts.Add(columnName, dal.MakeCreateColumnScript(fColumns[columnName]));
                            altered = true;
                        }
                        else
                        {
                            this.DropColumnsScripts.Add(columnName, dal.MakeDropColumnScript(iColumns[columnName]));
                            altered = true;
                        }
                    }
                    if (!altered)
                    {
                        this.UnalteredTablesScripts.Add(tableName, "-- no changes");
                    }
                }
                else if (endTables.ContainsKey(tableName))
                {
                    this.CreateTablesScripts.Add(tableName, dal.MakeCreateTableScript(endTables[tableName]));
                }
                else if (startTables.ContainsKey(tableName))
                {
                    this.DropTablesScripts.Add(tableName, dal.MakeDropTableScript(startTables[tableName]));
                }
                else
                {
                    this.UnalteredTablesScripts.Add(tableName, "-- where did this come from?");
                }
            }
        }
    }
}
