using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon.Mapping
{
    public class Index : MappedObjectAttribute
    {
        public MappedClassAttribute Table { get; private set; }
        public MappedPropertyAttribute[] Columns { get; private set; }

        public Index(Type toType, string name)
        {
            this.Name = name;
            this.Table = PrimaryKey.GetAttribute(toType);
            this.Columns = this.Table.Properties
                .Where(p => p.IncludeInIndex.Contains(name))
                .ToArray();
        }
    }
}
