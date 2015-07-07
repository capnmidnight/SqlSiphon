using System;
using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [Table]
    public class TestNullablePrimaryKey
    {
        [PK]
        public int? KeyColumn { get; set; }
    }
}
