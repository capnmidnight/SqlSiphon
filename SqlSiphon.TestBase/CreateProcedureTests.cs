using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [TestFixture]
    public abstract class CreateProcedureTests<QueryDefT> where QueryDefT : TestQueries, new()
    {
        private static readonly Dictionary<string, MethodInfo> QueryDefs = typeof(QueryDefT)
            .GetMethods()
            .ToDictionary(m => m.Name);

        protected abstract ISqlSiphon MakeConnector();

        protected string GetScript()
        {
            var t = typeof(QueryDefT);
            var method = new System.Diagnostics.StackTrace(1)
                .GetFrames()
                .Select(frame => frame.GetMethod().Name)
                .Where(QueryDefs.ContainsKey)
                .Select(name => QueryDefs[name])
                .FirstOrDefault();

            if(method is null)
            {
                throw new EntryPointNotFoundException("Couldn't find a method to test");
            }

            var methodInfo = DatabaseObjectAttribute.GetAttribute(method);

            // We aren't opening a connection, we're just trying to generate scripts
            // so it shouldn't be a problem to provide no connection string.
            using var ss = MakeConnector();
            return ss.MakeCreateRoutineScript(methodInfo);
        }

        public abstract void GetEmptyTable();

        public abstract void EmptyStoredProcedure();
    }

    [TestFixture]
    public abstract class CreateProcedureTests : CreateProcedureTests<TestQueries>
    {

    }
}
