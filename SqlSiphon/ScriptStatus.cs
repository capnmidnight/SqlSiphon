using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon
{
    [Mapping.MappedClass]
    public class ScriptStatus
    {
        public string Script { get; set; }
        [Mapping.MappedProperty(DefaultValue = "getdate()")]
        public DateTime RanOn { get; set; }
    }
}
