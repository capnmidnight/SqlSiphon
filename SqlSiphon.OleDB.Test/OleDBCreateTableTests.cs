using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSiphon.TestBase;


namespace SqlSiphon.OleDB.Test
{
    [TestClass]
    public class OleDBCreateTableTests : CreateTableTests
    {
        private const string TEST_FILE_NAME = "CreateTableTest.mdb";

        protected override ISqlSiphon MakeConnector()
        {
            var db = new OleDBDataAccessLayer(TEST_FILE_NAME);
            db.Disposed += db_Disposed;
            return db;
        }

        void db_Disposed(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("cmd", "/C del " + TEST_FILE_NAME);
        }

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
@"create table [TestOneColumnTable](
    [ColumnA] int NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateSingleColumnTableWithSchema()
        {
            var script = GetScriptFor<TestOneColumnTableWithSchema>();
            Assert.AreEqual(
@"create table [test_TestOneColumnTableWithSchema](
    [ColumnA] int NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateTwoColumnTable()
        {
            var script = GetScriptFor<TestTwoColumnTable>();
            Assert.AreEqual(
@"create table [TestTwoColumnTable](
    [ColumnA] int NOT NULL,
    [ColumnB] int NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateTwoColumnTableAsChild()
        {
            var script = GetScriptFor<TestTwoColumnTableAsChild>();
            Assert.AreEqual(
@"create table [TestTwoColumnTableAsChild](
    [ColumnA] int NOT NULL,
    [ColumnB] int NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateOneNullableColumn()
        {
            var script = GetScriptFor<TestOneNullableColumnTable>();
            Assert.AreEqual(
@"create table [TestOneNullableColumnTable](
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
@"create table [TestPrimaryKeyColumn](
    [KeyColumn] text,
    [DateColumn] datetime NOT NULL
);
--
create index [pk_TestPrimaryKeyColumn] on [TestPrimaryKeyColumn]([KeyColumn]) with primary;", script);
        }

        [TestMethod]
        public override void CreateLongerPrimaryKey()
        {
            var script = GetScriptFor<TestPrimaryKeyTwoColumns>();
            Assert.AreEqual(@"create table [TestPrimaryKeyTwoColumns](
    [KeyColumn1] text,
    [KeyColumn2] datetime
);
--
create index [pk_TestPrimaryKeyTwoColumns] on [TestPrimaryKeyTwoColumns]([KeyColumn1], [KeyColumn2]) with primary;", script);
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
@"create table [TestIdentityColumn](
    [KeyColumn] autoincrement,
    [DateColumn] datetime NOT NULL
);
--
create index [pk_TestIdentityColumn] on [TestIdentityColumn]([KeyColumn]) with primary;", script);
        }

        [TestMethod]
        public override void CreateTableFromEnumeration()
        {
            var script = GetScriptFor<TestEnumeration>();
            Assert.AreEqual(
@"create table [TestEnumeration](
    [Value] int,
    [Description] text NOT NULL
);
--
create index [pk_TestEnumeration] on [TestEnumeration]([Value]) with primary;
--
insert into [TestEnumeration]([Value], [Description]) values(0, 'A');
insert into [TestEnumeration]([Value], [Description]) values(1, 'B');
insert into [TestEnumeration]([Value], [Description]) values(2, 'C');
insert into [TestEnumeration]([Value], [Description]) values(3, 'D');
insert into [TestEnumeration]([Value], [Description]) values(4, 'EFG');
insert into [TestEnumeration]([Value], [Description]) values(5, 'HIJ');
insert into [TestEnumeration]([Value], [Description]) values(6, 'KLM');
insert into [TestEnumeration]([Value], [Description]) values(7, 'OpQ');
insert into [TestEnumeration]([Value], [Description]) values(8, 'rSt');", script);
        }

        [TestMethod]
        public override void CreateTableWithSimpleIndex()
        {
            var script = GetScriptFor<TestTableWithSimpleIndex>();
            Assert.AreEqual(
@"create table [TestTableWithSimpleIndex](
    [KeyColumn] autoincrement,
    [NotInIndex] float NOT NULL,
    [DoubleColumn] double NOT NULL
);
--
create index [pk_TestTableWithSimpleIndex] on [TestTableWithSimpleIndex]([KeyColumn]) with primary;
--
create index [idx_Test1] on [TestTableWithSimpleIndex]([DoubleColumn]);", script);
        }

        [TestMethod]
        public override void CreateTableWithLongIndex()
        {
            var script = GetScriptFor<TestTableWithLongIndex>();
            Assert.AreEqual(
@"create table [TestTableWithLongIndex](
    [KeyColumn] autoincrement,
    [NotInIndex] float NOT NULL,
    [DoubleColumn] double NOT NULL,
    [IntColumn] int NOT NULL,
    [ByteColumn] byte NOT NULL,
    [BoolColumn] bit NOT NULL,
    [LongColumn] int NOT NULL,
    [DecimalColumn] currency NOT NULL,
    [CharColumn] char NOT NULL
);
--
create index [pk_TestTableWithLongIndex] on [TestTableWithLongIndex]([KeyColumn]) with primary;
--
create index [idx_Test2] on [TestTableWithLongIndex]([DoubleColumn], [IntColumn], [BoolColumn]);
create index [idx_Test3] on [TestTableWithLongIndex]([LongColumn], [DecimalColumn], [CharColumn]);", script);
        }

        [TestMethod]
        public override void CreateTableWithFK()
        {
            var script = GetScriptFor<TestWithFK>();
            Assert.AreEqual(
@"create table [TestWithFK](
    [Stuff] int,
    [KeyColumn] text NOT NULL
);
--
create index [pk_TestWithFK] on [TestWithFK]([Stuff]) with primary;
--
alter table [TestWithFK] add foreign key([KeyColumn]) references [TestPrimaryKeyColumn]([KeyColumn]);
--
create index [idx_fk_from_TestWithFK_to_pk_TestPrimaryKeyColumn] on [TestWithFK]([KeyColumn]);", script);
        }
    }
}
