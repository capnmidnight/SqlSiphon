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
        protected Dictionary<string, Type> tables;

        protected ClauseBuilder(Dictionary<string, Type> tables)
        {
            this.query = new StringBuilder();
            this.tables = tables;
        }

        protected C Combine<T, U, V>(string pre, Func<T, V> left, Ops op, Func<U, V> right)
            where T : BoundObject, new()
            where U : BoundObject, new()
        {
            this.query.AppendFormat("\n\t\t{0} ", pre);
            return this.Combine(left, op, right);
        }

        protected C Combine<T, V>(string pre, Func<T, V> left, Ops op, V right)
            where T : BoundObject, new()
        {
            this.query.AppendFormat("\n\t{0}", this.GetExpression(left, op, right));
            return (C)this;
        }

        protected C Combine<T, V>(Func<T, V> left, Ops op, V right)
            where T : BoundObject, new()
        {
            this.query.Append(this.GetExpression(left, op, right));
            return (C)this;
        }

        protected C Combine<T, U, V>(Func<T, V> left, Ops op, Func<U, V> right)
            where T : BoundObject, new()
            where U : BoundObject, new()
        {
            this.query.Append(this.GetExpression(left, op, right));
            return (C)this;
        }

        protected string GetExpression<T, V>(Func<T, V> left, Ops op, V right)
            where T : BoundObject, new()
        {
            var l = GetColumnSpec(left);
            return string.Join(" ", l, op, right);
        }

        protected string GetExpression<T, U, V>(Func<T, V> left, Ops op, Func<U, V> right)
            where T : BoundObject, new()
            where U : BoundObject, new()
        {
            var l = GetColumnSpec(left);
            var r = GetColumnSpec(right);
            return string.Join(" ", l, op, r);
        }

        protected string GetColumnSpec<T, V>(Func<T, V> f)
            where T : BoundObject, new()
        {
            var tableAlias = f.Method.GetParameters().First().Name;

            if (!this.tables.ContainsKey(tableAlias))
            {
                throw new Exception("don't know this table: " + tableAlias);
            }

            var t = typeof(T);
            if (this.tables[tableAlias] != t)
            {
                throw new Exception("type didn't match: " + tableAlias);
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
