using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.Postgres.Memberships
{
    [MappedClass]
    class aspnet_UsersInRoles
    {
        [PK]
        public Guid UserId { get; set; }
        [PK]
        public Guid RoleId { get; set; }
    }
}
