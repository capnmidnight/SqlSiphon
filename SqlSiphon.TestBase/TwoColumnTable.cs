using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [Table]
    public class TwoColumnTable
    {
        public int ColumnA { get; set; }
        public int ColumnB { get; set; }
    }
}
