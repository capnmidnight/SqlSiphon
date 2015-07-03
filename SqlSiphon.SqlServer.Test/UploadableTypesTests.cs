using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSiphon;
using SqlSiphon.Mapping;
using SqlSiphon.SqlServer;

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

    class MockDB : SqlServerDataAccessLayer
    {
        public MockDB()
            : base((string)null)
        {
        }
    }

    [TestClass]
    public class UploadableTypesTests
    {
        private MockDB testDB = new MockDB();

        private bool FieldTest(string fieldName)
        {
            var t = typeof(TestUploadableClass);
            var m = t.GetMember(fieldName).FirstOrDefault();
            if (m == null)
            {
                throw new Exception("Field " + fieldName + " doesn't exist");
            }
            //var script = testDB.MakeCreateUDTTScript("TestUploadableClassUDTT", t);
            //return script.IndexOf(fieldName) > -1;
            return false;
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
