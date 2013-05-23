using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Test.Bootstrap
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new SqlSiphon.Test.UnifiedSetter();
            test.SetEnumByString();
        }
    }
}
