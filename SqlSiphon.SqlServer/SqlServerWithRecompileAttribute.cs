using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.SqlServer
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SqlServerWithRecompileAttribute : Attribute
    {

    }
}
