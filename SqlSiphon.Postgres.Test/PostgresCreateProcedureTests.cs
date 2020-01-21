using Microsoft.VisualStudio.TestTools.UnitTesting;

using SqlSiphon.TestBase;

namespace SqlSiphon.Postgres.Test
{
    [TestClass]
    public class PostgresCreateProcedureTests : CreateProcedureTests
    {
        protected override ISqlSiphon MakeConnector()
        {
            return new PostgresDataAccessLayer((string)null);
        }

        [TestMethod]
        public override void EmptyStoredProcedure()
        {
            var script = GetScript();
            Assert.AreEqual(
@"create function ""public"".""emptystoredprocedure""()
returns void 
as $$
begin
-- nothing here
end;
$$
language plpgsql;", script);
        }

        [TestMethod]
        public override void GetEmptyTable()
        {
            var script = GetScript();
            Assert.IsNotNull(script);
        }
    }
}
