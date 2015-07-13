using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{

    [Table(Schema = "test")]
    public class OneColumnTableWithSchema
    {
        public int ColumnA { get; set; }
    }
}
