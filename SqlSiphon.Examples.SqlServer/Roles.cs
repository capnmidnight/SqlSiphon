using System;
using SqlSiphon.Mapping;

namespace SqlSiphon.Examples.SqlServer
{
    [Table]
    public class Roles
    {
        public static Relationship FK_Roles_to_Applications = new Relationship(typeof(Roles), typeof(Applications));
        
        [PK]
        public Guid RoleID { get; set; }
        public Guid ApplicationID { get; set; }
        [Column(Size = 256)]
        public string Rolename { get; set; }
        [Column(Size = 256)]
        public string LoweredRolename { get; set; }
        [Column(Size = 256, IsOptional = true)]
        public string Description { get; set; }
    }
}
