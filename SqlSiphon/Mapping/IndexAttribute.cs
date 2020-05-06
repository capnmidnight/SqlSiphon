using System;

namespace SqlSiphon.Mapping
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class IndexAttribute : Attribute
    {
        public string Schema { get; set; }
        public string Name { get; private set; }
        public IndexAttribute(string name)
        {
            Name = name;
        }
    }
}
