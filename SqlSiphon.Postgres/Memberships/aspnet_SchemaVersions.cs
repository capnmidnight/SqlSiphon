using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.Postgres.Memberships
{
    [MappedClass]
    public class aspnet_SchemaVersions
    {
        [PK(Size = 128)]
        public string Feature { get; set; }
        [PK(Size = 128)]
        public string CompatibleSchemaVersion { get; set; }
        public bool IsCurrentVersion { get; set; }
    }
}
