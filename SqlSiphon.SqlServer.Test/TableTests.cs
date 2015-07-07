using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSiphon;
using SqlSiphon.Mapping;
using SqlSiphon.SqlServer;
using SqlSiphon.TestBase;

namespace SqlSiphon.SqlServer.Test
{
    [TestClass]
    public class TableTests : ScriptConstructionTest<SqlServerDataConnectorFactory>
    {
        [TestMethod, ExpectedException(typeof(TableHasNoColumnsException))]
        public override void CantCreateEmptyTables()
        {
            var script = GetScriptFor<TestEmptyTable>();
        }

        [TestMethod]
        public override void CreateSingleColumnTable()
        {
            var script = GetScriptFor<TestOneColumnTable>();
            Assert.AreEqual(@"create table [dbo].[TestOneColumnTable](
    ColumnA int NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateSingleColumnTableWithSchema()
        {
            var script = GetScriptFor<TestOneColumnTableWithSchema>();
            Assert.AreEqual(@"create table [test].[TestOneColumnTableWithSchema](
    ColumnA int NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateTwoColumnTable()
        {
            var script = GetScriptFor<TestTwoColumnTable>();
            Assert.AreEqual(@"create table [dbo].[TestTwoColumnTable](
    ColumnA int NOT NULL,
    ColumnB int NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateTwoColumnTableAsChild()
        {
            var script = GetScriptFor<TestTwoColumnTableAsChild>();
            Assert.AreEqual(@"create table [dbo].[TestTwoColumnTableAsChild](
    ColumnA int NOT NULL,
    ColumnB int NOT NULL
);", script);
        }
    }
}
