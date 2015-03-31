using System;
using SqlSiphon.Mapping;

namespace SqlSiphon.Examples
{
    [Table]
    [FK(typeof(Applications))]
    public class Roles
    {
        [PK]
        public Guid RoleID { get; set; }
        public Guid ApplicationID { get; set; }
        [Column(Size = 256)]
        public string RoleName { get; set; }
        [Column(Size = 256)]
        public string LoweredRoleName { get; set; }
        [Column(Size = 256, IsOptional = true)]
        public string Description { get; set; }
    }
}
