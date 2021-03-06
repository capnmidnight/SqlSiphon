﻿using System;

using SqlSiphon.Mapping;

namespace SqlSiphon.Examples
{
    [Table]
    public class Applications
    {
        [PK(DefaultValue = "newid()")]
        public Guid ApplicationID { get; set; }

        [Column(StringLength = 256)]
        public string ApplicationName { get; set; }

        [Column(StringLength = 256)]
        public string LoweredApplicationName { get; set; }

        [Column(StringLength = 256, IsOptional = true)]
        public string Description { get; set; }
    }
}
