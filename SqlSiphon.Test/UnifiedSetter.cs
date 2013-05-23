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
        public TestEnum TestField;

        private FieldInfo TestFieldInfo { get { return this.GetType().GetField("TestField"); } }

        [TestMethod]
        public void SetEnumByString()
        {
            var setter = new SqlSiphon.UnifiedSetter(TestFieldInfo);
            setter.SetValue(this, "A");
            Assert.AreEqual(TestField, TestEnum.A);
            setter.SetValue(this, "B");
            Assert.AreEqual(TestField, TestEnum.B);
            setter.SetValue(this, "C");
            Assert.AreEqual(TestField, TestEnum.C);
        }

        [TestMethod]
        public void SetEnumByInt()
        {
            var setter = new SqlSiphon.UnifiedSetter(TestFieldInfo);
            setter.SetValue(this, 0);
            Assert.AreEqual(TestField, TestEnum.A);
            setter.SetValue(this, 1);
            Assert.AreEqual(TestField, TestEnum.B);
            setter.SetValue(this, 2);
            Assert.AreEqual(TestField, TestEnum.C);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void FailOnOutOfRangeInt()
        {
            var setter = new SqlSiphon.UnifiedSetter(TestFieldInfo);
            setter.SetValue(this, 99);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void FailOnOutOfBadString()
        {
            var setter = new SqlSiphon.UnifiedSetter(TestFieldInfo);
            setter.SetValue(this, "Garbage");
        }
    }
}
