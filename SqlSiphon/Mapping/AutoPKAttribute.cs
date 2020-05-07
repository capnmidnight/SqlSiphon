using System;

namespace SqlSiphon.Mapping
{
    /// <summary>
    /// An attribute to tag a property in a class as a "standard"
    /// primary key. IsIdentity and IncludedInPrimaryKey are set
    /// to true by default.
    /// 
    /// Only one attribute of a given type may be applied to
    /// any type of thing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class AutoPKAttribute : PKAttribute
    {
        public AutoPKAttribute()
        {
            IsIdentity = true;
        }
    }
}
