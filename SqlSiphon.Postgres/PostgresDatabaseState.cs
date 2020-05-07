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
        public Dictionary<string, Dictionary<string, string>> Settings { get; private set; }

        public PostgresDatabaseState(DatabaseState state)
            : base(state)
        {
            Extensions = new Dictionary<string, pg_extension>();
            Settings = new Dictionary<string, Dictionary<string, string>>();
        }

        public override DatabaseDelta Diff(DatabaseState initial, ISqlSiphon dal)
        {
            var pg = initial as PostgresDatabaseState;
            if (pg != null)
            {
                /// Remove any references to functions, tables, etc. that were installed
                /// by a Postgres Extension Module, which we will not want to uninstall.
                var extSchema = Extensions.Keys.ToList();
                RemoveExtensionObjects(extSchema, pg.Functions);
                RemoveExtensionObjects(extSchema, pg.Indexes);
                RemoveExtensionObjects(extSchema, pg.PrimaryKeys);
                RemoveExtensionObjects(extSchema, pg.Relationships);
                RemoveExtensionObjects(extSchema, pg.Tables);
                RemoveExtensionObjects(extSchema, pg.UDTTs);
                RemoveExtensionObjects(extSchema, pg.Views);
            }

            var delta = base.Diff(initial, dal);

            if (pg != null)
            {
                ProcessExtensions(dal, delta, pg.Extensions);
                ProcessSettings(dal, delta, pg.Settings);
            }

            delta.Scripts.Sort();
            delta.Initial.Sort();
            delta.Final.Sort();
            return delta;
        }

        private void RemoveExtensionObjects<T>(List<string> extSchema, Dictionary<string, T> collect)
            where T : Mapping.DatabaseObjectAttribute
        {
            var remove = collect
                .Where(f => extSchema.Contains(f.Value.Schema))
                .Select(f => f.Key).ToList();
            foreach (var key in remove)
            {
                collect.Remove(key);
            }
        }

        private void ProcessExtensions(ISqlSiphon dal, DatabaseDelta delta, Dictionary<string, pg_extension> extensions)
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

        private void ProcessSettings(ISqlSiphon dal, DatabaseDelta delta, Dictionary<string, Dictionary<string, string>> initialUserSettings)
        {
            foreach (var userName in Settings.Keys)
            {
                var userSettings = Settings[userName];
                var isNewUser = !initialUserSettings.ContainsKey(userName);
                foreach (var setting in userSettings)
                {
                    var isNewSetting = !initialUserSettings[userName].ContainsKey(setting.Key);
                    var finalValue = setting.Value;
                    var initialValue = (isNewUser || isNewSetting) ? null : initialUserSettings[userName][setting.Key];
                    var settingValueChanged = !SettingsAreEqual(finalValue, initialValue);
                    if (isNewUser || isNewSetting || settingValueChanged)
                    {
                        delta.Scripts.Add(new ScriptStatus(
                            ScriptType.AlterSettings,
                            $"set {setting.Key} for {userName}.",
                            $"alter user {userName} set {setting.Key} = {finalValue};",
                            isNewUser
                                ? "Initializing new user"
                                : isNewSetting
                                    ? "New setting for user"
                                    : $"Setting value has changed from `{initialValue}` to `{finalValue}`"));
                    }
                }
            }
        }

        private static string[] ParseValues(string value)
        {
            return value
                .Split(',')
                .Select(p =>
                {
                    p = p.Trim();
                    if (p.Length > 0 && p[0] == '"')
                    {
                        p = p.Substring(1);
                        p = p.Substring(0, p.Length - 1);
                    }
                    return p.Trim();
                })
                .Where(p => p.Length > 0)
                .OrderBy(p => p)
                .ToArray();
        }

        private static bool SettingsAreEqual(string finalValue, string initialValue)
        {
            var final = ParseValues(finalValue);
            var initial = ParseValues(initialValue);

            if(final.Length != initial.Length)
            {
                return false;
            }

            for(var i = 0; i < final.Length; ++i)
            {
                if (final[i] != initial[i])
                {
                    return false;
                }
            }

            return true;
        }

        public void AddExtension(pg_extension ext)
        {
            AddExtension(ext.extname, ext.extversion);
        }

        public void AddExtension(string name, string version)
        {
            Extensions.Add(name, new pg_extension(name, version));
        }

        public void AddUserSettings(pg_user row)
        {
            var userName = row.usename;
            var config = row.useconfig;
            foreach (var line in config)
            {
                var sep = line.IndexOf('=');
                var key = line.Substring(0, sep);
                var value = line.Substring(sep + 1);
                AddUserSetting(userName, key, value);
            }
        }

        public void AddUserSetting(string userName, string settingName, string value)
        {
            if (!Settings.ContainsKey(userName))
            {
                Settings.Add(userName, new Dictionary<string, string>());
            }

            Settings[userName][settingName] = value;
        }
    }
}
