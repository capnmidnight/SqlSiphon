using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Examples.SqlServer.Runner
{
    public static class Begin
    {
        public static Query<T> From<T>(string alias = null) where T : BoundObject, new()
        {
            return new Query<T>(new Dictionary<string, Type>(), alias);
        }

        public static BlockBuilder Declare<T>(string name)
        {
            var block = new BlockBuilder(new Dictionary<string, Type>());
            return block.Declare<T>(name);
        }
    }
}
