using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Examples.SqlServer.Runner
{
    public abstract class LogicClauseBuilder<C> : ClauseBuilder<C> where C : LogicClauseBuilder<C>
    {
        protected LogicClauseBuilder(Dictionary<string, Type> tables)
            : base(tables)
        {
        }

        public C And<T, U, V>(Func<T, V> left, Ops op, Func<U, V> right)
            where T : BoundObject, new()
            where U : BoundObject, new()
        {
            return (C)this.Combine<T, U, V>("and", left, op, right);
        }

        public C And<T, V>(Func<T, V> left, Ops op, V right)
            where T : BoundObject, new()
        {
            return (C)this.Combine<T, V>("and", left, op, right);
        }

        public C Or<T, U, V>(Func<T, V> left, Ops op, Func<U, V> right)
            where T : BoundObject, new()
            where U : BoundObject, new()
        {
            return (C)this.Combine<T, U, V>("or", left, op, right);
        }

        public C Or<T, V>(Func<T, V> left, Ops op, V right)
            where T : BoundObject, new()
        {
            return (C)this.Combine<T, V>("or", left, op, right);
        }
    }
}
