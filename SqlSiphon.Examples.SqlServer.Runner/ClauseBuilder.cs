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

        protected void ValidateNewName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("need a name for the variable");
            }
        }

        protected void AddSymbol(Type t, string alias)
        {
            var name = alias ?? t.Name;
            ValidateNewName(name);
            if (this.symbols.ContainsKey(name))
            {
                throw new Exception("can't join in that name again: " + name);
            }
            else
            {
                this.symbols.Add(name, t);
            }
        }

        protected C AppendExpression<T, U, V>(Func<T, V> left, Ops op, Func<U, V> right, string pre = null, string post = null)
            where T : BoundObject, new()
            where U : BoundObject, new()
        {
            if (pre != null)
            {
                this.query.Append(pre);
            }
            this.query.Append(this.MakeExpression(left, op, right));
            if (post != null)
            {
                this.query.Append(post);
            }
            return (C)this;
        }

        protected C AppendExpression<T, V>(Func<T, V> left, Ops op, V right, string pre = null, string post = null)
            where T : BoundObject, new()
        {
            if (pre != null)
            {
                this.query.Append(pre);
            }
            this.query.Append(this.MakeExpression(left, op, right));
            if (post != null)
            {
                this.query.Append(post);
            }
            return (C)this;
        }

        protected string MakeExpression<T, V>(Func<T, V> left, Ops op, V value)
            where T : BoundObject, new()
        {
            if (value is string)
            {
                var symbolName = value as string;
                ValidateSymbol(symbolName, typeof(V));
            }
            var l = GetColumnSpec(left);
            string valueString = value != null ? value.ToString() : "null";
            return string.Join(" ", l, op, valueString);
        }

        protected string MakeExpression<T, U, V>(Func<T, V> left, Ops op, Func<U, V> right)
            where T : BoundObject, new()
            where U : BoundObject, new()
        {
            var l = GetColumnSpec(left);
            var r = GetColumnSpec(right);
            return string.Join(" ", l, op, r);
        }

        private void ValidateSymbol(string name, Type t)
        {
            if (!this.symbols.ContainsKey(name))
            {
                throw new Exception("Unknown symbol: " + name);
            }

            if (this.symbols[name] != t)
            {
                throw new Exception(string.Format("Type didn't match: {0} is a {1}, not a {2}.", name, this.symbols[name].Name, t.Name));
            }
        }

        protected string GetColumnSpec<T, V>(Func<T, V> f)
            where T : BoundObject, new()
        {
            var tableAlias = f.Method.GetParameters().First().Name;
            ValidateSymbol(tableAlias, typeof(T));
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
