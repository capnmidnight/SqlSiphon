using System;
using SqlSiphon.Mapping;

namespace SqlSiphon.Examples
{
    [Table]
    [FK(typeof(Users))]
    [FK(typeof(Roles))]
    public class UsersInRoles
    {
        [PK]
        public Guid UserID { get; set; }
        [PK]
        public Guid RoleID { get; set; }
    }
}
