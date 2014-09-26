using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using SqlSiphon.Mapping;

namespace SqlSiphon.Postgres.Test
{
    class TestDatabase: NpgsqlDataAccessLayer
    {
        public TestDatabase()
            : base("Server=127.0.0.1;Port=5432;Database=sqlsiphontest;User Id=sqlsiphontest;Password=sqlsiphontest;") { }
        protected override string[] FKScripts
        {
            get { return null; }
        }

        public void SyncProcs()
        {
            this.DropProcedures();
            this.SynchronizeUserDefinedTableTypes();
            this.CreateProcedures();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        [MappedMethod(
            CommandType = CommandType.StoredProcedure,
            Query = "select * from test_table order by id")]
        public List<TestEntity> TestCreateFunction()
        {
            return this.GetList<TestEntity>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        [MappedMethod(
            CommandType = CommandType.StoredProcedure,
            Query = 
@"SELECT sum($1[i])::int FROM
generate_series(array_lower($1,1),array_upper($1,1)
) index(i)")]
        public int TestArray(int[] arr)
        {
            return this.Get<int>(0, arr);
        }

        public void TestDropFunction()
        {
            this.DropProcedures();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "select * from test_table order by id")]
        public List<TestEntity> GetNames()
        {
            return this.GetList<TestEntity>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        [MappedMethod]
        public List<TestEntity> GetAllNames()
        {
            return this.GetList<TestEntity>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "select * from test_table order by id")]
        public List<string> GetNamesPrimitiveByName()
        {
            return this.GetList<string>("name");
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "select * from test_table order by id")]
        public List<string> GetNamesPrimitiveByIndex()
        {
            return this.GetList<string>(1);
        }

        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "select * from test_table where id = :id")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public TestEntity GetName(int id)
        {
            return this.Get<TestEntity>(id);
        }


        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "select * from test_table where id >= :id limit 1")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public TestEntity FindName(int id)
        {
            return this.Get<TestEntity>(id);
        }

        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "select * from test_table where id = :id")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public string GetNamePrimitiveByName(int id)
        {
            return this.Get<string>("name", id);
        }

        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "select * from test_table where id = :id")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public string GetNamePrimitiveByIndex(int id)
        {
            return this.Get<string>(1, id);
        }

        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "update test_table set name = :name where id = :id")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public void UpdateName(int id, string name)
        {
            this.Execute(id, name);
        }

        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "delete from test_table where id >= :id")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public void DeleteName(int id)
        {
            this.Execute(id);
        }

        [MappedMethod(
            CommandType = CommandType.Text,
            Query = "insert into test_table(name) values(:name)")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public void InsertName(string name)
        {
            this.Execute(name);
        }
    }
}
