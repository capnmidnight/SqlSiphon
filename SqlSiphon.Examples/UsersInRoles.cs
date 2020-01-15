using System;

using SqlSiphon.Mapping;

namespace SqlSiphon.Examples
{
    [Table]
    public class UsersInRoles
    {
        [PK, FK(typeof(Users))]
        public Guid UserID { get; set; }

        [PK, FK(typeof(Roles))]
        public Guid RoleID { get; set; }
    }
}
