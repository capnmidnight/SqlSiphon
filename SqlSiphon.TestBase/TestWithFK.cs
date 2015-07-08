using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [Table]
    public class TestWithFK
    {
        [PK]
        public int Stuff { get; set; }

        [FK(typeof(TestPrimaryKeyColumn))]
        [Column(Size = 255)]
        public string KeyColumn { get; set; }
    }
}
