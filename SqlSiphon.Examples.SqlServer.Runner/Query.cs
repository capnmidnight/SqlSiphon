using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Examples.SqlServer.Runner
{
    public class Query<T> where T : BoundObject, new()
    {
        private string alias;
        private Dictionary<string, Type> symbols;

        public Query(Dictionary<string, Type> symbols, string alias = null)
        {
            this.symbols = symbols;
            this.alias = alias;
        }

        public FromClauseBuilder InnerJoin<U, V>(string alias, Func<T, V> left, Ops op, Func<U, V> right)
            where U : BoundObject, new()
        {
            var spec = new FromClauseBuilder(this.symbols, typeof(T), this.alias);
            return spec.InnerJoin(alias, left, op, right);
        }

        public FromClauseBuilder InnerJoin<U, V>(Func<T, V> left, Ops op, Func<U, V> right)
            where U : BoundObject, new()
        {
            return this.InnerJoin(null, left, op, right);
        }

        public FromClauseBuilder LeftOuterJoin<U, V>(string alias, Func<T, V> left, Ops op, Func<U, V> right)
            where U : BoundObject, new()
        {
            var spec = new FromClauseBuilder(this.symbols, typeof(T), this.alias);
            return spec.LeftOuterJoin(alias, left, op, right);
        }

        public FromClauseBuilder LeftOuterJoin<U, V>(Func<T, V> left, Ops op, Func<U, V> right)
            where U : BoundObject, new()
        {
            return this.LeftOuterJoin(null, left, op, right);
        }

        public FromClauseBuilder RightOuterJoin<U, V>(string alias, Func<T, V> left, Ops op, Func<U, V> right)
            where U : BoundObject, new()
        {
            var spec = new FromClauseBuilder(this.symbols, typeof(T), this.alias);
            return spec.RightOuterJoin(alias, left, op, right);
        }

        public FromClauseBuilder RightOuterJoin<U, V>(Func<T, V> left, Ops op, Func<U, V> right)
            where U : BoundObject, new()
        {
            return this.RightOuterJoin(null, left, op, right);
        }

        public override string ToString()
        {
            return string.Format("from {0} as {1}", typeof(T).Name, this.alias);
        }
    }
}
