using System;

namespace SqlSiphon
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DatabaseVendorNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public DatabaseVendorNameAttribute(string name)
        {
            Name = name;
        }
    }
}
