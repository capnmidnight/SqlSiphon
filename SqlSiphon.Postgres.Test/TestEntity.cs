using SqlSiphon.Mapping;

namespace SqlSiphon.Postgres.Test
{
    [MappedType(SqlType = "setof test_table")]
    class TestEntity
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}
