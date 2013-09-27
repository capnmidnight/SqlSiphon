using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon.SqlServer
{
    /// <summary>
    /// An attribute to use for tagging methods as being mapped to a stored procedure call.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum, Inherited = true, AllowMultiple = false)]
    public class SqlServerMappedClassAttribute : SqlSiphon.Mapping.MappedClassAttribute
    {
        public bool IsUploadable { get; set; }
    }
}
