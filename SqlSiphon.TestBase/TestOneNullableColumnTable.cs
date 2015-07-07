using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [Table]
    public class TestOneNullableColumnTable
    {
        public int? ColumnA { get; set; }
    }
}
