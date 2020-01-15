using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

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

    [TestClass]
    public class UploadableTypesTests
    {
        private readonly SqlServerDataAccessLayer testDB = new SqlServerDataAccessLayer((string)null);

        private bool FieldTest(string fieldName)
        {
            var t = typeof(TestUploadableClass);
            var m = t.GetMember(fieldName).FirstOrDefault();
            if (m == null)
            {
                throw new Exception("Field " + fieldName + " doesn't exist");
            }
            var attr = DatabaseObjectAttribute.GetAttribute(t);
            var script = testDB.MakeCreateUDTTScript(attr);
            return script.IndexOf(fieldName) > -1;
        }

        [TestMethod]
        public void BasicIntegerArrayUDTT()
        {
            var table = testDB.MakeUDTTTableAttribute(typeof(int[]));
            var script = testDB.MakeCreateUDTTScript(table);
            Assert.AreEqual(@"create type [dbo].[Int32UDTT] as table(
    [Value] int NOT NULL
);", script);
        }

        [TestMethod]
        public void BasicFloatArrayUDTT()
        {
            var table = testDB.MakeUDTTTableAttribute(typeof(float[]));
            var script = testDB.MakeCreateUDTTScript(table);
            Assert.AreEqual(@"create type [dbo].[SingleUDTT] as table(
    [Value] real NOT NULL
);", script);
        }

        [TestMethod]
        public void BasicDoubleArrayUDTT()
        {
            var table = testDB.MakeUDTTTableAttribute(typeof(double[]));
            var script = testDB.MakeCreateUDTTScript(table);
            Assert.AreEqual(@"create type [dbo].[DoubleUDTT] as table(
    [Value] float NOT NULL
);", script);
        }

        [TestMethod]
        public void BasicStringArrayUDTT()
        {
            var table = testDB.MakeUDTTTableAttribute(typeof(string[]));
            var script = testDB.MakeCreateUDTTScript(table);
            Assert.AreEqual(@"create type [dbo].[StringUDTT] as table(
    [Value] nvarchar(MAX) NOT NULL
);", script);
        }

        [TestMethod]
        public void TableTypeUDTT()
        {
            var table = testDB.MakeUDTTTableAttribute(typeof(TestUploadableClass));
            var script = testDB.MakeCreateUDTTScript(table);
            Assert.AreEqual(@"create type [dbo].[TestUploadableClassUDTT] as table(
    [BareField] int NOT NULL,
    [FieldWithDefaultValue] int NOT NULL default (1337)
);", script);
        }

        [TestMethod]
        public void TableTypeArrayUDTT()
        {
            var table = testDB.MakeUDTTTableAttribute(typeof(TestUploadableClass[]));
            var script = testDB.MakeCreateUDTTScript(table);
            Assert.AreEqual(@"create type [dbo].[TestUploadableClassUDTT] as table(
    [BareField] int NOT NULL,
    [FieldWithDefaultValue] int NOT NULL default (1337)
);", script);
        }

        [TestMethod]
        public void BareFieldsIncluded()
        {
            Assert.IsTrue(FieldTest("BareField"));
        }

        [TestMethod]
        public void FieldsAreExcluded()
        {
            Assert.IsFalse(FieldTest("ExcludedField"));
        }

        [TestMethod]
        public void FieldsWithDefaultValuesAreExcluded()
        {
            Assert.IsFalse(FieldTest("ExcludedFieldWithDefaultValue"));
        }

        [TestMethod]
        public void FieldsWithDefaultValuesExplicitlyIncludedAreIncluded()
        {
            Assert.IsTrue(FieldTest("FieldWithDefaultValue"));
        }

        [TestMethod]
        public void IdentityFieldExcluded()
        {
            Assert.IsFalse(FieldTest("ExcludedIdentityField"));
        }

        [TestMethod]
        public void CantForceIdentityFieldToInclude()
        {
            Assert.IsFalse(FieldTest("IncludedIdentityField"));
        }
    }
}
