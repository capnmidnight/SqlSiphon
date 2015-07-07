using System;
using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [Table]
    public class TestLongStringPrimaryKey
    {
        [PK]
        public string KeyColumn { get; set; }

        public DateTime DateColumn { get; set; }
    }
}
