using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Mapping
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum, Inherited = false, AllowMultiple = true)]
    public class InitializationScriptAttribute : Attribute
    {
        public string Query { get; private set; }

        public InitializationScriptAttribute(string query)
        {
            this.Query = query;
        }
    }
}
