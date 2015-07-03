using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DatabaseVendorNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public DatabaseVendorNameAttribute(string name)
        {
            this.Name = name;
        }
    }
}
