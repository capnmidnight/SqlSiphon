using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon.Mapping
{
    public class TableIndex : DatabaseObjectAttribute
    {
        public TableAttribute Table { get; private set; }
        public List<string> Columns { get; private set; }

        internal TableIndex(TableAttribute table, string name)
        {
            this.Name = name;
            this.Table = table;
            this.Columns = new List<string>();
        }
    }
}
