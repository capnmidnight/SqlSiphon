using SqlSiphon.Mapping;

namespace SqlSiphon.OleDB.Test
{
    [Table(Name = "test_table")]
    public class TestEntity
    {
        [AutoPK]
        public int id { get; set; }
        public string name { get; set; }

        public TestEntity() { }
        public TestEntity(string name)
        {
            this.name = name;
        }
    }
}
