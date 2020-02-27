using System;
using NUnit.Framework;
using SqlSiphon.TestBase;

namespace SqlSiphon.OleDB.Test
{
    [TestFixture]
    public class OleDBCreateProcedureTests : CreateProcedureTests
    {
        protected override ISqlSiphon MakeConnector()
        {
            return new OleDBDataAccessLayer("Test.mbd");
        }

        [TestCase]
        public override void EmptyStoredProcedure()
        {
            var script = this.GetScript();
            Assert.AreEqual(@"create procedure [EmptyStoredProcedure] as -- nothing here", script);
        }

        [TestCase]
        public override void GetEmptyTable() 
        {
            Assert.Throws<TableHasNoColumnsException>(() =>
            {
                GetScript();
            });
        }
    }
}
