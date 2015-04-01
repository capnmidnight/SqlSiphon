using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon
{
    [Mapping.Table]
    public class MyTable : BoundObject
    {
        public int MyNumber { get { return Get<int>(); } set { Set(value); } }
        public string MyWord { get { return Get<string>(); } set { Set(value); } }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(
                From<MyTable>("a")
                .InnerJoin<MyTable, int>("b", a => a.MyNumber, LessThan, b => b.MyNumber)
                .And<MyTable, MyTable, string>(a => a.MyWord, Equal, b => b.MyWord)
                .Or<MyTable, MyTable, int>(a => a.MyNumber, GreaterThan, b => b.MyNumber)
                .Select<MyTable, int>(a => a.MyNumber)
                ._<MyTable, string>(a => a.MyWord)
                ._<MyTable, int>(b => b.MyNumber)
                ._<MyTable, string>(b => b.MyWord));
        }
        public static Ops Equal = new Ops("=");
        public static Ops NotEqual = new Ops("!=");
        public static Ops Like = new Ops("like");
        public static Ops LessThan = new Ops("<");
        public static Ops LessThanEqual = new Ops("<=");
        public static Ops GreaterThan = new Ops(">");
        public static Ops GreaterThanEqual = new Ops(">=");
        public class Ops
        {
            private string token;
            internal Ops(string token) { this.token = token; }
            public override string ToString() { return this.token; }
        }

        public static NamedTable<T> From<T>(string alias)
                where T : BoundObject, new()
        {
            return new NamedTable<T>(alias);
        }

        interface IFromSpec
        {
            FromClauseBuilder InnerJoin<T, U, V>(string alias, Func<T, V> left, Ops op, Func<U, V> right)
                where T : BoundObject, new()
                where U : BoundObject, new();
        }

        interface IFromSpec<T> : IFromSpec
                where T : BoundObject, new()
        {
            FromClauseBuilder InnerJoin<U, V>(string alias, Func<T, V> left, Ops op, Func<U, V> right)
                where U : BoundObject, new();
        }

        public class NamedTable<T> : IFromSpec<T>
                where T : BoundObject, new()
        {
            private string alias;

            public NamedTable(string alias)
            {
                this.alias = alias;
            }

            public FromClauseBuilder InnerJoin<U, V>(string alias, Func<T, V> left, Ops op, Func<U, V> right)
                where U : BoundObject, new()
            {
                var spec = new FromClauseBuilder(typeof(T), this.alias);
                return spec.InnerJoin(alias, left, op, right);
            }

            FromClauseBuilder IFromSpec.InnerJoin<T2, U, V>(string alias, Func<T2, V> left, Ops op, Func<U, V> right)
            {
                var spec = new FromClauseBuilder(typeof(T2), this.alias);
                return spec.InnerJoin(alias, left, op, right);
            }

            public override string ToString()
            {
                return string.Format("from {0} as {1}", typeof(T).Name, this.alias);
            }
        }

        public abstract class ClauseBuilder
        {
            protected StringBuilder query;
            protected Dictionary<string, Type> tables;

            protected ClauseBuilder(Dictionary<string, Type> tables)
            {
                this.query = new StringBuilder();
                this.tables = tables;
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

        public class FromClauseBuilder : ClauseBuilder, IFromSpec
        {
            public FromClauseBuilder(Type t, string alias)
                : base(new Dictionary<string, Type>())
            {
                this.query.AppendFormat("from {0} as {1}", t.Name, alias);
                this.AddTable(t, alias);
            }

            public override string ToString()
            {
                return this.query.ToString();
            }

            private void AddTable(Type t, string alias)
            {
                var name = alias ?? t.Name;
                if (!this.tables.ContainsKey(name))
                {
                    this.tables.Add(name, t);
                }
                else
                {

                }
            }

            private FromClauseBuilder Combine<T, U, V>(Func<T, V> left, Ops op, Func<U, V> right)
                where T : BoundObject, new()
                where U : BoundObject, new()
            {
                var l = GetColumnSpec(left);
                var r = GetColumnSpec(right);
                this.query.AppendFormat("{0} {1} {2}", l, op, r);
                return this;
            }

            public FromClauseBuilder InnerJoin<T, U, V>(string alias, Func<T, V> left, Ops op, Func<U, V> right)
                where T : BoundObject, new()
                where U : BoundObject, new()
            {
                this.AddTable(typeof(U), alias);
                this.query.AppendFormat("\ninner join {0} as {1} on ", typeof(U).Name, alias);
                return this.Combine(left, op, right);
            }

            public FromClauseBuilder And<T, U, V>(Func<T, V> left, Ops op, Func<U, V> right)
                where T : BoundObject, new()
                where U : BoundObject, new()
            {
                return this.Combine<T, U, V>("and", left, op, right);
            }

            public FromClauseBuilder Or<T, U, V>(Func<T, V> left, Ops op, Func<U, V> right)
                where T : BoundObject, new()
                where U : BoundObject, new()
            {
                return this.Combine<T, U, V>("or", left, op, right);
            }

            private FromClauseBuilder Combine<T, U, V>(string pre, Func<T, V> left, Ops op, Func<U, V> right)
                where T : BoundObject, new()
                where U : BoundObject, new()
            {
                this.query.AppendFormat("\n{0} ", pre);
                return this.Combine(left, op, right);
            }

            public SelectClauseBuilder Select<T, V>(Func<T, V> column, string alias = null)
                where T : BoundObject, new()
            {
                return new SelectClauseBuilder(this.tables, this.ToString(), GetColumnSpec(column), alias);
            }
        }

        public class SelectClauseBuilder : ClauseBuilder
        {
            private string from;
            public SelectClauseBuilder(Dictionary<string, Type> tables, string from, string firstColumn, string alias = null)
                : base(tables)
            {
                this.from = from;
                this.AddColumn("select ", firstColumn, alias);
            }

            public SelectClauseBuilder _<T, V>(Func<T, V> column, string alias = null)
                where T : BoundObject, new()
            {
                return this.AddColumn(",\n", GetColumnSpec(column), alias);
            }

            private SelectClauseBuilder AddColumn(string pre, string column, string alias)
            {
                this.query.AppendFormat("{0}{1}", pre, column);
                if (alias != null)
                {
                    this.query.AppendFormat(" as {0}", alias);
                }
                return this;
            }

            public override string ToString()
            {
                return string.Format("{0}\n{1}", this.query.ToString(), this.from);
            }
        }

        public class WhereClauseBuilder
        {
            public WhereClauseBuilder(string select, string from)
            {

            }
        }
    }
}
