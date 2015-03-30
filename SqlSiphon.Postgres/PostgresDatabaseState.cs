using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon.Postgres
{
    /// <summary>
    /// Extends the DatabaseState class with some features that are specific to
    /// Postgres.
    /// </summary>
    internal class PostgresDatabaseState : DatabaseState
    {
        public Dictionary<string, pg_extension> Extensions { get; private set; }
        public PostgresDatabaseState(DatabaseState state)
            : base(state)
        {
            this.Extensions = new Dictionary<string, pg_extension>();
        }

        public override DatabaseDelta Diff(DatabaseState initial, IAssemblyStateReader asm, IDatabaseScriptGenerator dal)
        {
            var pg = initial as PostgresDatabaseState;
            if (pg != null)
            {
                RemoveExtensionObjects(pg);
            }

            var delta = base.Diff(initial, asm, dal);

            if (pg != null)
            {
                ProcessExtensions(dal, delta, pg.Extensions);
            }
            return delta;
        }

        private void RemoveExtensionObjects(PostgresDatabaseState pg)
        {
            var extSchema = this.Extensions.Keys.ToList();
            RemoveExtensionObjects(extSchema, pg.Functions);
            RemoveExtensionObjects(extSchema, pg.Indexes);
            RemoveExtensionObjects(extSchema, pg.PrimaryKeys);
            RemoveExtensionObjects(extSchema, pg.Relationships);
            RemoveExtensionObjects(extSchema, pg.Tables);
        }

        private void RemoveExtensionObjects<T>(List<string> extSchema, Dictionary<string, T> collect) where T : SqlSiphon.Mapping.DatabaseObjectAttribute
        {
            var remove = collect
                .Where(f => extSchema.Contains(f.Value.Schema))
                .Select(f => f.Key).ToList();
            foreach (var key in remove)
            {
                collect.Remove(key);
            }
        }

        private void ProcessExtensions(IDatabaseObjectHandler dal, DatabaseDelta delta, Dictionary<string, pg_extension> extensions)
        {
            foreach (var ext in this.Extensions)
            {
                if (!extensions.ContainsKey(ext.Key))
                {
                    var schemaName = dal.MakeIdentifier(ext.Key);
                    delta.Scripts.Add(new ScriptStatus(
                        ScriptType.InstallExtension,
                        string.Format("{0} v{1}", ext.Key, ext.Value.Version),
                        string.Format("create extension if not exists \"{0}\" with schema {1};",
                            ext.Key,
                            schemaName)));
                }
                else if (extensions[ext.Key].Version < ext.Value.Version)
                {
                    delta.Scripts.Add(new ScriptStatus(
                        ScriptType.InstallExtension,
                        string.Format("{0} v{1}", ext.Key, ext.Value.Version),
                        string.Format("alter extension \"{0}\" update;", ext.Key)));
                }
            }
        }

        public void AddExtension(pg_extension ext)
        {
            this.AddExtension(ext.extname, ext.extversion);
        }

        public void AddExtension(string name, string version)
        {
            this.Extensions.Add(name, new pg_extension(name, version));
        }
    }
}
