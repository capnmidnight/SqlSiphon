using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.SqlServer.Memberships
{
    public class aspnet_Roles
    {
        public Guid RoleId { get; set; }
        public Guid ApplicationId { get; set; }
        public string RoleName { get; set; }
        public string LoweredRoleName { get; set; }
        [Column(IsOptional = true)]
        public string Description { get; set; }
    }
}
