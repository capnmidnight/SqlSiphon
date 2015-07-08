using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSiphon;
using SqlSiphon.Mapping;
using SqlSiphon.SqlServer;
using SqlSiphon.TestBase;

namespace SqlSiphon.SqlServer.Test
{
    [TestClass]
    public class SqlServerCreateTableTests : CreateTableTests<SqlServerDataConnectorFactory>
    {
        [TestMethod]
        public override void CantCreateEmptyTables()
        {
            GetScriptFor<TestEmptyTable>();
        }

        [TestMethod]
        public override void CreateSingleColumnTable()
        {
            var script = GetScriptFor<TestOneColumnTable>();
            Assert.AreEqual(
@"create table [dbo].[TestOneColumnTable](
    [ColumnA] int NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateSingleColumnTableWithSchema()
        {
            var script = GetScriptFor<TestOneColumnTableWithSchema>();
            Assert.AreEqual(
@"create table [test].[TestOneColumnTableWithSchema](
    [ColumnA] int NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateTwoColumnTable()
        {
            var script = GetScriptFor<TestTwoColumnTable>();
            Assert.AreEqual(
@"create table [dbo].[TestTwoColumnTable](
    [ColumnA] int NOT NULL,
    [ColumnB] int NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateTwoColumnTableAsChild()
        {
            var script = GetScriptFor<TestTwoColumnTableAsChild>();
            Assert.AreEqual(
@"create table [dbo].[TestTwoColumnTableAsChild](
    [ColumnA] int NOT NULL,
    [ColumnB] int NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateOneNullableColumn()
        {
            var script = GetScriptFor<TestOneNullableColumnTable>();
            Assert.AreEqual(
@"create table [dbo].[TestOneNullableColumnTable](
    [ColumnA] int
);", script);
        }

        [TestMethod]
        public override void CantCreatePKWithMAXString()
        {
            GetScriptFor<TestLongStringPrimaryKey>();
        }

        [TestMethod]
        public override void CreateWithPK()
        {
            var script = GetScriptFor<TestPrimaryKeyColumn>();
            Assert.AreEqual(
@"create table [dbo].[TestPrimaryKeyColumn](
    [KeyColumn] nvarchar(255) NOT NULL,
    [DateColumn] datetime2 NOT NULL
);
--
alter table [dbo].[TestPrimaryKeyColumn] add constraint [pk_TestPrimaryKeyColumn] primary key([KeyColumn]);", script);
        }

        [TestMethod]
        public override void CreateLongerPrimaryKey()
        {
            var script = GetScriptFor<TestPrimaryKeyTwoColumns>();
            Assert.AreEqual(@"create table [dbo].[TestPrimaryKeyTwoColumns](
    [KeyColumn1] nvarchar(255) NOT NULL,
    [KeyColumn2] datetime2 NOT NULL
);
--
alter table [dbo].[TestPrimaryKeyTwoColumns] add constraint [pk_TestPrimaryKeyTwoColumns] primary key([KeyColumn1], [KeyColumn2]);", script);
        }

        [TestMethod]
        public override void CantCreateNullablePK()
        {
            GetScriptFor<TestNullablePrimaryKey>();
        }

        [TestMethod]
        public override void CreateWithIdentity()
        {
            var script = GetScriptFor<TestIdentityColumn>();
            Assert.AreEqual(
@"create table [dbo].[TestIdentityColumn](
    [KeyColumn] int NOT NULL identity(1, 1),
    [DateColumn] datetime2 NOT NULL
);
--
alter table [dbo].[TestIdentityColumn] add constraint [pk_TestIdentityColumn] primary key([KeyColumn]);", script);
        }

        [TestMethod]
        public override void CreateTableFromEnumeration()
        {
            var script = GetScriptFor<TestEnumeration>();
            Assert.AreEqual(
@"create table [dbo].[TestEnumeration](
    [Value] int NOT NULL,
    [Description] nvarchar(MAX) NOT NULL
);
--
alter table [dbo].[TestEnumeration] add constraint [pk_TestEnumeration] primary key([Value]);
--
insert into [dbo].[TestEnumeration](Value, Description) values(0, 'A');
insert into [dbo].[TestEnumeration](Value, Description) values(1, 'B');
insert into [dbo].[TestEnumeration](Value, Description) values(2, 'C');
insert into [dbo].[TestEnumeration](Value, Description) values(3, 'D');
insert into [dbo].[TestEnumeration](Value, Description) values(4, 'EFG');
insert into [dbo].[TestEnumeration](Value, Description) values(5, 'HIJ');
insert into [dbo].[TestEnumeration](Value, Description) values(6, 'KLM');
insert into [dbo].[TestEnumeration](Value, Description) values(7, 'OpQ');
insert into [dbo].[TestEnumeration](Value, Description) values(8, 'rSt');", script);
        }

        [TestMethod]
        public override void CreateTableWithSimpleIndex()
        {
            var script = GetScriptFor<TestTableWithSimpleIndex>();
            Assert.AreEqual(
@"create table [dbo].[TestTableWithSimpleIndex](
    [KeyColumn] int NOT NULL identity(1, 1),
    [NotInIndex] real NOT NULL,
    [FloatColumn] float NOT NULL
);
--
alter table [dbo].[TestTableWithSimpleIndex] add constraint [pk_TestTableWithSimpleIndex] primary key([KeyColumn]);
--
create nonclustered index [idx_Test1] on [dbo].[TestTableWithSimpleIndex]([FloatColumn]);", script);
        }

        [TestMethod]
        public override void CreateTableWithLongIndex()
        {
            var script = GetScriptFor<TestTableWithLongIndex>();
            Assert.AreEqual(
@"create table [dbo].[TestTableWithLongIndex](
    [KeyColumn] int NOT NULL identity(1, 1),
    [NotInIndex] real NOT NULL,
    [FloatColumn] float NOT NULL,
    [IntColumn] int NOT NULL,
    [ByteColumn] tinyint NOT NULL,
    [BoolColumn] bit NOT NULL,
    [LongColumn] bigint NOT NULL,
    [DecimalColumn] decimal NOT NULL,
    [CharColumn] tinyint NOT NULL
);
--
alter table [dbo].[TestTableWithLongIndex] add constraint [pk_TestTableWithLongIndex] primary key([KeyColumn]);
--
create nonclustered index [idx_Test2] on [dbo].[TestTableWithLongIndex]([FloatColumn],[IntColumn],[BoolColumn]);
create nonclustered index [idx_Test3] on [dbo].[TestTableWithLongIndex]([LongColumn],[DecimalColumn],[CharColumn]);", script);
        }

        [TestMethod]
        public override void CreateTableWithFK()
        {
            var script = GetScriptFor<TestWithFK>();
            Assert.AreEqual(
@"create table [dbo].[TestWithFK](
    [Stuff] int NOT NULL,
    [KeyColumn] nvarchar(255) NOT NULL
);
--
alter table [dbo].[TestWithFK] add constraint [pk_TestWithFK] primary key([Stuff]);
--
alter table [dbo].[TestWithFK] add constraint [fk_from_dbo_TestWithFK_to_pk_TestPrimaryKeyColumn]
    foreign key([KeyColumn])
    references [dbo].[TestPrimaryKeyColumn]([KeyColumn])
--
create nonclustered index [idx_fk_from_dbo_TestWithFK_to_pk_TestPrimaryKeyColumn] on [dbo].[TestWithFK]([KeyColumn]);", script);
        }
    }
}
