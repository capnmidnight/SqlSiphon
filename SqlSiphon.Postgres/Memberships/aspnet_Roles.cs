using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.Postgres.Memberships
{
    [Table]
    public class aspnet_Roles
    {
        [PK(DefaultValue = "newid()")]
        public Guid RoleId { get; set; }

        public Guid ApplicationId { get; set; }

        [Column(Size = 256)]
        public string RoleName { get; set; }

        [Column(Size = 256)]
        public string LoweredRoleName { get; set; }

        [Column(Size = 256, IsOptional = true)]
        public string Description { get; set; }
    }
}
