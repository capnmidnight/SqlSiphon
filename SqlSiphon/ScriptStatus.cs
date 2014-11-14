using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon
{
    [MappedClass]
    public class ScriptStatus
    {
        [AutoPK]
        public int ScriptID { get; set; }
        public string Script { get; set; }
        [MappedProperty(DefaultValue = "getdate()")]
        public DateTime RanOn { get; set; }
    }
}
