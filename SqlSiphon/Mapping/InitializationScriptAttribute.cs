using System;

namespace SqlSiphon.Mapping
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum, Inherited = false, AllowMultiple = true)]
    public class InitializationScriptAttribute : Attribute
    {
        public string Query { get; private set; }

        public InitializationScriptAttribute(string query)
        {
            Query = query;
        }
    }
}
