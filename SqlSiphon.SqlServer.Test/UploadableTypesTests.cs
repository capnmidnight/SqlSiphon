using System;
using System.Linq;

using NUnit.Framework;

using SqlSiphon.Mapping;

namespace SqlSiphon.SqlServer.Test
{
    [Uploadable]
    public class TestUploadableClass
    {
        public int BareField { get; set; }

        [Exclude]
        public int ExcludedField { get; set; }

        [Column(DefaultValue = "1337")]
        public int ExcludedFieldWithDefaultValue { get; set; }

        [Column(DefaultValue = "1337", Include = true)]
        public int FieldWithDefaultValue { get; set; }

        [AutoPK]
        public int ExcludedIdentityField { get; set; }

        [AutoPK(Include = true)]
        public int IncludedIdentityField { get; set; }
    }

    [TestFixture]
    public class UploadableTypesTests
    {
        private bool FieldTest(string fieldName)
        {
            var t = typeof(TestUploadableClass);
            var m = t.GetMember(fieldName).FirstOrDefault();
            if (m == null)
            {
                throw new Exception("Field " + fieldName + " doesn't exist");
            }
            var attr = DatabaseObjectAttribute.GetAttribute(t);
            using var testDB = new SqlServerDataAccessLayer((string)null);
            var script = testDB.MakeCreateUDTTScript(attr);
            return script.IndexOf(fieldName, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        [TestCase]
        public void BasicIntegerArrayUDTT()
        {
            using var testDB = new SqlServerDataAccessLayer((string)null);
            var table = testDB.MakeUDTTTableAttribute(typeof(int[]));
            var script = testDB.MakeCreateUDTTScript(table);
            Assert.AreEqual(@"create type [dbo].[Int32UDTT] as table(
    [Value] int NOT NULL
);", script);
        }

        [TestCase]
        public void BasicFloatArrayUDTT()
        {
            using var testDB = new SqlServerDataAccessLayer((string)null);
            var table = testDB.MakeUDTTTableAttribute(typeof(float[]));
            var script = testDB.MakeCreateUDTTScript(table);
            Assert.AreEqual(@"create type [dbo].[SingleUDTT] as table(
    [Value] real NOT NULL
);", script);
        }

        [TestCase]
        public void BasicDoubleArrayUDTT()
        {
            using var testDB = new SqlServerDataAccessLayer((string)null);
            var table = testDB.MakeUDTTTableAttribute(typeof(double[]));
            var script = testDB.MakeCreateUDTTScript(table);
            Assert.AreEqual(@"create type [dbo].[DoubleUDTT] as table(
    [Value] float NOT NULL
);", script);
        }

        [TestCase]
        public void BasicStringArrayUDTT()
        {
            using var testDB = new SqlServerDataAccessLayer((string)null);
            var table = testDB.MakeUDTTTableAttribute(typeof(string[]));
            var script = testDB.MakeCreateUDTTScript(table);
            Assert.AreEqual(@"create type [dbo].[StringUDTT] as table(
    [Value] nvarchar(MAX) NOT NULL
);", script);
        }

        [TestCase]
        public void TableTypeUDTT()
        {
            using var testDB = new SqlServerDataAccessLayer((string)null);
            var table = testDB.MakeUDTTTableAttribute(typeof(TestUploadableClass));
            var script = testDB.MakeCreateUDTTScript(table);
            Assert.AreEqual(@"create type [dbo].[TestUploadableClassUDTT] as table(
    [BareField] int NOT NULL,
    [FieldWithDefaultValue] int NOT NULL default (1337)
);", script);
        }

        [TestCase]
        public void TableTypeArrayUDTT()
        {
            using var testDB = new SqlServerDataAccessLayer((string)null);
            var table = testDB.MakeUDTTTableAttribute(typeof(TestUploadableClass[]));
            var script = testDB.MakeCreateUDTTScript(table);
            Assert.AreEqual(@"create type [dbo].[TestUploadableClassUDTT] as table(
    [BareField] int NOT NULL,
    [FieldWithDefaultValue] int NOT NULL default (1337)
);", script);
        }

        [TestCase]
        public void BareFieldsIncluded()
        {
            Assert.IsTrue(FieldTest("BareField"));
        }

        [TestCase]
        public void FieldsAreExcluded()
        {
            Assert.IsFalse(FieldTest("ExcludedField"));
        }

        [TestCase]
        public void FieldsWithDefaultValuesAreExcluded()
        {
            Assert.IsFalse(FieldTest("ExcludedFieldWithDefaultValue"));
        }

        [TestCase]
        public void FieldsWithDefaultValuesExplicitlyIncludedAreIncluded()
        {
            Assert.IsTrue(FieldTest("FieldWithDefaultValue"));
        }

        [TestCase]
        public void IdentityFieldExcluded()
        {
            Assert.IsFalse(FieldTest("ExcludedIdentityField"));
        }

        [TestCase]
        public void CantForceIdentityFieldToInclude()
        {
            Assert.IsFalse(FieldTest("IncludedIdentityField"));
        }
    }
}
