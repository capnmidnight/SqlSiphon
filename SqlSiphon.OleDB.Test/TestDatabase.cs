﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;

using SqlSiphon.Mapping;

namespace SqlSiphon.OleDB.Test
{
    internal class TestDatabase : DataConnector
    {
        public TestDatabase(string fileName)
            : base(new OleDBDataConnectorFactory().MakeConnector(fileName))
        {
        }

        protected void GenerateAndExecuteScripts<T>(Func<DatabaseState, Dictionary<string, T>> getter, Func<IDatabaseScriptGenerator, T, string> maker)
        {
            var ss = GetSqlSiphon();
            var final = new DatabaseState(new Type[] { GetType() }, ss, ss, null, null);
            foreach (var o in getter(final))
            {
                ss.AlterDatabase(new ScriptStatus(ScriptType.None, null, maker(ss, o.Value), null));
            }
        }

        public void CreateProcedures()
        {
            GenerateAndExecuteScripts(f => f.Functions, (ss, o) => ss.MakeCreateRoutineScript(o));
        }

        public void CreateTables()
        {
            GenerateAndExecuteScripts(f => f.Tables, (ss, o) => ss.MakeCreateTableScript(o));
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        [Routine(
            CommandType = CommandType.StoredProcedure,
            Query = "select * from test_table order by id")]
        public List<TestEntity> TestCreateFunction()
        {
            return GetList<TestEntity>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        [Routine(
            CommandType = CommandType.Text,
            Query = "select * from test_table order by id")]
        public List<TestEntity> GetNames()
        {
            return GetList<TestEntity>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        [Routine(
            CommandType = CommandType.Text,
            Query = "select name from test_table order by id")]
        public List<string> GetNamesPrimitiveByIndex()
        {
            return GetList<string>();
        }

        [Routine(
            CommandType = CommandType.StoredProcedure,
            Query = "select * from test_table where id = userid")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public TestEntity GetName(int userid)
        {
            return Get<TestEntity>(userid);
        }


        [Routine(
            CommandType = CommandType.Text,
            Query = "select top 1 * from test_table where id >= userid")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public TestEntity FindName(int userid)
        {
            return Get<TestEntity>(userid);
        }

        [Routine(
            CommandType = CommandType.StoredProcedure,
            Query = "update test_table set name = username where id = userid")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public void UpdateName(int userid, string username)
        {
            Execute(userid, username);
        }

        [Routine(
            CommandType = CommandType.Text,
            Query = "delete from test_table where id >= userid")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public void DeleteName(int userid)
        {
            Execute(userid);
        }

        [Routine(
            CommandType = CommandType.Text,
            Query = "drop table test_table")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public void DropNamesTable()
        {
            Execute();
        }

        [Routine(
            CommandType = CommandType.Text,
            Query = "insert into test_table(name) values(username)")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public void InsertName(string username)
        {
            Execute(username);
        }
    }
}
