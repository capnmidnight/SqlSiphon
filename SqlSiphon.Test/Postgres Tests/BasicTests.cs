using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSiphon.Mapping;
using SqlSiphon.Postgres;

namespace SqlSiphon.Test.Postgres_Tests
{
    class Name
    {
        public int id { get; set; }
        public string name { get; set; }

        public Name()
        {
        }
    }

    class Dal : DataAccessLayer
    {
        public Dal()
            : base("Server=127.0.0.1;Port=5432;Database=sqlsiphontest;User Id=sqlsiphontest;Password=sqlsiphontest;") { }


        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "select * from test_table order by id",
            EnableTransaction = false)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public List<Name> GetNames()
        {
            return this.GetList<Name>();
        }

        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "select * from test_table order by id",
            EnableTransaction = false)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public List<string> GetNamesPrimitiveByName()
        {
            return this.GetList<string>("name");
        }

        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "select * from test_table order by id",
            EnableTransaction = false)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public List<string> GetNamesPrimitiveByIndex()
        {
            return this.GetList<string>(1);
        }

        public List<Name> GetNamesFromTextQuery()
        {
            return this.GetListQuery<Name>("select * from test_table order by id");
        }

        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "select * from test_table where id = :id",
            EnableTransaction = false)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public Name GetName(int id)
        {
            return this.Get<Name>(id);
        }


        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "select * from test_table where id >= :id limit 1",
            EnableTransaction = false)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public Name FindName(int id)
        {
            return this.Get<Name>(id);
        }

        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "select * from test_table where id = :id",
            EnableTransaction = false)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public string GetNamePrimitiveByName(int id)
        {
            return this.Get<string>("name", id);
        }

        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "select * from test_table where id = :id",
            EnableTransaction = false)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public string GetNamePrimitiveByIndex(int id)
        {
            return this.Get<string>(1, id);
        }

        public Name GetNameFromTextQuery()
        {
            return this.GetQuery<Name>("select * from test_table where id = 4");
        }

        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "update test_table set name = :name where id = :id",
            EnableTransaction = false)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public void UpdateName(int id, string name)
        {
            this.Execute(id, name);
        }

        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "delete from test_table where id >= :id",
            EnableTransaction = false)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public void DeleteName(int id)
        {
            this.Execute(id);
        }

        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "insert into test_table(name) values(:name)",
            EnableTransaction = false)]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public void InsertName(string name)
        {
            this.Execute(name);
        }
    }

    [TestClass]
    public class BasicTests
    {
        private Dal d;
        [TestInitialize]
        public void Init()
        {
            d = new Dal();
        }

        [TestCleanup]
        public void Done()
        {
            d.Dispose();
            d = null;
        }

        [TestMethod]
        public void GetList()
        {
            var names = d.GetNames();
            var expected = new string[] { "sean", "dave", "mike", "carl", "paul", "neil", "mark" };
            Assert.AreEqual(names.Count, expected.Length);
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
            Assert.AreEqual(names.Count, expected.Length);
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
            Assert.AreEqual(names.Count, expected.Length);
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
            Assert.AreEqual(names.Count, expected.Length);
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
