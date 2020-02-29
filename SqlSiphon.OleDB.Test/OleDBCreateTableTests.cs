using NUnit.Framework;

using SqlSiphon.TestBase;


namespace SqlSiphon.OleDB.Test
{
    [TestFixture]
    public class OleDBCreateTableTests : CreateTableTests
    {
        private const string TEST_FILE_NAME = "CreateTableTest.mdb";

        protected override ISqlSiphon MakeConnector()
        {
            var db = new OleDBDataAccessLayer(TEST_FILE_NAME);
            db.Disposed += db_Disposed;
            return db;
        }

        private void db_Disposed(object sender, System.EventArgs e)
        {
            System.Diagnostics.Process.Start("cmd", "/C del " + TEST_FILE_NAME);
        }

        [TestCase]
        public override void CantCreateEmptyTables()
        {
            Assert.Throws<TableHasNoColumnsException>(() =>
            {
                GetScriptFor<EmptyTable>();
            });
        }

        [TestCase]
        public override void CreateSingleColumnTable()
        {
            var script = GetScriptFor<OneColumnTable>();
            Assert.AreEqual(
@"create table [OneColumnTable](
    [ColumnA] int NOT NULL
);", script);
        }

        [TestCase]
        public override void CreateSingleColumnTableWithSchema()
        {
            var script = GetScriptFor<OneColumnTableWithSchema>();
            Assert.AreEqual(
@"create table [test_OneColumnTableWithSchema](
    [ColumnA] int NOT NULL
);", script);
        }

        [TestCase]
        public override void CreateTwoColumnTable()
        {
            var script = GetScriptFor<TwoColumnTable>();
            Assert.AreEqual(
@"create table [TwoColumnTable](
    [ColumnA] int NOT NULL,
    [ColumnB] int NOT NULL
);", script);
        }

        [TestCase]
        public override void CreateTwoColumnTableAsChild()
        {
            var script = GetScriptFor<TwoColumnTableAsChild>();
            Assert.AreEqual(
@"create table [TwoColumnTableAsChild](
    [ColumnA] int NOT NULL,
    [ColumnB] int NOT NULL
);", script);
        }

        [TestCase]
        public override void CreateOneNullableColumn()
        {
            var script = GetScriptFor<OneNullableColumnTable>();
            Assert.AreEqual(
@"create table [OneNullableColumnTable](
    [ColumnA] int
);", script);
        }

        [TestCase]
        public override void CantCreatePKWithMAXString()
        {
            Assert.Throws<MustSetStringSizeInPrimaryKeyException>(() =>
            {
                GetScriptFor<LongStringPrimaryKeyTable>();
            });
        }

        [TestCase]
        public override void CreateWithPK()
        {
            var script = GetScriptFor<PrimaryKeyColumnTable>();
            Assert.AreEqual(
@"create table [PrimaryKeyColumnTable](
    [KeyColumn] text,
    [DateColumn] datetime NOT NULL
);
--
create index [pk_PrimaryKeyColumnTable] on [PrimaryKeyColumnTable]([KeyColumn]) with primary;", script);
        }

        [TestCase]
        public override void CreateLongerPrimaryKey()
        {
            var script = GetScriptFor<PrimaryKeyTwoColumnsTable>();
            Assert.AreEqual(@"create table [PrimaryKeyTwoColumnsTable](
    [KeyColumn1] text,
    [KeyColumn2] datetime
);
--
create index [pk_PrimaryKeyTwoColumnsTable] on [PrimaryKeyTwoColumnsTable]([KeyColumn1], [KeyColumn2]) with primary;", script);
        }

        [TestCase]
        public override void CantCreateNullablePK()
        {
            Assert.Throws<PrimaryKeyColumnNotNullableException>(() =>
            {
                GetScriptFor<NullablePrimaryKeyTable>();
            });
        }

        [TestCase]
        public override void CreateWithIdentity()
        {
            var script = GetScriptFor<IdentityColumnTable>();
            Assert.AreEqual(
@"create table [IdentityColumnTable](
    [KeyColumn] autoincrement,
    [DateColumn] datetime NOT NULL
);
--
create index [pk_IdentityColumnTable] on [IdentityColumnTable]([KeyColumn]) with primary;", script);
        }

        [TestCase]
        public override void CreateTableFromEnumeration()
        {
            var script = GetScriptFor<EnumerationTable>();
            Assert.AreEqual(
@"create table [EnumerationTable](
    [Value] int,
    [Description] text NOT NULL
);
--
create index [pk_EnumerationTable] on [EnumerationTable]([Value]) with primary;
--
insert into [EnumerationTable]([Value], [Description]) values(0, 'A');
insert into [EnumerationTable]([Value], [Description]) values(1, 'B');
insert into [EnumerationTable]([Value], [Description]) values(2, 'C');
insert into [EnumerationTable]([Value], [Description]) values(3, 'D');
insert into [EnumerationTable]([Value], [Description]) values(4, 'EFG');
insert into [EnumerationTable]([Value], [Description]) values(5, 'HIJ');
insert into [EnumerationTable]([Value], [Description]) values(6, 'KLM');
insert into [EnumerationTable]([Value], [Description]) values(7, 'OpQ');
insert into [EnumerationTable]([Value], [Description]) values(8, 'rSt');", script);
        }

        [TestCase]
        public override void CreateTableWithSimpleIndex()
        {
            var script = GetScriptFor<SimpleIndexTable>();
            Assert.AreEqual(
@"create table [SimpleIndexTable](
    [KeyColumn] autoincrement,
    [NotInIndex] float NOT NULL,
    [DoubleColumn] double NOT NULL
);
--
create index [pk_SimpleIndexTable] on [SimpleIndexTable]([KeyColumn]) with primary;
--
create index [idx_Test1] on [SimpleIndexTable]([DoubleColumn]);", script);
        }

        [TestCase]
        public override void CreateTableWithLongIndex()
        {
            var script = GetScriptFor<LongIndexTable>();
            Assert.AreEqual(
@"create table [LongIndexTable](
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
create index [pk_LongIndexTable] on [LongIndexTable]([KeyColumn]) with primary;
--
create index [idx_Test2] on [LongIndexTable]([DoubleColumn], [IntColumn], [BoolColumn]);
create index [idx_Test3] on [LongIndexTable]([LongColumn], [DecimalColumn], [CharColumn]);", script);
        }

        [TestCase]
        public override void CreateTableWithFK()
        {
            var script = GetScriptFor<FKTable>();
            Assert.AreEqual(
@"create table [FKTable](
    [Stuff] int,
    [KeyColumn] text NOT NULL
);
--
create index [pk_FKTable] on [FKTable]([Stuff]) with primary;
--
alter table [FKTable] add foreign key([KeyColumn]) references [PrimaryKeyColumnTable]([KeyColumn]);
--
create index [idx_fk_from_FKTable_to_pk_PrimaryKeyColumnTable] on [FKTable]([KeyColumn]);", script);
        }

        [TestCase]
        public override void CreateTableWithLongFK()
        {
            var script = GetScriptFor<LongFKTable>();
            Assert.AreEqual(
@"create table [LongFKTable](
    [Stuff] int,
    [KeyColumn1] text NOT NULL,
    [KeyColumn2] datetime NOT NULL
);
--
create index [pk_LongFKTable] on [LongFKTable]([Stuff]) with primary;
--
alter table [LongFKTable] add foreign key([KeyColumn1], [KeyColumn2]) references [PrimaryKeyTwoColumnsTable]([KeyColumn1], [KeyColumn2]);
--
create index [idx_fk_from_LongFKTable_to_pk_PrimaryKeyTwoColumnsTable] on [LongFKTable]([KeyColumn1], [KeyColumn2]);", script);
        }
    }
}
