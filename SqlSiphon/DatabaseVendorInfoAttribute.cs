using System;

namespace SqlSiphon
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class DatabaseVendorInfoAttribute : Attribute
    {
        public string Name { get; }

        public string ToolName { get; }

        public string DefaultPath { get; }

        public DatabaseVendorInfoAttribute(string name, string toolName, string defaultPath)
        {
            Name = name;
            ToolName = toolName;
            DefaultPath = defaultPath;
        }
    }
}
