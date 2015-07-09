﻿using System;
using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [Table]
    public class TestTableWithSimpleIndex
    {
        [AutoPK]
        public int KeyColumn { get; set; }

        public float NotInIndex { get; set; }

        [Index("idx_Test1")]
        public double DoubleColumn { get; set; }
    }
}
