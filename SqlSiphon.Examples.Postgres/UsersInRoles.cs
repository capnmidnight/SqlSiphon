using System;
using SqlSiphon.Mapping;

namespace SqlSiphon.Examples.SqlServer
{
    [Table]
    public class UsersInRoles
    {
        [PK]
        public Guid UserID { get; set; }
        [PK]
        public Guid RoleID { get; set; }
    }
}
