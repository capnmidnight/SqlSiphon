using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon.Postgres
{
    class PostgresDatabaseState : DatabaseState
    {
        public Dictionary<string, Version> Extensions { get; private set; }
        public PostgresDatabaseState(DatabaseState state)
            : base(state)
        {
            this.Extensions = new Dictionary<string, Version>();
        }

        public override DatabaseDelta Diff(DatabaseState initial, ISqlSiphon dal)
        {
            var delta = base.Diff(initial, dal);
            var pg = initial as PostgresDatabaseState;
            if (pg != null)
            {
                foreach (var ext in this.Extensions)
                {
                    if (!pg.Extensions.ContainsKey(ext.Key))
                    {
                        delta.Scripts.Add(new ScriptStatus(
                            ScriptType.InstallExtension, 
                            string.Format("{0} v{1}", ext.Key, ext.Value),
                            string.Format("create extension \"{0}\";", ext.Key)));
                    }
                    else if (pg.Extensions[ext.Key] < ext.Value)
                    {
                        delta.Scripts.Add(new ScriptStatus(
                            ScriptType.InstallExtension,
                            string.Format("{0} v{1}", ext.Key, ext.Value),
                            string.Format("alter extension \"{0}\" update;", ext.Key)));
                    }
                }
            }
            return delta;
        }

        public void AddExtension(pg_extension ext)
        {
            this.AddExtension(ext.extname, ext.extversion);
        }

        public void AddExtension(string name, string version)
        {
            this.Extensions.Add(name, new Version(version));
        }
    }
}
