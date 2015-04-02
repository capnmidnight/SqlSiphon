using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Examples.SqlServer.Runner
{
    public class Ops
    {
        public static Ops Equal = new Ops("=");
        public static Ops NotEqual = new Ops("!=");
        public static Ops Like = new Ops("like");
        public static Ops LessThan = new Ops("<");
        public static Ops LessThanEqual = new Ops("<=");
        public static Ops GreaterThan = new Ops(">");
        public static Ops GreaterThanEqual = new Ops(">=");
        private string token;
        internal Ops(string token) { this.token = token; }
        public override string ToString() { return this.token; }
    }
}
