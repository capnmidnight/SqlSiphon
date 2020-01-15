using System.Collections.Generic;

using SqlSiphon.Mapping;

namespace SqlSiphon.Model
{
    public class TableIndex : DatabaseObjectAttribute
    {
        public TableAttribute Table { get; private set; }
        public List<string> Columns { get; private set; }

        public bool IsClustered { get; set; }

        internal TableIndex(TableAttribute table, string name)
        {
            Name = name;
            Table = table;
            Columns = new List<string>();
        }
    }
}
