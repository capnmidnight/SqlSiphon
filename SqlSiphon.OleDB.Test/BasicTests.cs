using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSiphon.Mapping;

namespace SqlSiphon.OleDB.Test
{

    [TestClass]
    public class BasicTests
    {
        private static TestDatabase d;
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            if (!System.IO.File.Exists("Test.mdb"))
            {
                d = new TestDatabase("Test.mdb");
                d.CreateTables();
                d.CreateProcedures();
                var expected = new string[] { "sean", "dave", "mike", "carl", "paul", "neil", "mark" };
                foreach (var name in expected) d.InsertName(name);
            }
            else
            {
                d = new TestDatabase("Test.mdb");
            }
        }

        [ClassCleanup]
        public static void Done()
        {
            d.Dispose();
            d = null;
            System.Diagnostics.Process.Start("cmd", "/C del Test.mdb");
        }

        [TestMethod]
        public void GetList()
        {
            var names = d.GetNames();
            var expected = new string[] { "sean", "dave", "mike", "carl", "paul", "neil", "mark" };
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.AreEqual(expected[i], names[i].name);
            }
        }

        [TestMethod]
        public void GetListPrimitiveByIndex()
        {
            var names = d.GetNamesPrimitiveByIndex();
            var expected = new string[] { "sean", "dave", "mike", "carl", "paul", "neil", "mark" };
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.AreEqual(expected[i], names[i]);
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

        [TestMethod]
        public void AutoInsert()
        {
            var newNames = new TestEntity[]{
                new TestEntity("testName1"),
                new TestEntity("testName2"),
                new TestEntity("testName3"),
                new TestEntity("testName4"),
                new TestEntity("testName5")
            };

            d.InsertAll(newNames);

            var foundNames = d.GetNames()
                .Where(n => n.name.StartsWith("testName"))
                .OrderBy(n => n.name)
                .ToArray();

            Assert.AreEqual(newNames.Length, foundNames.Length);
            for (var i = 0; i < newNames.Length; ++i)
            {
                Assert.AreEqual(newNames[i].name, foundNames[i].name);
            }
        }
    }
}
