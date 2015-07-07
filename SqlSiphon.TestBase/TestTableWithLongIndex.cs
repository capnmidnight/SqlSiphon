using System;
using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [Table]
    public class TestTableWithLongIndex
    {
        [AutoPK]
        public int KeyColumn { get; set; }

        public float NotInIndex { get; set; }

        [Index("idx_Test2")]
        public double FloatColumn { get; set; }

        [Index("idx_Test2")]
        public int IntColumn { get; set; }

        public byte ByteColumn { get; set; }

        [Index("idx_Test2")]
        public bool BoolColumn { get; set; }

        [Index("idx_Test3")]
        public long LongColumn { get; set; }

        [Index("idx_Test3")]
        public decimal DecimalColumn { get; set; }

        [Index("idx_Test3")]
        public char CharColumn { get; set; }
    }
}
