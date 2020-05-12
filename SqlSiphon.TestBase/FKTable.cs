using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [Table]
    public class FKTable
    {
        [PK]
        public int Stuff { get; set; }

        [FK(typeof(PrimaryKeyColumnTable))]
        [Column(StringLength = 255)]
        public string KeyColumn { get; set; }
    }
}
