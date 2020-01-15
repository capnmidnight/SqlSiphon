using System;

namespace SqlSiphon.SqlServer
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SqlServerWithRecompileAttribute : Attribute
    {

    }
}
