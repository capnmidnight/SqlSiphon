using SqlSiphon.Mapping;

namespace SqlSiphon.Postgres.Test
{
    [Table(Name = "test_table")]
    class TestEntity
    {
        [AutoPK]
        public int id { get; set; }
        public string name { get; set; }
    }
}
