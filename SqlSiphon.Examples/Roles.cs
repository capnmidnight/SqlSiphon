using System;

using SqlSiphon.Mapping;

namespace SqlSiphon.Examples
{
    [Table]
    public class Roles
    {
        [PK]
        public Guid RoleID { get; set; }

        [FK(typeof(Applications))]
        public Guid ApplicationID { get; set; }

        [Column(StringLength = 256)]
        public string RoleName { get; set; }

        [Column(StringLength = 256)]
        public string LoweredRoleName { get; set; }

        [Column(StringLength = 256, IsOptional = true)]
        public string Description { get; set; }
    }
}
