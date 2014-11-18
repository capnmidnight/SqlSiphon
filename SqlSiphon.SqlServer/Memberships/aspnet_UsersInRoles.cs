using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.SqlServer.Memberships
{
    class aspnet_UsersInRoles
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
    }
}
