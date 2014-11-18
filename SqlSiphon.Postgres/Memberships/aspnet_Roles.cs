using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.Postgres.Memberships
{
    [MappedClass]
    public class aspnet_Roles
    {
        [PK(DefaultValue = "newid()")]
        public Guid RoleId { get; set; }

        public Guid ApplicationId { get; set; }

        [MappedProperty(Size = 256)]
        public string RoleName { get; set; }

        [MappedProperty(Size = 256)]
        public string LoweredRoleName { get; set; }

        [MappedProperty(Size = 256, IsOptional = true)]
        public string Description { get; set; }
    }
}
