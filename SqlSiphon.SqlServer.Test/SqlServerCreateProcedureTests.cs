using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSiphon.TestBase;

namespace SqlSiphon.SqlServer.Test
{
    [TestClass]
    public class SqlServerCreateProcedureTests : CreateProcedureTests
    {
        protected override ISqlSiphon MakeConnector()
        {
            return new SqlServerDataAccessLayer((string)null);
        }

        [TestMethod]
        public void EmptyStoredProcedure()
        {
            var script = this.GetScript();
            Assert.AreEqual(
@"create procedure [dbo].[EmptyStoredProcedure]
    
as begin
    set nocount on;
    -- nothing here
end", script);
        }
    }
}
