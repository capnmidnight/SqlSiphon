using NUnit.Framework;

using SqlSiphon.TestBase;

namespace SqlSiphon.Postgres.Test
{
    [TestFixture]
    public class PostgresCreateProcedureTests : CreateProcedureTests
    {
        protected override ISqlSiphon MakeConnector()
        {
            return new PostgresDataAccessLayer((string)null);
        }

        [TestCase]
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
