using System;

using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [Table]
    public class LongFKTable
    {
        [PK]
        public int Stuff { get; set; }

        [FK(typeof(PrimaryKeyTwoColumnsTable))]
        [Column(Size = 255)]
        public string KeyColumn1 { get; set; }

        [FK(typeof(PrimaryKeyTwoColumnsTable))]
        public DateTime KeyColumn2 { get; set; }
    }
}
