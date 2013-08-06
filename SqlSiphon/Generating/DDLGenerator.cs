using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace SqlSiphon.Generating
{
    public class DDLGenerator<TableGenT, ProcedureGenT, TypeGenT, DataAccessT, ConnectionT, CommandT, ParameterT, DataAdapterT, DataReaderT>
        where TableGenT : TableGenerator
        where ProcedureGenT : ProcedureGenerator
        where TypeGenT : TypeGenerator
        where DataAccessT : DataAccessLayer<ConnectionT, CommandT, ParameterT, DataAdapterT, DataReaderT>
        where ConnectionT : DbConnection, new()
        where CommandT : DbCommand, new()
        where ParameterT : DbParameter, new()
        where DataAdapterT : DbDataAdapter, new()
        where DataReaderT : DbDataReader
    {
    }
}
