using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Examples.SqlServer.Runner
{
    public abstract class LogicClauseBuilder<C> : ClauseBuilder<C> where C : LogicClauseBuilder<C>
    {
        protected LogicClauseBuilder(Dictionary<string, Type> symbols)
            : base(symbols)
        {
        }

        public C And<T, U, V>(Func<T, V> left, Ops op, Func<U, V> right)
            where T : BoundObject, new()
            where U : BoundObject, new()
        {
            return (C)this.AppendExpression<T, U, V>(left, op, right, " and ");
        }

        public C And<T, V>(Func<T, V> left, Ops op, V right)
            where T : BoundObject, new()
        {
            return (C)this.AppendExpression<T, V>(left, op, right, " and ");
        }

        public C Or<T, U, V>(Func<T, V> left, Ops op, Func<U, V> right)
            where T : BoundObject, new()
            where U : BoundObject, new()
        {
            return (C)this.AppendExpression<T, U, V>(left, op, right, " or ");
        }

        public C Or<T, V>(Func<T, V> left, Ops op, V right)
            where T : BoundObject, new()
        {
            return (C)this.AppendExpression<T, V>(left, op, right, " or ");
        }
    }
}
