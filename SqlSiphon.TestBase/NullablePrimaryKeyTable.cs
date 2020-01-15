using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [Table]
    public class NullablePrimaryKeyTable
    {
        [PK]
        public int? KeyColumn { get; set; }
    }
}
