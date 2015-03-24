using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon.Postgres
{
    class pg_extension
    {
        public string extname { get; set; }
        public string extversion
        {
            get { return this.Version.ToString(); }
            set { this.Version = new Version(value); }
        }

        public Version Version { get; set; }

        public pg_extension() { }

        public pg_extension(string name, string version)
        {
            this.extname = name;
            this.extversion = version;
        }
    }
}
