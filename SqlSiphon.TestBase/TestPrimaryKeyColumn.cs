using System;
using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [Table]
    public class TestPrimaryKeyColumn
    {
        [PK]
        public string KeyColumn { get; set; }

        public DateTime DateColumn { get; set; }
    }
}
