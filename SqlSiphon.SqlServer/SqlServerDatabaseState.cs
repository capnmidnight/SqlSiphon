using System.Collections.Generic;
using System.Linq;

using SqlSiphon.Mapping;

namespace SqlSiphon.SqlServer
{
    internal class SqlServerDatabaseState : DatabaseState
    {
        public Dictionary<string, TableAttribute> UDTTs;
        public SqlServerDatabaseState(DatabaseState state)
            : base(state)
        {
            UDTTs = new Dictionary<string, TableAttribute>();
        }

        public override DatabaseDelta Diff(DatabaseState initial, IAssemblyStateReader asm, IDatabaseScriptGenerator dal)
        {
            var delta = base.Diff(initial, asm, dal);
            if (initial is SqlServerDatabaseState ssState
                && dal is SqlServerDataAccessLayer ssDal)
            {
                ProcessUDTTs(delta, UDTTs, ssState.UDTTs, asm, ssDal);
            }

            delta.Scripts.Sort();
            delta.Initial.Sort();
            delta.Final.Sort();
            return delta;
        }

        private void ProcessUDTTs(DatabaseDelta delta, Dictionary<string, TableAttribute> finalUDTTs, Dictionary<string, TableAttribute> initialUDTTs, IAssemblyStateReader asm, SqlServerDataAccessLayer gen)
        {
            DatabaseDelta.DumpAll(delta.Initial, initialUDTTs, ScriptType.CreateUDTT, gen.MakeCreateUDTTScript);
            DatabaseDelta.DumpAll(delta.Final, finalUDTTs, ScriptType.CreateUDTT, gen.MakeCreateUDTTScript);
            DatabaseDelta.Traverse(
                finalUDTTs,
                initialUDTTs,
                (UDTTName, initialUDTT) => delta.Scripts.Add(new ScriptStatus(ScriptType.DropUDTT, UDTTName, gen.MakeDropUDTTScript(initialUDTT), "User-defined table type no longer exists")),
                (UDTTName, finalUDTT) => delta.Scripts.Add(new ScriptStatus(ScriptType.CreateUDTT, UDTTName, gen.MakeCreateUDTTScript(finalUDTT), "User-defined table type does not exist")),
                (UDTTName, finalUDTT, initialUDTT) =>
                {
                    var finalColumns = finalUDTT.Properties.ToDictionary(p => gen.MakeIdentifier(finalUDTT.Schema ?? gen.DefaultSchemaName, finalUDTT.Name, p.Name).ToLower());
                    var initialColumns = initialUDTT.Properties.ToDictionary(p => gen.MakeIdentifier(initialUDTT.Schema ?? gen.DefaultSchemaName, initialUDTT.Name, p.Name).ToLower());

                    var changed = false;
                    DatabaseDelta.Traverse(
                        finalColumns,
                        initialColumns,
                        (columnName, initialColumn) => changed = true,
                        (columnName, finalColumn) => changed = true,
                        (columnName, finalColumn, initialColumn) =>
                        {
                            var colDiff = asm.ColumnChanged(finalColumn, initialColumn);
                            changed = changed || colDiff != null;
                        }, true);
                    if (changed)
                    {
                        delta.Scripts.Add(new ScriptStatus(ScriptType.DropUDTT, UDTTName, gen.MakeDropUDTTScript(initialUDTT), "User-defined table type has changed"));
                        delta.Scripts.Add(new ScriptStatus(ScriptType.CreateUDTT, UDTTName, gen.MakeCreateUDTTScript(finalUDTT), "User-defined table type has changed"));
                    }
                },
                true);
        }
    }
}
