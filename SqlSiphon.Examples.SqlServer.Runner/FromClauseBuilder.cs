using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Examples.SqlServer.Runner
{
    public class FromClauseBuilder : LogicClauseBuilder<FromClauseBuilder>
    {
        public FromClauseBuilder(Type t, string alias = null)
            : base(new Dictionary<string, Type>())
        {
            this.query.AppendFormat("from\n\t{0}", t.Name);
            if (alias != null)
            {
                this.query.AppendFormat(" as {0}", alias);
            }
            this.AddTable(t, alias);
        }

        public override string ToString()
        {
            return this.query.ToString();
        }

        private void AddTable(Type t, string alias)
        {
            var name = alias ?? t.Name;
            if (this.tables.ContainsKey(name))
            {
                throw new Exception("can't join in that name again: " + name);
            }
            else{
                this.tables.Add(name, t);
            }
        }

        public FromClauseBuilder InnerJoin<T, U, V>(string alias, Func<T, V> left, Ops op, Func<U, V> right)
            where T : BoundObject, new()
            where U : BoundObject, new()
        {
            this.AddTable(typeof(U), alias);
            this.query.AppendFormat("\n\tinner join {0} as {1}\n\t\ton ", typeof(U).Name, alias);
            return this.Combine(left, op, right);
        }

        public SelectClauseBuilder Select<T, V>(Func<T, V> column, string alias = null)
            where T : BoundObject, new()
        {
            return new SelectClauseBuilder(this.tables, this.ToString(), GetColumnSpec(column), alias);
        }
    }
}
