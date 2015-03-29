using System;
using SqlSiphon.Mapping;

namespace SqlSiphon.Examples.SqlServer
{
    [Table]
    public class UsersInRoles
    {
        public static Relationship FK_UsersInRoles_to_Users = new Relationship(typeof(UsersInRoles), typeof(Users));
        public static Relationship FK_UsersInRoles_to_Roles = new Relationship(typeof(UsersInRoles), typeof(Roles));
        
        [PK]
        public Guid UserID { get; set; }
        [PK]
        public Guid RoleID { get; set; }
    }
}
