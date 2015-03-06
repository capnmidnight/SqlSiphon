using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.SqlServer.Memberships
{
    public class aspnet_Applications
    {
        public Guid ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public string LoweredApplicationName { get; set; }

        [Column(IsOptional = true)]
        public string Description { get; set; }
    }
}
