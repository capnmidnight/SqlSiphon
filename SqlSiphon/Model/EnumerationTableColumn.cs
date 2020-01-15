using System;
using System.Collections.Generic;

using SqlSiphon.Mapping;

namespace SqlSiphon.Model
{
    public class EnumerationTableColumn : ColumnAttribute
    {
        public EnumerationTableColumn(Type enumType, TableAttribute table, string columnName, Type systemType)
        {
            SourceObject = enumType;
            Table = table;
            Name = columnName;
            SetSystemType(systemType);
        }

        public override object GetValue(object source)
        {
            if (source is KeyValuePair<int, string>)
            {
                var kp = (KeyValuePair<int, string>)source;
                if (Name == "Value")
                {
                    return kp.Key;
                }
                else if (Name == "Description")
                {
                    return kp.Value;
                }
            }
            return null;
        }
    }
}
