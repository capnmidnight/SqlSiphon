using System;

namespace SqlSiphon.Mapping
{
    /// <summary>
    /// Shorthand for "IncludeInPrimaryKey=true"
    /// 
    /// Only one attribute of a given type may be applied to
    /// any type of thing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class PKAttribute : ColumnAttribute
    {
        public PKAttribute()
        {
            IncludeInPrimaryKey = true;
        }

        public override string ToString()
        {
            return base.ToString() + " PK";
        }
    }
}
