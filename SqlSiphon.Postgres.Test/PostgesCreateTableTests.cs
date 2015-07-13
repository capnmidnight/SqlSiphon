using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSiphon;
using SqlSiphon.Mapping;
using SqlSiphon.Postgres;
using SqlSiphon.TestBase;

namespace SqlSiphon.Postgres.Test
{
    [TestClass]
    public class PostgresCreateTableTests : CreateTableTests
    {
        protected override ISqlSiphon MakeConnector()
        {
            return new PostgresDataAccessLayer((string)null);
        }
        [TestMethod]
        public override void CantCreateEmptyTables()
        {
            GetScriptFor<EmptyTable>();
        }

        [TestMethod]
        public override void CreateSingleColumnTable()
        {
            var script = GetScriptFor<OneColumnTable>();
            Assert.AreEqual(
@"create table ""public"".""onecolumntable"" (
    ""columna"" integer NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateSingleColumnTableWithSchema()
        {
            var script = GetScriptFor<OneColumnTableWithSchema>();
            Assert.AreEqual(
@"create table ""test"".""onecolumntablewithschema"" (
    ""columna"" integer NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateTwoColumnTable()
        {
            var script = GetScriptFor<TwoColumnTable>();
            Assert.AreEqual(
@"create table ""public"".""twocolumntable"" (
    ""columna"" integer NOT NULL,
    ""columnb"" integer NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateTwoColumnTableAsChild()
        {
            var script = GetScriptFor<TwoColumnTableAsChild>();
            Assert.AreEqual(
@"create table ""public"".""twocolumntableaschild"" (
    ""columna"" integer NOT NULL,
    ""columnb"" integer NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateOneNullableColumn()
        {
            var script = GetScriptFor<OneNullableColumnTable>();
            Assert.AreEqual(
@"create table ""public"".""onenullablecolumntable"" (
    ""columna"" integer NULL
);", script);
        }

        [TestMethod]
        public override void CantCreatePKWithMAXString()
        {
            GetScriptFor<LongStringPrimaryKeyTable>();
        }

        [TestMethod]
        public override void CreateWithPK()
        {
            var script = GetScriptFor<PrimaryKeyColumnTable>();
            Assert.AreEqual(
@"create table ""public"".""primarykeycolumntable"" (
    ""keycolumn"" varchar(255) NOT NULL,
    ""datecolumn"" date NOT NULL
);
--
create unique index ""idx_pk_primarykeycolumntable"" on ""public"".""primarykeycolumntable"" (""keycolumn"");
alter table ""public"".""primarykeycolumntable"" add constraint ""pk_primarykeycolumntable"" primary key using index ""idx_pk_primarykeycolumntable"";", script);
        }

        [TestMethod]
        public override void CreateLongerPrimaryKey()
        {
            var script = GetScriptFor<PrimaryKeyTwoColumnsTable>();
            Assert.AreEqual(@"create table ""public"".""primarykeytwocolumnstable"" (
    ""keycolumn1"" varchar(255) NOT NULL,
    ""keycolumn2"" date NOT NULL
);
--
create unique index ""idx_pk_primarykeytwocolumnstable"" on ""public"".""primarykeytwocolumnstable"" (""keycolumn1"", ""keycolumn2"");
alter table ""public"".""primarykeytwocolumnstable"" add constraint ""pk_primarykeytwocolumnstable"" primary key using index ""idx_pk_primarykeytwocolumnstable"";", script);
        }

        [TestMethod]
        public override void CantCreateNullablePK()
        {
            GetScriptFor<NullablePrimaryKeyTable>();
        }

        [TestMethod]
        public override void CreateWithIdentity()
        {
            var script = GetScriptFor<IdentityColumnTable>();
            Assert.AreEqual(
@"create table ""public"".""identitycolumntable"" (
    ""keycolumn"" serial NOT NULL,
    ""datecolumn"" date NOT NULL
);
--
create unique index ""idx_pk_identitycolumntable"" on ""public"".""identitycolumntable"" (""keycolumn"");
alter table ""public"".""identitycolumntable"" add constraint ""pk_identitycolumntable"" primary key using index ""idx_pk_identitycolumntable"";", script);
        }

        [TestMethod]
        public override void CreateTableFromEnumeration()
        {
            var script = GetScriptFor<EnumerationTable>();
            Assert.AreEqual(
@"create table ""public"".""enumerationtable"" (
    ""value"" integer NOT NULL,
    ""description"" text NOT NULL
);
--
create unique index ""idx_pk_enumerationtable"" on ""public"".""enumerationtable"" (""value"");
alter table ""public"".""enumerationtable"" add constraint ""pk_enumerationtable"" primary key using index ""idx_pk_enumerationtable"";
--
insert into ""public"".""enumerationtable""(""value"", ""description"") values(0, 'A');
insert into ""public"".""enumerationtable""(""value"", ""description"") values(1, 'B');
insert into ""public"".""enumerationtable""(""value"", ""description"") values(2, 'C');
insert into ""public"".""enumerationtable""(""value"", ""description"") values(3, 'D');
insert into ""public"".""enumerationtable""(""value"", ""description"") values(4, 'EFG');
insert into ""public"".""enumerationtable""(""value"", ""description"") values(5, 'HIJ');
insert into ""public"".""enumerationtable""(""value"", ""description"") values(6, 'KLM');
insert into ""public"".""enumerationtable""(""value"", ""description"") values(7, 'OpQ');
insert into ""public"".""enumerationtable""(""value"", ""description"") values(8, 'rSt');", script);
        }

        [TestMethod]
        public override void CreateTableWithSimpleIndex()
        {
            var script = GetScriptFor<SimpleIndexTable>();
            Assert.AreEqual(
@"create table ""public"".""simpleindextable"" (
    ""keycolumn"" serial NOT NULL,
    ""notinindex"" real NOT NULL,
    ""doublecolumn"" double precision NOT NULL
);
--
create unique index ""idx_pk_simpleindextable"" on ""public"".""simpleindextable"" (""keycolumn"");
alter table ""public"".""simpleindextable"" add constraint ""pk_simpleindextable"" primary key using index ""idx_pk_simpleindextable"";
--
create index ""idx_test1"" on ""public"".""simpleindextable""(""doublecolumn"");", script);
        }

        [TestMethod]
        public override void CreateTableWithLongIndex()
        {
            var script = GetScriptFor<LongIndexTable>();
            Assert.AreEqual(
@"create table ""public"".""longindextable"" (
    ""keycolumn"" serial NOT NULL,
    ""notinindex"" real NOT NULL,
    ""doublecolumn"" double precision NOT NULL,
    ""intcolumn"" integer NOT NULL,
    ""bytecolumn"" smallint NOT NULL,
    ""boolcolumn"" boolean NOT NULL,
    ""longcolumn"" bigint NOT NULL,
    ""decimalcolumn"" money NOT NULL,
    ""charcolumn"" smallint NOT NULL
);
--
create unique index ""idx_pk_longindextable"" on ""public"".""longindextable"" (""keycolumn"");
alter table ""public"".""longindextable"" add constraint ""pk_longindextable"" primary key using index ""idx_pk_longindextable"";
--
create index ""idx_test2"" on ""public"".""longindextable""(""doublecolumn"",""intcolumn"",""boolcolumn"");
create index ""idx_test3"" on ""public"".""longindextable""(""longcolumn"",""decimalcolumn"",""charcolumn"");", script);
        }

        [TestMethod]
        public override void CreateTableWithFK()
        {
            var script = GetScriptFor<FKTable>();
            Assert.AreEqual(
@"create table ""public"".""fktable"" (
    ""stuff"" integer NOT NULL,
    ""keycolumn"" varchar(255) NOT NULL
);
--
create unique index ""idx_pk_fktable"" on ""public"".""fktable"" (""stuff"");
alter table ""public"".""fktable"" add constraint ""pk_fktable"" primary key using index ""idx_pk_fktable"";
--
alter table ""public"".""fktable"" add constraint ""fk_from_public_fktable_to_pk_primarykeycolumntable""
    foreign key(""keycolumn"")
    references ""public"".""primarykeycolumntable""(""keycolumn"");
--
create index ""idx_fk_from_public_fktable_to_pk_primarykeycolumntable"" on ""public"".""fktable""(""keycolumn"");", script);
        }

        [TestMethod]
        public override void CreateTableWithLongFK()
        {
            var script = GetScriptFor<LongFKTable>();
            Assert.AreEqual(
@"create table ""public"".""longfktable"" (
    ""stuff"" integer NOT NULL,
    ""keycolumn1"" varchar(255) NOT NULL,
    ""keycolumn2"" date NOT NULL
);
--
create unique index ""idx_pk_longfktable"" on ""public"".""longfktable"" (""stuff"");
alter table ""public"".""longfktable"" add constraint ""pk_longfktable"" primary key using index ""idx_pk_longfktable"";
--
alter table ""public"".""longfktable"" add constraint ""fk_from_public_longfktable_to_pk_primarykeytwocolumnstable""
    foreign key(""keycolumn1"", ""keycolumn2"")
    references ""public"".""primarykeytwocolumnstable""(""keycolumn1"", ""keycolumn2"");
--
create index ""idx_fk_from_public_longfktable_to_pk_primarykeytwocolumnstable"" on ""public"".""longfktable""(""keycolumn1"",""keycolumn2"");", script);
        }
    }
}
