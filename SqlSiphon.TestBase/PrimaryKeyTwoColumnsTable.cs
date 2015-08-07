﻿using System;
using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [Table]
    public class PrimaryKeyTwoColumnsTable
    {
        [PK(Size=255)]
        public string KeyColumn1 { get; set; }

        [PK]
        public DateTime KeyColumn2 { get; set; }
    }
}