/*
https://www.github.com/capnmidnight/SqlSiphon
Copyright (c) 2009 - 2014 Sean T. McBeth
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
using SqlSiphon.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSiphon.Test
{
    [TestClass]
    public class MappedPropertyTests
    {
        public enum TestEnum
        {
            Value1,
            Value2,
            Value3
        }

        public TestEnum TestProperty { get; set; }

        public MappedPropertyAttribute TestAttribute
        {
            get
            {
                var t = this.GetType();
                var p = t.GetProperty("TestProperty");
                var a = new MappedPropertyAttribute();
                a.InferProperties(p);
                return a;
            }
        }

        [TestMethod]
        public void SetEnumByInteger()
        {
            var p = TestAttribute;
            p.SetValue(this, 2);
            Assert.AreEqual(TestEnum.Value3, TestProperty);
        }

        [TestMethod]
        public void SetEnumByString()
        {
            var p = TestAttribute;
            p.SetValue(this, "Value2");
            Assert.AreEqual(TestEnum.Value2, TestProperty);
        }
    }
}
