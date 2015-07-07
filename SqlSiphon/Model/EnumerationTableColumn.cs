using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSiphon.Mapping;

namespace SqlSiphon.Model
{
    public class EnumerationTableColumn : ColumnAttribute
    {
        public EnumerationTableColumn(Type enumType, TableAttribute table, string columnName, Type systemType)
        {
            this.SourceObject = enumType;
            this.Table = table;
            this.Name = columnName;
            this.SetSystemType(systemType);
        }

        public override object GetValue(object source)
        {
            if (source is KeyValuePair<int, string>)
            {
                var kp = (KeyValuePair<int, string>)source;
                if (this.Name == "Value")
                {
                    return kp.Key;
                }
                else if (this.Name == "Description")
                {
                    return kp.Value;
                }
            }
            return null;
        }
    }
}
