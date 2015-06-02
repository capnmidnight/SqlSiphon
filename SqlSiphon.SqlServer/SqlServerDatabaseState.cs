﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSiphon.Mapping;

namespace SqlSiphon.SqlServer
{
    internal class SqlServerDatabaseState : DatabaseState
    {
        public Dictionary<string, TableAttribute> UDTTs;
        public SqlServerDatabaseState(DatabaseState state)
            : base(state)
        {
            this.UDTTs = new Dictionary<string, TableAttribute>();
        }
        
        public override DatabaseDelta Diff(DatabaseState initial, IAssemblyStateReader asm, IDatabaseScriptGenerator dal)
        {
            var delta = base.Diff(initial, asm, dal);
            var ssState = initial as SqlServerDatabaseState;
            var ssDal = dal as SqlServerDataAccessLayer;
            if (ssState != null && ssDal != null)
            {
                ProcessUDTTs(delta, this.UDTTs, ssState.UDTTs, asm, ssDal);
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
                (UDTTName, initialUDTT) =>
                {
                    delta.Scripts.Add(new ScriptStatus(ScriptType.DropUDTT, UDTTName, gen.MakeDropUDTTScript(initialUDTT)));
                },
                (UDTTName, finalUDTT) =>
                {
                    delta.Scripts.Add(new ScriptStatus(ScriptType.CreateUDTT, UDTTName, gen.MakeCreateUDTTScript(finalUDTT)));
                },
                (UDTTName, finalUDTT, initialUDTT) =>
                {
                    var finalColumns = finalUDTT.Properties.ToDictionary(p => gen.MakeIdentifier(finalUDTT.Schema ?? gen.DefaultSchemaName, finalUDTT.Name, p.Name).ToLower());
                    var initialColumns = initialUDTT.Properties.ToDictionary(p => gen.MakeIdentifier(initialUDTT.Schema ?? gen.DefaultSchemaName, initialUDTT.Name, p.Name).ToLower());

                    bool changed = false;
                    DatabaseDelta.Traverse(
                        finalColumns,
                        initialColumns,
                        (columnName, initialColumn) =>
                        {
                            changed = true;
                        },
                        (columnName, finalColumn) =>
                        {
                            changed = true;
                        },
                        (columnName, finalColumn, initialColumn) =>
                        {
                            var colDiff = asm.ColumnChanged(finalColumn, initialColumn);
                            changed = changed || asm.ColumnChanged(finalColumn, initialColumn);
                        });
                    if (changed)
                    {
                        delta.Scripts.Add(new ScriptStatus(ScriptType.DropUDTT, UDTTName, gen.MakeDropUDTTScript(initialUDTT)));
                        delta.Scripts.Add(new ScriptStatus(ScriptType.CreateUDTT, UDTTName, gen.MakeCreateUDTTScript(finalUDTT)));
                    }
                });
        }
    }
}
