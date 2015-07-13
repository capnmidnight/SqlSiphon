using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [Table]
    public class OneNullableColumnTable
    {
        public int? ColumnA { get; set; }
    }
}
