using Microsoft.VisualStudio.TestTools.UnitTesting;

using SqlSiphon.Mapping;
using SqlSiphon.TestBase;

namespace SqlSiphon.SqlServer.Test
{
    public class SqlServerTestQueries : TestQueries
    {
        public SqlServerTestQueries() : base() { }

        [Routine(CommandType = System.Data.CommandType.StoredProcedure, Query = @"insert into whatever(bleh) select Value from @values;")]
        public void TestUploadableSimpleArrayType(int[] values)
        {
            Execute(values);
        }

        [Routine(CommandType = System.Data.CommandType.StoredProcedure,
Query = @"insert into TestUploadable(BareField, ExcludedFieldWithDefaultValue)
select BareField, ExcludedFieldWithDefaultValue
from @values;")]
        public void TestUploadableComplexArrayType(TestUploadableClass[] values)
        {
            Execute(values);
        }

        [Routine(CommandType = System.Data.CommandType.StoredProcedure,
Query = @"insert into TestUploadable(BareField, ExcludedFieldWithDefaultValue)
select BareField, ExcludedFieldWithDefaultValue
from @value;")]
        public void TestUploadableComplexType(TestUploadableClass value)
        {
            Execute(value);
        }
    }

    [TestClass]
    public class SqlServerCreateProcedureTests : CreateProcedureTests<SqlServerTestQueries>
    {
        protected override ISqlSiphon MakeConnector()
        {
            return new SqlServerDataAccessLayer((string)null);
        }

        [TestMethod]
        public override void EmptyStoredProcedure()
        {
            var script = GetScript();
            Assert.AreEqual(
@"create procedure [dbo].[EmptyStoredProcedure]
    
as begin
    set nocount on;
    -- nothing here
end", script);
        }

        [TestMethod]
        public override void GetEmptyTable()
        {
            var script = GetScript();
            Assert.IsNotNull(script);
        }

        [TestMethod]
        public void TestUploadableSimpleArrayType()
        {
            var script = GetScript();
            Assert.AreEqual(@"create procedure [dbo].[TestUploadableSimpleArrayType]
    @values Int32UDTT  readonly
as begin
    set nocount on;
    insert into whatever(bleh) select Value from @values;
end", script);
        }

        [TestMethod]
        public void TestUploadableComplexArrayType()
        {
            var script = GetScript();
            Assert.AreEqual(@"create procedure [dbo].[TestUploadableComplexArrayType]
    @values TestUploadableClassUDTT  readonly
as begin
    set nocount on;
    insert into TestUploadable(BareField, ExcludedFieldWithDefaultValue)
select BareField, ExcludedFieldWithDefaultValue
from @values;
end", script);
        }

        [TestMethod]
        public void TestUploadableComplexType()
        {
            var script = GetScript();
            Assert.AreEqual(@"create procedure [dbo].[TestUploadableComplexType]
    @value TestUploadableClassUDTT  readonly
as begin
    set nocount on;
    insert into TestUploadable(BareField, ExcludedFieldWithDefaultValue)
select BareField, ExcludedFieldWithDefaultValue
from @value;
end", script);
        }
    }
}
