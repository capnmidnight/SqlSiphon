using System;

using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [Table]
    public class IdentityColumnTable
    {
        [AutoPK]
        public int KeyColumn { get; set; }

        public DateTime DateColumn { get; set; }
    }
}
