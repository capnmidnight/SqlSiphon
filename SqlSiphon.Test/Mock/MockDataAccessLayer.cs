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

        protected override string BuildDropProcedureScript(string identifier)
        {
            throw new NotImplementedException();
        }

        protected override string MakeParameterString(Mapping.MappedParameterAttribute p)
        {
            throw new NotImplementedException();
        }

        protected override string MakeSqlTypeString(MappedTypeAttribute type)
        {
            return type.SystemType.Name.ToLower();
        }

        protected override bool ProcedureExists(string schemaName, string routineName)
        {
            return routineName.Contains("Exists");
        }
    }
}
