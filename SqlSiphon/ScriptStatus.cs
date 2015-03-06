using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon
{
    [Table]
    public class ScriptStatus
    {
        [AutoPK]
        public int ScriptID { get; set; }
        public string Script { get; set; }
        [Column(DefaultValue = "getdate()")]
        public DateTime RanOn { get; set; }
    }
}
