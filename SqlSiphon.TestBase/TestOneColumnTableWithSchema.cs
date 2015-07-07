using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{

    [Table(Schema = "test")]
    public class TestOneColumnTableWithSchema
    {
        public int ColumnA { get; set; }
    }
}
