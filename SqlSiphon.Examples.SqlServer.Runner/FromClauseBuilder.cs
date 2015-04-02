using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Examples.SqlServer.Runner
{
    public class FromClauseBuilder : LogicClauseBuilder<FromClauseBuilder>
    {
        public FromClauseBuilder(Dictionary<string, Type> symbols, Type t, string alias = null)
            : base(symbols)
        {
            this.query.AppendFormat("from\n\t{0}", t.Name);
            if (alias != null)
            {
                this.query.AppendFormat(" as {0}", alias);
            }
            this.AddSymbol(t, alias);
        }

        public override string ToString()
        {
            return this.query.ToString();
        }

        public FromClauseBuilder InnerJoin<T, U, V>(string alias, Func<T, V> left, Ops op, Func<U, V> right)
            where T : BoundObject, new()
            where U : BoundObject, new()
        {
            this.AddSymbol(typeof(U), alias);
            this.query.AppendFormat("\n\tinner join {0} as {1}\n\t\ton ", typeof(U).Name, alias);
            return this.AppendExpression(left, op, right);
        }

        public FromClauseBuilder LeftOuterJoin<T, U, V>(string alias, Func<T, V> left, Ops op, Func<U, V> right)
            where T : BoundObject, new()
            where U : BoundObject, new()
        {
            this.AddSymbol(typeof(U), alias);
            this.query.AppendFormat("\n\tleft outer join {0} as {1}\n\t\ton ", typeof(U).Name, alias);
            return this.AppendExpression(left, op, right);
        }

        public FromClauseBuilder RightOuterJoin<T, U, V>(string alias, Func<T, V> left, Ops op, Func<U, V> right)
            where T : BoundObject, new()
            where U : BoundObject, new()
        {
            this.AddSymbol(typeof(U), alias);
            this.query.AppendFormat("\n\tright outer join {0} as {1}\n\t\ton ", typeof(U).Name, alias);
            return this.AppendExpression(left, op, right);
        }

        public SelectClauseBuilder Select<T, V>(Func<T, V> column, string alias = null)
            where T : BoundObject, new()
        {
            return new SelectClauseBuilder(this.symbols, this.ToString(), GetColumnSpec(column), alias);
        }
    }
}
