using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [Table]
    public class TestTwoColumnTableAsChild : TestOneColumnTable
    {
        public int ColumnB { get; set; }
    }
}
