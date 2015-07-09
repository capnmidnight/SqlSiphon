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

        protected void ExecuteScriptsOfType(ScriptType scriptType, bool adding)
        {
            var ss = this.GetSqlSiphon();
            var initial = new DatabaseState(null, null, ss);
            var final = new DatabaseState(new Type[] { this.GetType() }, ss, ss, null, null);
            var diff = final.MakeScripts(initial, ss, ss);
            var scripts = (adding ? diff.Final : diff.Initial)
                .Where(script => (script.ScriptType & scriptType) == scriptType)
                .OrderBy(script => script.ScriptType);
            foreach (var script in scripts)
            {
                script.Run = true;
                ss.AlterDatabase(script);
            }
        }

        public void DropProcedures()
        {
            ExecuteScriptsOfType(ScriptType.DropRoutine, false);
        }

        public void DropTables()
        {
            ExecuteScriptsOfType(ScriptType.DropTable, false);
        }

        public void CreateProcedures()
        {
            ExecuteScriptsOfType(ScriptType.CreateRoutine, true);
        }

        public void CreateTables()
        {
            ExecuteScriptsOfType(ScriptType.CreateTable, true);
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
            Query = "select name from test_table order by id")]
        public List<string> GetNamesPrimitiveByIndex()
        {
            return this.GetList<string>();
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
