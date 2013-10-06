using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSiphon.Mapping;
using SqlSiphon.Postgres;

namespace SqlSiphon.Postgres.Test
{

    [TestClass]
    public class BasicTests
    {
        private TestDatabase d;
        [TestInitialize]
        public void Init()
        {
            d = new TestDatabase();
        }

        [TestCleanup]
        public void Done()
        {
            d.Dispose();
            d = null;
        }

        [TestMethod]
        public void SynchProcedures()
        {
            d.SyncProcs();
            var names = d.TestCreateFunction();
            var expected = new string[] { "sean", "dave", "mike", "carl", "paul", "neil", "mark" };
            Assert.AreEqual(expected.Length, names.Count);
            for (int i = 0; i < expected.Length; ++i)
            {
                if (i < expected.Length - 1)
                    Assert.AreEqual(i + 1, names[i].id);
                Assert.AreEqual(expected[i], names[i].name);
            }
        }

        [TestMethod, ExpectedException(typeof(Exception), AllowDerivedTypes = false)]
        public void DropProcedures()
        {
            this.SynchProcedures();
            d.TestDropFunction();
            var names = d.TestCreateFunction();
        }

        [TestMethod]
        public void GetListFromFunction()
        {
            var names = d.GetAllNames();
            var expected = new string[] { "sean", "dave", "mike", "carl", "paul", "neil", "mark" };
            Assert.AreEqual(expected.Length, names.Count);
            for (int i = 0; i < expected.Length; ++i)
            {
                if (i < expected.Length - 1)
                    Assert.AreEqual(i + 1, names[i].id);
                Assert.AreEqual(expected[i], names[i].name);
            }
        }

        [TestMethod]
        public void GetList()
        {
            var names = d.GetNames();
            var expected = new string[] { "sean", "dave", "mike", "carl", "paul", "neil", "mark" };
            Assert.AreEqual(expected.Length, names.Count);
            for (int i = 0; i < expected.Length; ++i)
            {
                if (i < expected.Length - 1)
                    Assert.AreEqual(i + 1, names[i].id);
                Assert.AreEqual(expected[i], names[i].name);
            }
        }

        [TestMethod]
        public void GetListPrimitiveByName()
        {
            var names = d.GetNamesPrimitiveByName();
            var expected = new string[] { "sean", "dave", "mike", "carl", "paul", "neil", "mark" };
            Assert.AreEqual(expected.Length, names.Count);
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.AreEqual(expected[i], names[i]);
            }
        }

        [TestMethod]
        public void GetListPrimitiveByIndex()
        {
            var names = d.GetNamesPrimitiveByIndex();
            var expected = new string[] { "sean", "dave", "mike", "carl", "paul", "neil", "mark" };
            Assert.AreEqual(expected.Length, names.Count);
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.AreEqual(expected[i], names[i]);
            }
        }

        [TestMethod]
        public void GetListFromTextQuery()
        {
            var names = d.GetNamesFromTextQuery();
            var expected = new string[] { "sean", "dave", "mike", "carl", "paul", "neil", "mark" };
            Assert.AreEqual(expected.Length, names.Count);
            for (int i = 0; i < expected.Length; ++i)
            {
                if (i < expected.Length - 1)
                    Assert.AreEqual(i + 1, names[i].id);
                Assert.AreEqual(expected[i], names[i].name);
            }
        }

        [TestMethod]
        public void GetOne()
        {
            var name = d.GetName(3);
            Assert.AreEqual(3, name.id);
            Assert.AreEqual("mike", name.name);
        }

        [TestMethod]
        public void GetOnePrimitiveByName()
        {
            var name = d.GetNamePrimitiveByName(3);
            Assert.AreEqual("mike", name);
        }

        [TestMethod]
        public void GetOnePrimitiveByIndex()
        {
            var name = d.GetNamePrimitiveByIndex(3);
            Assert.AreEqual("mike", name);
        }

        [TestMethod]
        public void GetOneFromTextQuery()
        {
            var name = d.GetNameFromTextQuery();
            Assert.AreEqual(4, name.id);
            Assert.AreEqual("carl", name.name);
        }

        [TestMethod]
        public void ExecuteUpdate()
        {
            var name1 = d.GetName(5);
            d.UpdateName(5, "joel");
            var name2 = d.GetName(5);
            d.UpdateName(5, "paul");
            Assert.AreEqual(name1.id, name2.id);
            Assert.AreNotEqual(name1.name, name2.name);
            Assert.AreEqual("joel", name2.name);
        }

        [TestMethod]
        public void ExecuteDelete()
        {
            var name1 = d.FindName(7);
            d.DeleteName(7);
            var name2 = d.FindName(7);
            d.InsertName("mark");
            Assert.AreEqual("mark", name1.name);
            Assert.IsNull(name2);
        }
    }
}
