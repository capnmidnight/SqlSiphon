using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;

using SqlSiphon.Mapping;


namespace SqlSiphon.TestBase
{
    public class TestQueries : DataConnector
    {
        public TestQueries() : base() { }
        public TestQueries(IDataConnector connection)
            : base(connection)
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query = @"-- nothing here")]
        public void EmptyStoredProcedure()
        {
            Execute();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.StoredProcedure,
            Query = @"select * from EmptyTable")]
        public List<EmptyTable> GetEmptyTable()
        {
            return GetList<EmptyTable>();
        }
    }
}
