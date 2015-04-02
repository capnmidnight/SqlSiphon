using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Examples.SqlServer.Runner
{
    public class SelectClauseBuilder : ClauseBuilder<SelectClauseBuilder>
    {
        private string from;
        public SelectClauseBuilder(Dictionary<string, Type> tables, string from, string firstColumn, string alias = null)
            : base(tables)
        {
            this.from = from;
            this.AddColumn("select \n\t", firstColumn, alias);
        }

        public SelectClauseBuilder _<T, V>(Func<T, V> column, string alias = null)
            where T : BoundObject, new()
        {
            return this.AddColumn(",\n\t", GetColumnSpec(column), alias);
        }

        public SelectClauseBuilder Max<T, V>(Func<T, V> column, string alias = null)
            where T : BoundObject, new()
        {
            return this.AddColumn(",\n\tmax(", GetColumnSpec(column), alias, ")");
        }

        public SelectClauseBuilder Min<T, V>(Func<T, V> column, string alias = null)
            where T : BoundObject, new()
        {
            return this.AddColumn(",\n\tmin(", GetColumnSpec(column), alias, ")");
        }

        public WhereClauseBuilder Where<T, V>(Func<T, V> column, Ops op, V value)
            where T : BoundObject, new()
        {
            return new WhereClauseBuilder(this.tables, this.query.ToString(), this.from, this.GetExpression(column, op, value));
        }

        private SelectClauseBuilder AddColumn(string pre, string column, string alias, string post = null)
        {
            this.query.AppendFormat("{0}{1}", pre, column);
            if (post != null)
            {
                this.query.Append(post);
            }
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
}
