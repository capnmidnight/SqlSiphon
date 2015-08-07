using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSiphon.TestBase;
using SqlSiphon.Mapping;

namespace SqlSiphon.SqlServer.Test
{
    public class SqlServerTestQueries : TestQueries
    {
        public SqlServerTestQueries() : base() { }

        [Routine(CommandType = System.Data.CommandType.StoredProcedure, Query = @"insert into whatever(bleh) select Value from @values;")]
        public void TestUploadableSimpleArrayType(int[] values)
        {
            this.Execute(values);
        }

        [Routine(CommandType = System.Data.CommandType.StoredProcedure, 
Query = @"insert into TestUploadable(BareField, ExcludedFieldWithDefaultValue)
select BareField, ExcludedFieldWithDefaultValue
from @values;")]
        public void TestUploadableComplexArrayType(TestUploadableClass[] values)
        {
            this.Execute(values);
        }

        [Routine(CommandType = System.Data.CommandType.StoredProcedure,
Query = @"insert into TestUploadable(BareField, ExcludedFieldWithDefaultValue)
select BareField, ExcludedFieldWithDefaultValue
from @value;")]
        public void TestUploadableComplexType(TestUploadableClass value)
        {
            this.Execute(value);
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
            var script = this.GetScript();
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
            var script = this.GetScript();
        }

        [TestMethod]
        public void TestUploadableSimpleArrayType()
        {
            var script = this.GetScript();
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
            var script = this.GetScript();
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
            var script = this.GetScript();
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
