using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSiphon;
using SqlSiphon.Mapping;
using SqlSiphon.SqlServer;

namespace SqlSiphon.SqlServer.Test
{
    [TestClass]
    public class TableTests
    {

        private string GetScriptFor<T>()
        {
            // We aren't opening a connection, we're just trying to generate scripts
            // so it shouldn't be a problem to provide no connection string.
            using (var ss = new SqlServerDataAccessLayer((string)null))
            {
                var t = typeof(T);
                var a = DatabaseObjectAttribute.GetAttribute(t);
                return ss.MakeCreateTableScript(a);
            }
        }

        [Table]
        class TestEmptyTable { }

        [TestMethod, ExpectedException(typeof(TableHasNoColumnsException))]
        public void CantCreateEmptyTables()
        {
            var script = GetScriptFor<TestEmptyTable>();
        }

        [Table]
        class TestOneColumnTable
        {
            public int ColumnA { get; set; }
        }

        [TestMethod]
        public void CreateSingleColumnTable()
        {
            var script = GetScriptFor<TestOneColumnTable>();
            Assert.AreEqual(@"create table [dbo].[TestOneColumnTable](
    ColumnA int NOT NULL
);", script);
        }

        [Table(Schema = "test")]
        class TestOneColumnTableWithSchema
        {
            public int ColumnA { get; set; }
        }

        [TestMethod]
        public void CreateSingleColumnTableWithSchema()
        {
            var script = GetScriptFor<TestOneColumnTableWithSchema>();
            Assert.AreEqual(@"create table [test].[TestOneColumnTableWithSchema](
    ColumnA int NOT NULL
);", script);
        }

        [Table]
        class TestTwoColumnTable
        {
            public int ColumnA { get; set; }
            public int ColumnB { get; set; }
        }

        [TestMethod]
        public void CreateTwoColumnTable()
        {
            var script = GetScriptFor<TestTwoColumnTable>();
            Assert.AreEqual(@"create table [dbo].[TestTwoColumnTable](
    ColumnA int NOT NULL,
    ColumnB int NOT NULL
);", script);
        }

        [Table]
        class TestTwoColumnTableAsChild : TestOneColumnTable
        {
            public int ColumnB { get; set; }
        }

        [TestMethod]
        public void CreateTwoColumnTableAsChild()
        {
            var script = GetScriptFor<TestTwoColumnTableAsChild>();
            Assert.AreEqual(@"create table [dbo].[TestTwoColumnTableAsChild](
    ColumnA int NOT NULL,
    ColumnB int NOT NULL
);", script);
        }
    }
}
