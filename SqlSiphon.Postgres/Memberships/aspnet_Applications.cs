using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.Postgres.Memberships
{
    [Table]
    public class aspnet_Applications
    {
        [PK(DefaultValue = "newid()")]
        public Guid ApplicationId { get; set; }

        [Column(Size = 256)]
        public string ApplicationName { get; set; }

        [Column(Size = 256)]
        public string LoweredApplicationName { get; set; }

        [Column(Size = 256, IsOptional = true)]
        public string Description { get; set; }
    }
}
