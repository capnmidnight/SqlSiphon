using System.Collections.Generic;
using System.Linq;

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
            Extensions = new Dictionary<string, pg_extension>();
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

            var searchPath = string.Join(",", Schemata.Select(s => dal.MakeIdentifier(s)));
            foreach (var userName in DatabaseLogins.Keys)
            {
                delta.Scripts.Add(new ScriptStatus(
                    ScriptType.AlterSettings,
                    $"set schema search path for {userName}",
                    $"alter user {userName} set search_path = {searchPath},public;",
                    "Schema search path needs to be set"));
            }

            delta.Scripts.Sort();
            delta.Initial.Sort();
            delta.Final.Sort();
            return delta;
        }

        private void RemoveExtensionObjects(PostgresDatabaseState pg)
        {
            var extSchema = Extensions.Keys.ToList();
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
            foreach (var ext in Extensions)
            {
                var extensionName = ext.Key;
                var requiredExtensionVersion = ext.Value.Version;
                if (!extensions.TryGetValue(extensionName, out var currentExtension))
                {
                    var schemaName = dal.MakeIdentifier(extensionName);
                    delta.Scripts.Add(new ScriptStatus(
                        ScriptType.InstallExtension,
                        $"{extensionName} v{requiredExtensionVersion}",
                        $"create extension if not exists \"{extensionName}\" with schema {schemaName};",
                            "Extension needs to be installed"));
                }
                else if (currentExtension.Version < requiredExtensionVersion)
                {
                    delta.Scripts.Add(new ScriptStatus(
                        ScriptType.InstallExtension,
                        $"{currentExtension.Version} v{requiredExtensionVersion}",
                        $"alter extension \"{extensionName}\" update;",
                        $"Extension needs to be upgraded. Was v{currentExtension.Version}, now v{requiredExtensionVersion}"));
                }
            }
        }

        public void AddExtension(pg_extension ext)
        {
            AddExtension(ext.extname, ext.extversion);
        }

        public void AddExtension(string name, string version)
        {
            Extensions.Add(name, new pg_extension(name, version));
        }
    }
}
