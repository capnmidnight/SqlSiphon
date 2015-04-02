﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Examples.SqlServer.Runner
{
    public static class From
    {
        public static Query<T> Table<T>(string alias = null) where T : BoundObject, new()
        {
            return new Query<T>(alias);
        }
    }

    public class Query<T> where T : BoundObject, new()
    {
        private string alias;

        public Query(string alias)
        {
            this.alias = alias;
        }

        public FromClauseBuilder InnerJoin<U, V>(string alias, Func<T, V> left, Ops op, Func<U, V> right)
            where U : BoundObject, new()
        {
            var spec = new FromClauseBuilder(typeof(T), this.alias);
            return spec.InnerJoin(alias, left, op, right);
        }

        public FromClauseBuilder InnerJoin<U, V>(Func<T, V> left, Ops op, Func<U, V> right)
            where U : BoundObject, new()
        {
            return this.InnerJoin(null, left, op, right);
        }

        public override string ToString()
        {
            return string.Format("from {0} as {1}", typeof(T).Name, this.alias);
        }
    }
}