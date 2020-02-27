using NUnit.Framework;

using SqlSiphon.TestBase;

namespace SqlSiphon.SqlServer.Test
{
    [TestFixture]
    public class SqlServerCreateTableTests : CreateTableTests
    {
        protected override ISqlSiphon MakeConnector()
        {
            return new SqlServerDataAccessLayer((string)null);
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
@"create table [dbo].[OneColumnTable](
    [ColumnA] int NOT NULL
);", script);
        }

        [TestCase]
        public override void CreateSingleColumnTableWithSchema()
        {
            var script = GetScriptFor<OneColumnTableWithSchema>();
            Assert.AreEqual(
@"create table [test].[OneColumnTableWithSchema](
    [ColumnA] int NOT NULL
);", script);
        }

        [TestCase]
        public override void CreateTwoColumnTable()
        {
            var script = GetScriptFor<TwoColumnTable>();
            Assert.AreEqual(
@"create table [dbo].[TwoColumnTable](
    [ColumnA] int NOT NULL,
    [ColumnB] int NOT NULL
);", script);
        }

        [TestCase]
        public override void CreateTwoColumnTableAsChild()
        {
            var script = GetScriptFor<TwoColumnTableAsChild>();
            Assert.AreEqual(
@"create table [dbo].[TwoColumnTableAsChild](
    [ColumnA] int NOT NULL,
    [ColumnB] int NOT NULL
);", script);
        }

        [TestCase]
        public override void CreateOneNullableColumn()
        {
            var script = GetScriptFor<OneNullableColumnTable>();
            Assert.AreEqual(
@"create table [dbo].[OneNullableColumnTable](
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
@"create table [dbo].[PrimaryKeyColumnTable](
    [KeyColumn] nvarchar(255) NOT NULL,
    [DateColumn] datetime2 NOT NULL
);
--
alter table [dbo].[PrimaryKeyColumnTable] add constraint [pk_PrimaryKeyColumnTable] primary key([KeyColumn]);", script);
        }

        [TestCase]
        public override void CreateLongerPrimaryKey()
        {
            var script = GetScriptFor<PrimaryKeyTwoColumnsTable>();
            Assert.AreEqual(@"create table [dbo].[PrimaryKeyTwoColumnsTable](
    [KeyColumn1] nvarchar(255) NOT NULL,
    [KeyColumn2] datetime2 NOT NULL
);
--
alter table [dbo].[PrimaryKeyTwoColumnsTable] add constraint [pk_PrimaryKeyTwoColumnsTable] primary key([KeyColumn1], [KeyColumn2]);", script);
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
@"create table [dbo].[IdentityColumnTable](
    [KeyColumn] int NOT NULL identity(1, 1),
    [DateColumn] datetime2 NOT NULL
);
--
alter table [dbo].[IdentityColumnTable] add constraint [pk_IdentityColumnTable] primary key([KeyColumn]);", script);
        }

        [TestCase]
        public override void CreateTableFromEnumeration()
        {
            var script = GetScriptFor<EnumerationTable>();
            Assert.AreEqual(
@"create table [dbo].[EnumerationTable](
    [Value] int NOT NULL,
    [Description] nvarchar(MAX) NOT NULL
);
--
alter table [dbo].[EnumerationTable] add constraint [pk_EnumerationTable] primary key([Value]);
--
insert into [dbo].[EnumerationTable](Value, Description) values(0, 'A');
insert into [dbo].[EnumerationTable](Value, Description) values(1, 'B');
insert into [dbo].[EnumerationTable](Value, Description) values(2, 'C');
insert into [dbo].[EnumerationTable](Value, Description) values(3, 'D');
insert into [dbo].[EnumerationTable](Value, Description) values(4, 'EFG');
insert into [dbo].[EnumerationTable](Value, Description) values(5, 'HIJ');
insert into [dbo].[EnumerationTable](Value, Description) values(6, 'KLM');
insert into [dbo].[EnumerationTable](Value, Description) values(7, 'OpQ');
insert into [dbo].[EnumerationTable](Value, Description) values(8, 'rSt');", script);
        }

        [TestCase]
        public override void CreateTableWithSimpleIndex()
        {
            var script = GetScriptFor<SimpleIndexTable>();
            Assert.AreEqual(
@"create table [dbo].[SimpleIndexTable](
    [KeyColumn] int NOT NULL identity(1, 1),
    [NotInIndex] real NOT NULL,
    [DoubleColumn] float NOT NULL
);
--
alter table [dbo].[SimpleIndexTable] add constraint [pk_SimpleIndexTable] primary key([KeyColumn]);
--
create nonclustered index [idx_Test1] on [dbo].[SimpleIndexTable]([DoubleColumn]);", script);
        }

        [TestCase]
        public override void CreateTableWithLongIndex()
        {
            var script = GetScriptFor<LongIndexTable>();
            Assert.AreEqual(
@"create table [dbo].[LongIndexTable](
    [KeyColumn] int NOT NULL identity(1, 1),
    [NotInIndex] real NOT NULL,
    [DoubleColumn] float NOT NULL,
    [IntColumn] int NOT NULL,
    [ByteColumn] tinyint NOT NULL,
    [BoolColumn] bit NOT NULL,
    [LongColumn] bigint NOT NULL,
    [DecimalColumn] decimal NOT NULL,
    [CharColumn] smallint NOT NULL
);
--
alter table [dbo].[LongIndexTable] add constraint [pk_LongIndexTable] primary key([KeyColumn]);
--
create nonclustered index [idx_Test2] on [dbo].[LongIndexTable]([DoubleColumn],[IntColumn],[BoolColumn]);
create nonclustered index [idx_Test3] on [dbo].[LongIndexTable]([LongColumn],[DecimalColumn],[CharColumn]);", script);
        }

        [TestCase]
        public override void CreateTableWithFK()
        {
            var script = GetScriptFor<FKTable>();
            Assert.AreEqual(
@"create table [dbo].[FKTable](
    [Stuff] int NOT NULL,
    [KeyColumn] nvarchar(255) NOT NULL
);
--
alter table [dbo].[FKTable] add constraint [pk_FKTable] primary key([Stuff]);
--
alter table [dbo].[FKTable] add constraint [fk_from_dbo_FKTable_to_pk_PrimaryKeyColumnTable]
    foreign key([KeyColumn])
    references [dbo].[PrimaryKeyColumnTable]([KeyColumn]);
--
create nonclustered index [idx_fk_from_dbo_FKTable_to_pk_PrimaryKeyColumnTable] on [dbo].[FKTable]([KeyColumn]);", script);
        }

        [TestCase]
        public override void CreateTableWithLongFK()
        {
            var script = GetScriptFor<LongFKTable>();
            Assert.AreEqual(
@"create table [dbo].[LongFKTable](
    [Stuff] int NOT NULL,
    [KeyColumn1] nvarchar(255) NOT NULL,
    [KeyColumn2] datetime2 NOT NULL
);
--
alter table [dbo].[LongFKTable] add constraint [pk_LongFKTable] primary key([Stuff]);
--
alter table [dbo].[LongFKTable] add constraint [fk_from_dbo_LongFKTable_to_pk_PrimaryKeyTwoColumnsTable]
    foreign key([KeyColumn1], [KeyColumn2])
    references [dbo].[PrimaryKeyTwoColumnsTable]([KeyColumn1], [KeyColumn2]);
--
create nonclustered index [idx_fk_from_dbo_LongFKTable_to_pk_PrimaryKeyTwoColumnsTable] on [dbo].[LongFKTable]([KeyColumn1],[KeyColumn2]);", script);
        }
    }
}
