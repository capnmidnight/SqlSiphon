using System;
using System.Collections.Generic;
using SqlSiphon;
using SqlSiphon.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSiphon.TestBase
{
    [TestClass]
    public abstract class CreateProcedureTests<QueryDefT> where QueryDefT : TestQueries, new()
    {
        protected abstract ISqlSiphon MakeConnector();

        protected string GetScript()
        {
            var methodName = new System.Diagnostics.StackTrace(1)
                .GetFrame(0)
                .GetMethod()
                .Name;
            // We aren't opening a connection, we're just trying to generate scripts
            // so it shouldn't be a problem to provide no connection string.
            using (var ss = this.MakeConnector())
            {
                var dc = new QueryDefT();
                dc.Connection = ss;
                var t = dc.GetType();
                var method = t.GetMethod(methodName);
                var methodInfo = DatabaseObjectAttribute.GetAttribute(method);
                return ss.MakeCreateRoutineScript(methodInfo);
            }
        }

        [ExpectedException(typeof(TableHasNoColumnsException))]
        public abstract void GetEmptyTable();

        public abstract void EmptyStoredProcedure();
    }

    [TestClass]
    public abstract class CreateProcedureTests : CreateProcedureTests<TestQueries>
    {

    }
}
