using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.Postgres.Memberships
{
    [MappedClass]
    public class aspnet_Users
    {
        [PK(DefaultValue = "newid()")]
        public Guid UserId { get; set; }

        public Guid ApplicationId { get; set; }

        [MappedProperty(Size = 256)]
        public string UserName { get; set; }

        [MappedProperty(Size = 256)]
        public string LoweredUserName { get; set; }

        [MappedProperty(Size = 16)]
        public string MobileAlias { get; set; }

        [MappedProperty(DefaultValue = "'0'")]
        public bool IsAnonymous { get; set; }
        public DateTime LastActivityDate { get; set; }
    }
}
