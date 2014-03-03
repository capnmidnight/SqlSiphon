using System;
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

        protected override string MakeSqlTypeString(string sqlType, Type systemType, bool isCollection, bool isSizeSet, int size, bool isPrecisionSet, int precision)
        {
            return systemType.Name.ToLower();
        }

        protected override bool ProcedureExists(MappedMethodAttribute info)
        {
            return info.Name.Contains("Exists");
        }
    }
}
