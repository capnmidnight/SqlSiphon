using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSiphon.Test
{
    public enum TestEnum
    {
        A,
        B,
        C
    }
    [TestClass]
    public class UnifiedSetter
    {
        public TestEnum TestField1;
        private FieldInfo TestField1Info { get { return this.GetType().GetField("TestField1"); } }

        public string TestField2;
        private FieldInfo TestField2Info { get { return this.GetType().GetField("TestField2"); } }

        [TestMethod]
        public void SetEnumByString()
        {
            var setter = new SqlSiphon.UnifiedSetter(TestField1Info);
            setter.SetValue(this, "A");
            Assert.AreEqual(TestField1, TestEnum.A);
            setter.SetValue(this, "B");
            Assert.AreEqual(TestField1, TestEnum.B);
            setter.SetValue(this, "C");
            Assert.AreEqual(TestField1, TestEnum.C);
        }

        [TestMethod]
        public void SetEnumByInt()
        {
            var setter = new SqlSiphon.UnifiedSetter(TestField1Info);
            setter.SetValue(this, 0);
            Assert.AreEqual(TestField1, TestEnum.A);
            setter.SetValue(this, 1);
            Assert.AreEqual(TestField1, TestEnum.B);
            setter.SetValue(this, 2);
            Assert.AreEqual(TestField1, TestEnum.C);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void FailOnOutOfRangeInt()
        {
            var setter = new SqlSiphon.UnifiedSetter(TestField1Info);
            setter.SetValue(this, 99);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void FailOnOutOfBadString()
        {
            var setter = new SqlSiphon.UnifiedSetter(TestField1Info);
            setter.SetValue(this, "Garbage");
        }

        [TestMethod]
        public void SimpleSetString()
        {
            var setter = new SqlSiphon.UnifiedSetter(TestField2Info);
            setter.SetValue(this, "payload");
            Assert.AreEqual(TestField2, "payload");
        }

        [TestMethod]
        public void NullClearsValue()
        {
            var setter = new SqlSiphon.UnifiedSetter(TestField2Info);
            Assert.IsNull(TestField2);
            setter.SetValue(this, "payload");
            Assert.AreEqual(TestField2, "payload");
            setter.SetValue(this, null);
            Assert.IsNull(TestField2);
        }
    }
}
