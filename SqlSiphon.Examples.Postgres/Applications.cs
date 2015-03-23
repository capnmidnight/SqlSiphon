﻿using System;
using SqlSiphon.Mapping;

namespace SqlSiphon.Examples.Postgres
{
    [Table]
    public class Applications
    {
        [PK]
        public Guid ApplicationID { get; set; }

        [Column(Size = 256)]
        public string ApplicationName { get; set; }

        [Column(Size = 256)]
	    public string LoweredApplicationName { get; set; }

        [Column(Size = 256, IsOptional = true)]
        public string Description { get; set; }
    }
}