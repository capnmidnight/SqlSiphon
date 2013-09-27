/*
https://www.github.com/capnmidnight/SqlSiphon
Copyright (c) 2009, 2010, 2011, 2012, 2013 Sean T. McBeth
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this 
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or 
  other materials provided with the distribution.

* Neither the name of McBeth Software Systems nor the names of its contributors
  may be used to endorse or promote products derived from this software without 
  specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE 
OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED 
OF THE POSSIBILITY OF SUCH DAMAGE.
*/

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
