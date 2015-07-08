using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSiphon;
using SqlSiphon.Mapping;
using SqlSiphon.Postgres;
using SqlSiphon.TestBase;

namespace SqlSiphon.SqlServer.Test
{
    [TestClass]
    public class SqlServerCreateTableTests : CreateTableTests<PostgresDataConnectorFactory>
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
@"create table ""public"".""testonecolumntable"" (
    ""columna"" integer NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateSingleColumnTableWithSchema()
        {
            var script = GetScriptFor<TestOneColumnTableWithSchema>();
            Assert.AreEqual(
@"create table ""test"".""testonecolumntablewithschema"" (
    ""columna"" integer NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateTwoColumnTable()
        {
            var script = GetScriptFor<TestTwoColumnTable>();
            Assert.AreEqual(
@"create table ""public"".""testtwocolumntable"" (
    ""columna"" integer NOT NULL,
    ""columnb"" integer NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateTwoColumnTableAsChild()
        {
            var script = GetScriptFor<TestTwoColumnTableAsChild>();
            Assert.AreEqual(
@"create table ""public"".""testtwocolumntableaschild"" (
    ""columna"" integer NOT NULL,
    ""columnb"" integer NOT NULL
);", script);
        }

        [TestMethod]
        public override void CreateOneNullableColumn()
        {
            var script = GetScriptFor<TestOneNullableColumnTable>();
            Assert.AreEqual(
@"create table ""public"".""testonenullablecolumntable"" (
    ""columna"" integer NULL
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
@"create table ""public"".""testprimarykeycolumn"" (
    ""keycolumn"" varchar(255) NOT NULL,
    ""datecolumn"" date NOT NULL
);
--
create unique index ""idx_pk_testprimarykeycolumn"" on ""public"".""testprimarykeycolumn"" (""keycolumn"");
alter table ""public"".""testprimarykeycolumn"" add constraint ""pk_testprimarykeycolumn"" primary key using index ""idx_pk_testprimarykeycolumn"";", script);
        }

        [TestMethod]
        public override void CreateLongerPrimaryKey()
        {
            var script = GetScriptFor<TestPrimaryKeyTwoColumns>();
            Assert.AreEqual(@"create table ""public"".""testprimarykeytwocolumns"" (
    ""keycolumn1"" varchar(255) NOT NULL,
    ""keycolumn2"" date NOT NULL
);
--
create unique index ""idx_pk_testprimarykeytwocolumns"" on ""public"".""testprimarykeytwocolumns"" (""keycolumn1"", ""keycolumn2"");
alter table ""public"".""testprimarykeytwocolumns"" add constraint ""pk_testprimarykeytwocolumns"" primary key using index ""idx_pk_testprimarykeytwocolumns"";", script);
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
@"create table ""public"".""testidentitycolumn"" (
    ""keycolumn"" serial NOT NULL,
    ""datecolumn"" date NOT NULL
);
--
create unique index ""idx_pk_testidentitycolumn"" on ""public"".""testidentitycolumn"" (""keycolumn"");
alter table ""public"".""testidentitycolumn"" add constraint ""pk_testidentitycolumn"" primary key using index ""idx_pk_testidentitycolumn"";", script);
        }

        [TestMethod]
        public override void CreateTableFromEnumeration()
        {
            var script = GetScriptFor<TestEnumeration>();
            Assert.AreEqual(
@"create table ""public"".""testenumeration"" (
    ""value"" integer NOT NULL,
    ""description"" text NOT NULL
);
--
create unique index ""idx_pk_testenumeration"" on ""public"".""testenumeration"" (""value"");
alter table ""public"".""testenumeration"" add constraint ""pk_testenumeration"" primary key using index ""idx_pk_testenumeration"";
--
insert into ""public"".""testenumeration""(""value"", ""description"") values(0, 'A');
insert into ""public"".""testenumeration""(""value"", ""description"") values(1, 'B');
insert into ""public"".""testenumeration""(""value"", ""description"") values(2, 'C');
insert into ""public"".""testenumeration""(""value"", ""description"") values(3, 'D');
insert into ""public"".""testenumeration""(""value"", ""description"") values(4, 'EFG');
insert into ""public"".""testenumeration""(""value"", ""description"") values(5, 'HIJ');
insert into ""public"".""testenumeration""(""value"", ""description"") values(6, 'KLM');
insert into ""public"".""testenumeration""(""value"", ""description"") values(7, 'OpQ');
insert into ""public"".""testenumeration""(""value"", ""description"") values(8, 'rSt');", script);
        }

        [TestMethod]
        public override void CreateTableWithSimpleIndex()
        {
            var script = GetScriptFor<TestTableWithSimpleIndex>();
            Assert.AreEqual(
@"create table ""public"".""testtablewithsimpleindex"" (
    ""keycolumn"" serial NOT NULL,
    ""notinindex"" real NOT NULL,
    ""floatcolumn"" double precision NOT NULL
);
--
create unique index ""idx_pk_testtablewithsimpleindex"" on ""public"".""testtablewithsimpleindex"" (""keycolumn"");
alter table ""public"".""testtablewithsimpleindex"" add constraint ""pk_testtablewithsimpleindex"" primary key using index ""idx_pk_testtablewithsimpleindex"";
--
create index ""idx_test1"" on ""public"".""testtablewithsimpleindex""(""floatcolumn"");", script);
        }

        [TestMethod]
        public override void CreateTableWithLongIndex()
        {
            var script = GetScriptFor<TestTableWithLongIndex>();
            Assert.AreEqual(
@"create table ""public"".""testtablewithlongindex"" (
    ""keycolumn"" serial NOT NULL,
    ""notinindex"" real NOT NULL,
    ""floatcolumn"" double precision NOT NULL,
    ""intcolumn"" integer NOT NULL,
    ""bytecolumn"" byte NOT NULL,
    ""boolcolumn"" boolean NOT NULL,
    ""longcolumn"" bigint NOT NULL,
    ""decimalcolumn"" money NOT NULL,
    ""charcolumn"" byte NOT NULL
);
--
create unique index ""idx_pk_testtablewithlongindex"" on ""public"".""testtablewithlongindex"" (""keycolumn"");
alter table ""public"".""testtablewithlongindex"" add constraint ""pk_testtablewithlongindex"" primary key using index ""idx_pk_testtablewithlongindex"";
--
create index ""idx_test2"" on ""public"".""testtablewithlongindex""(""floatcolumn"",""intcolumn"",""boolcolumn"");
create index ""idx_test3"" on ""public"".""testtablewithlongindex""(""longcolumn"",""decimalcolumn"",""charcolumn"");", script);
        }

        [TestMethod]
        public override void CreateTableWithFK()
        {
            var script = GetScriptFor<TestWithFK>();
            Assert.AreEqual(
@"create table ""public"".""testwithfk"" (
    ""stuff"" integer NOT NULL,
    ""keycolumn"" varchar(255) NOT NULL
);
--
create unique index ""idx_pk_testwithfk"" on ""public"".""testwithfk"" (""stuff"");
alter table ""public"".""testwithfk"" add constraint ""pk_testwithfk"" primary key using index ""idx_pk_testwithfk"";
--
alter table ""public"".""testwithfk"" add constraint ""fk_from_public_testwithfk_to_pk_testprimarykeycolumn""
    foreign key(""keycolumn"")
    references ""public"".""testprimarykeycolumn""(""keycolumn"");
--
create index ""idx_fk_from_public_testwithfk_to_pk_testprimarykeycolumn"" on ""public"".""testwithfk""(""keycolumn"");", script);
        }
    }
}
