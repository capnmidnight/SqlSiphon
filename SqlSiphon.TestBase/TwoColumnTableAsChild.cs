using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [Table]
    public class TwoColumnTableAsChild : OneColumnTable
    {
        public int ColumnB { get; set; }
    }
}
