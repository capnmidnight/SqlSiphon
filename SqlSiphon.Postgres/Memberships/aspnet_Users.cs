using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.Postgres.Memberships
{
    [Table]
    public class aspnet_Users
    {
        [PK(DefaultValue = "newid()")]
        public Guid UserId { get; set; }

        public Guid ApplicationId { get; set; }

        [Column(Size = 256)]
        public string UserName { get; set; }

        [Column(Size = 256)]
        public string LoweredUserName { get; set; }

        [Column(Size = 16)]
        public string MobileAlias { get; set; }

        [Column(DefaultValue = "'0'")]
        public bool IsAnonymous { get; set; }
        public DateTime LastActivityDate { get; set; }
    }
}
