using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.Postgres
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class NpgsqlMappedMethodAttribute : MappedMethodAttribute
    {
        public string ReturnType { get; set; }
    }
}
