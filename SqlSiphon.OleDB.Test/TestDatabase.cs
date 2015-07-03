using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using SqlSiphon.Mapping;

namespace SqlSiphon.OleDB.Test
{
    class TestDatabase : DataConnector
    {
        public TestDatabase(string fileName)
            : base(new OleDBDataConnectorFactory().MakeConnector(fileName))
        {
        }

        public void SyncProcs()
        {
            ExecuteScriptsOfType(ScriptType.DropRoutine | ScriptType.CreateRoutine);
        }

        public void CreateTables()
        {
            ExecuteScriptsOfType(ScriptType.DropTable | ScriptType.CreateTable);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        [Routine(
            CommandType = CommandType.StoredProcedure,
            Query = "select * from test_table order by id")]
        public List<TestEntity> TestCreateFunction()
        {
            return this.GetList<TestEntity>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        [Routine(
            CommandType = CommandType.Text,
            Query = "select * from test_table order by id")]
        public List<TestEntity> GetNames()
        {
            return this.GetList<TestEntity>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        [Routine(
            CommandType = CommandType.Text,
            Query = "select * from test_table order by id")]
        public List<string> GetNamesPrimitiveByName()
        {
            return this.GetList<string>("name");
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        [Routine(
            CommandType = CommandType.Text,
            Query = "select * from test_table order by id")]
        public List<string> GetNamesPrimitiveByIndex()
        {
            return this.GetList<string>(1);
        }

        [Routine(
            CommandType = CommandType.StoredProcedure,
            Query = "select * from test_table where id = userid")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public TestEntity GetName(int userid)
        {
            return this.Get<TestEntity>(userid);
        }


        [Routine(
            CommandType = CommandType.Text,
            Query = "select top 1 * from test_table where id >= userid")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public TestEntity FindName(int userid)
        {
            return this.Get<TestEntity>(userid);
        }

        [Routine(
            CommandType = CommandType.Text,
            Query = "select * from test_table where id = userid")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public string GetNamePrimitiveByName(int userid)
        {
            return this.Get<string>("name", userid);
        }

        [Routine(
            CommandType = CommandType.Text,
            Query = "select * from test_table where id = userid")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public string GetNamePrimitiveByIndex(int userid)
        {
            return this.Get<string>(1, userid);
        }

        [Routine(
            CommandType = CommandType.StoredProcedure,
            Query = "update test_table set name = username where id = userid")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public void UpdateName(int userid, string username)
        {
            this.Execute(userid, username);
        }

        [Routine(
            CommandType = CommandType.Text,
            Query = "delete from test_table where id >= userid")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public void DeleteName(int userid)
        {
            this.Execute(userid);
        }

        [Routine(
            CommandType = CommandType.Text,
            Query = "drop table test_table")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public void DropNamesTable()
        {
            this.Execute();
        }

        [Routine(
            CommandType = CommandType.Text,
            Query = "insert into test_table(name) values(username)")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public void InsertName(string username)
        {
            this.Execute(username);
        }
    }
}
