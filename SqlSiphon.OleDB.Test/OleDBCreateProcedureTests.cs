using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSiphon.TestBase;

namespace SqlSiphon.OleDB.Test
{
    [TestClass]
    public class OleDBCreateProcedureTests : CreateProcedureTests
    {
        protected override ISqlSiphon MakeConnector()
        {
            return new OleDBDataAccessLayer("Test.mbd");
        }

        [TestMethod]
        public override void EmptyStoredProcedure()
        {
            var script = this.GetScript();
            Assert.AreEqual(@"create procedure [EmptyStoredProcedure] as -- nothing here", script);
        }

        [TestMethod]
        public override void GetEmptyTable() 
        {
            var script = this.GetScript();
        }
    }
}
