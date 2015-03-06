using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon.Mapping
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class IncludeInIndexAttribute : Attribute
    {
        public string Name { get; private set; }
        public IncludeInIndexAttribute(string name)
        {
            this.Name = name;
        }
    }
}
