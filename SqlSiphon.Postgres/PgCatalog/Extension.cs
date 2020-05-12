using System;
using SqlSiphon.Mapping;

namespace SqlSiphon.Postgres.PgCatalog
{
    [View(
        Include = false,
        Schema = "pg_catalog",
        Name = "pg_extension")]
    internal class Extension
    {
        [Column(Name = "extname")]
        public string Name { get; set; }

        [Column(Name = "extversion")]
        public string VersionString
        {
            get { return Version.ToString(); }
            set { Version = new Version(value); }
        }

        public Version Version { get; set; }

        public Extension() { }

        public Extension(string name, string version)
        {
            Name = name;
            VersionString = version;
        }
    }
}
