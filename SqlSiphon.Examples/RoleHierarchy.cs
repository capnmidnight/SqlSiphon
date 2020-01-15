using System;

using SqlSiphon.Mapping;

namespace SqlSiphon.Examples
{
    [Table]
    public class RoleHierarchy
    {
        [PK, FK(typeof(Roles))]
        public Guid RoleID { get; set; }

        [PK, FK(typeof(Roles))]
        public Guid ParentRoleID { get; set; }
    }
}
