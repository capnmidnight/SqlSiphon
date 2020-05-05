using System;

namespace SqlSiphon.Postgres
{
    internal class pg_extension
    {
        public string extname { get; set; }
        public string extversion
        {
            get { return Version.ToString(); }
            set { Version = new Version(value); }
        }

        public Version Version { get; set; }

        public pg_extension() { }

        public pg_extension(string name, string version)
        {
            extname = name;
            extversion = version;
        }
    }
}
