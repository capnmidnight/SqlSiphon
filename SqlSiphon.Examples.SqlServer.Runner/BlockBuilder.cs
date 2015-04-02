using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Examples.SqlServer.Runner
{
    public class BlockBuilder : ClauseBuilder<BlockBuilder>
    {
        {
        }

        public BlockBuilder Declare<T>(string name)
        {
            ValidateNewName(name);
            this.AddSymbol(typeof(T), name);
            return this;
        }

        public Query<T> From<T>(string alias = null) 
            where T : BoundObject, new()
        {
            return new Query<T>(this.symbols, alias);
        }
    }
}

