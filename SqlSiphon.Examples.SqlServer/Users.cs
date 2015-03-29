using System;
using SqlSiphon.Mapping;

namespace SqlSiphon.Examples.SqlServer
{
    [Table]
    [FK(typeof(Applications))]
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
        [Column(IsOptional = true, DefaultValue = "0")]
        public bool IsAnonymous { get; set; }
        public DateTime LastActivityDate { get; set; }
    }
}
