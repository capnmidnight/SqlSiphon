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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSiphon.Mapping;

namespace SqlSiphon.Test
{
    [TestClass]
    public class ProcedureInfoTests
    {
        class Entity
        {
            public string Prop1 { get; set; }
            public int Prop2 { get; set; }
            public DateTime Prop3 { get; set; }
        }

        class DAL : Mock.MockDataAccessLayer
        {
            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
            [MappedMethod]
            public void Method1() { }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
            [MappedMethod]
            public void Method2(int p1) { }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
            [MappedMethod]
            public string Method3(int p1) { return null; }

            [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
            [MappedMethod]
            public Entity Method4() { return null; }
        }

        private DAL db;

        [TestInitialize]
        public void Setup()
        {
            db = new DAL();
        }

        [TestCleanup]
        public void Teardown()
        {
            db.Dispose();
        }

        private MappedMethodAttribute GetProc(string name)
        {
            return db.Procs.Where(x => x.Name == name).First();
        }

        [TestMethod]
        public void FindMethodDescriptions1()
        {
            var p = GetProc("Method1");
            Assert.AreEqual(CommandType.StoredProcedure, p.CommandType);
            Assert.IsFalse(p.EnableTransaction);
            Assert.IsTrue(p.Include);
            Assert.IsNull(p.Query);
            Assert.AreEqual(typeof(void), p.SystemType);
            Assert.AreEqual("void", p.SqlType);
            Assert.AreEqual("mock", p.Schema);
            Assert.AreEqual(-1, p.Timeout);
            Assert.AreEqual(0, p.Parameters.Count);
        }

        [TestMethod]
        public void FindMethodDescriptions2()
        {
            var p = GetProc("Method2");
            Assert.AreEqual(CommandType.StoredProcedure, p.CommandType);
            Assert.IsFalse(p.EnableTransaction);
            Assert.IsTrue(p.Include);
            Assert.IsNull(p.Query);
            Assert.AreEqual(typeof(void), p.SystemType);
            Assert.AreEqual("void", p.SqlType);
            Assert.AreEqual("mock", p.Schema);
            Assert.AreEqual(-1, p.Timeout);
            Assert.AreEqual(1, p.Parameters.Count);
            Assert.AreEqual("p1", p.Parameters[0].Name);
            Assert.AreEqual(typeof(int), p.Parameters[0].SystemType);
            Assert.AreEqual("int32", p.Parameters[0].SqlType);
        }

        [TestMethod]
        public void FindMethodDescriptions3()
        {
            var p = GetProc("Method3");
            Assert.AreEqual(CommandType.StoredProcedure, p.CommandType);
            Assert.IsFalse(p.EnableTransaction);
            Assert.IsTrue(p.Include);
            Assert.IsNull(p.Query);
            Assert.AreEqual(typeof(string), p.SystemType);
            Assert.AreEqual("string", p.SqlType);
            Assert.AreEqual("mock", p.Schema);
            Assert.AreEqual(-1, p.Timeout);
            Assert.AreEqual(1, p.Parameters.Count);
            Assert.AreEqual("p1", p.Parameters[0].Name);
            Assert.AreEqual(typeof(int), p.Parameters[0].SystemType);
            Assert.AreEqual("int32", p.Parameters[0].SqlType);
        }

        [TestMethod]
        public void FindMethodDescriptions4()
        {
            var p = GetProc("Method4");
            Assert.AreEqual(CommandType.StoredProcedure, p.CommandType);
            Assert.IsFalse(p.EnableTransaction);
            Assert.IsTrue(p.Include);
            Assert.IsNull(p.Query);
            Assert.AreEqual(typeof(Entity), p.SystemType);
            Assert.AreEqual("entity", p.SqlType);
            Assert.AreEqual("mock", p.Schema);
            Assert.AreEqual(-1, p.Timeout);
            Assert.AreEqual(0, p.Parameters.Count);
        }
    }
}
