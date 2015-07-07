using System;
using SqlSiphon;
using SqlSiphon.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSiphon.TestBase
{
    [TestClass]
    public abstract class ScriptConstructionTest<DataConnectorFactoryType>
        where DataConnectorFactoryType : IDataConnectorFactory, new()
    {
        private DataConnectorFactoryType factory;
        public ScriptConstructionTest()
        {
            factory = new DataConnectorFactoryType();
        }

        protected string GetScriptFor<T>()
        {
            // We aren't opening a connection, we're just trying to generate scripts
            // so it shouldn't be a problem to provide no connection string.
            using (var ss = (ISqlSiphon)this.factory.MakeConnector((string)null))
            {
                var t = typeof(T);
                var a = DatabaseObjectAttribute.GetAttribute(t);
                return ss.MakeCreateTableScript(a);
            }
        }


        [TestMethod, ExpectedException(typeof(TableHasNoColumnsException))]
        public abstract void CantCreateEmptyTables();

        [TestMethod]
        public abstract void CreateSingleColumnTable();

        [TestMethod]
        public abstract void CreateSingleColumnTableWithSchema();

        [TestMethod]
        public abstract void CreateTwoColumnTable();

        [TestMethod]
        public abstract void CreateTwoColumnTableAsChild();
    }
}
