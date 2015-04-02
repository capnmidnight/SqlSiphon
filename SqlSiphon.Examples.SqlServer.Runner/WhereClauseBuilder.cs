using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Examples.SqlServer.Runner
{
    public class WhereClauseBuilder : LogicClauseBuilder<WhereClauseBuilder>
    {
        private string select, from;
        public WhereClauseBuilder(Dictionary<string, Type> tables, string select, string from, string firstExpression)
            : base(tables)
        {
            this.select = select;
            this.from = from;
            this.query.AppendFormat("where\n\t{0}", firstExpression);
        }

        public override string ToString()
        {
            return string.Format("{0}\n{1}\n{2}", this.select, this.from, this.query.ToString());
        }
    }
}
