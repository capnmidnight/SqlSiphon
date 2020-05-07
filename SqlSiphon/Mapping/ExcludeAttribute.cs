using System;

namespace SqlSiphon.Mapping
{
    /// <summary>
    /// Shorthand for "Include = false"
    /// 
    /// Only one attribute of a given type may be applied to
    /// any type of thing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class ExcludeAttribute : ColumnAttribute
    {
        public ExcludeAttribute()
        {
            Include = false;
        }
    }
}
