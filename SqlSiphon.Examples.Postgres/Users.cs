using System;
using SqlSiphon.Mapping;

namespace SqlSiphon.Examples.Postgres
{
    [Table]
    public class Users
    {
        [PK]
        public Guid UserID { get; set; }
        public Guid ApplicationID { get; set; }
        [Column(Size = 256)]
        public string UserName { get; set; }
        [Column(Size = 256)]
        public string LoweredUserName { get; set; }
        [Column(Size = 16, IsOptional = true)]
        public string MobileAlias { get; set; }
        [Column(IsOptional = true, DefaultValue= "false")]
        public bool IsAnonymous { get; set; }
        public DateTime LastActivityDate { get; set; }
    }
}
