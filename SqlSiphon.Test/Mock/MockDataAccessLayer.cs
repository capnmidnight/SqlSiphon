﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSiphon.Mapping;

namespace SqlSiphon.Test.Mock
{
    class MockDataAccessLayer 
        : DataAccessLayer
        <MockConnection, MockCommand, MockParameter, MockDataAdapter, MockDataReader>
    {
        protected override string[] FKScripts
        {
            get { return null; }
        }
        public MockDataAccessLayer()
            : base(new MockConnection())
        {

        }

        protected override string DefaultSchemaName
        {
            get
            {
                return "mock";
            }
        }

        public List<MappedMethodAttribute> Procs { get { return this.FindProcedureDefinitions(); } }

        protected override string BuildCreateProcedureScript(Mapping.MappedMethodAttribute info)
        {
            throw new NotImplementedException();
        }

        protected override string BuildCreateTableScript(MappedClassAttribute info)
        {
            throw new NotImplementedException();
        }

        protected override string BuildDropProcedureScript(MappedMethodAttribute info)
        {
            throw new NotImplementedException();
        }

        protected override string MakeParameterString(Mapping.MappedParameterAttribute p)
        {
            throw new NotImplementedException();
        }

        protected override string MakeColumnString(MappedPropertyAttribute p)
        {
            throw new NotImplementedException();
        }

        protected override string MakeSqlTypeString(MappedTypeAttribute type)
        {
            return type.SystemType.Name.ToLower();
        }

        protected override bool ProcedureExists(MappedMethodAttribute info)
        {
            return info.Name.Contains("Exists");
        }

        protected override string MakeFKScript(string tableSchema, string tableName, string tableColumns, string foreignSchema, string foreignName, string foreignColumns)
        {
            throw new NotImplementedException();
        }

        protected override string[] IndexScripts
        {
            get { throw new NotImplementedException(); }
        }

        protected override string[] InitialScripts
        {
            get { throw new NotImplementedException(); }
        }

        protected override string MakeIndexScript(string tableSchema, string tableName, string[] tableColumns)
        {
            throw new NotImplementedException();
        }
    }
}
