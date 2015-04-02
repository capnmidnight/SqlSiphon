using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Examples.SqlServer.Runner
{
    public abstract class ClauseBuilder<C> where C : ClauseBuilder<C>
    {
        protected StringBuilder query;
        protected Dictionary<string, Type> symbols;

        protected ClauseBuilder(Dictionary<string, Type> symbols)
        {
            this.query = new StringBuilder();
            this.symbols = symbols;
        }

        {
        }

        {
        }

            where T : BoundObject, new()
        {
            return (C)this;
        }

            where T : BoundObject, new()
        {
            return (C)this;
        }

            where T : BoundObject, new()
        {
            var l = GetColumnSpec(left);
        }

            where T : BoundObject, new()
            where U : BoundObject, new()
        {
            var l = GetColumnSpec(left);
            var r = GetColumnSpec(right);
            return string.Join(" ", l, op, r);
        }

        {
            {
            }

            {
            }

            string columnSpec = null;
            var test = new T();
            test.PropertyAccessed += new BoundObject.PropertyAccessedEventHandler(delegate(object o, string columnName)
            {
                columnSpec = string.Join(".", tableAlias, columnName);
            });
            f(test);
            return columnSpec;
        }
    }
}
