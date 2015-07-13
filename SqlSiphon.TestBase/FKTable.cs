using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [Table]
    public class FKTable
    {
        [PK]
        public int Stuff { get; set; }

        [FK(typeof(PrimaryKeyColumnTable))]
        [Column(Size = 255)]
        public string KeyColumn { get; set; }
    }
}
