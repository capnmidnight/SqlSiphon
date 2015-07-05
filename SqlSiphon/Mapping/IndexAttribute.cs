using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon.Mapping
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class IndexAttribute : Attribute
    {
        public string Name { get; private set; }
        public IndexAttribute(string name)
        {
            this.Name = name;
        }
    }
}
