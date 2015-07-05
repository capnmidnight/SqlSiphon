using System;
using SqlSiphon.Mapping;

namespace SqlSiphon.Examples
{
    public enum YesNo
    {
        None,
        Yes,
        No
    }

    [Table]
    public class TestEntity1
    {
        [AutoPK]
        public int ColumnA { get; set; }

        public int? ColumnB { get; set; }

        [Column(DefaultValue = "2")]
        public int? ColumnC { get; set; }

        [Column(Name = "renamedColumnD")]
        public long ColumnD { get; set; }

        [Index("idx_tp")]
        public YesNo ColumnE { get; set; }
    }
}
