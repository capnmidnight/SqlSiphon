using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SqlSiphon.Mapping;

namespace SqlSiphon.SqlServer
{
    public class DDLGenerator : SqlSiphon.SqlServer.DataAccessLayer
    {
        public DDLGenerator(SqlSiphon.SqlServer.DataAccessLayer dal)
            : base(dal)
        {
        }

        public DDLGenerator(string connectionString)
            : base(connectionString)
        {
        }

        public void SynchronizeProcedures<T>() where T : SqlSiphon.SqlServer.DataAccessLayer
        {
            var t = typeof(T);
            var procSignatures = t.GetMethods();
            foreach (var procSignature in procSignatures)
            {
                SynchronizeProcedure(procSignature);
            }
        }

        protected override string IdentifierPartBegin { get { return "["; } }
        protected override string IdentifierPartEnd { get { return "]"; } }

        private void SynchronizeProcedure(MethodInfo method)
        {
            var info = (MappedMethodAttribute)method.GetCustomAttribute(typeof(MappedMethodAttribute));
            if (info != null
                && info.CommandType == CommandType.StoredProcedure
                && !string.IsNullOrEmpty(info.Query))
            {
                var script = CreateOrAlterProcedureScript(method, info);
                this.ExecuteQuery(script);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(CommandType=CommandType.Text,
            Query = 
@"select routine_name 
from information_schema.routines 
where routine_schema = @schemaName 
    and routine_name = @routineName")]
        private bool ProcedureExists(string schemaName, string procedureName)
        {
            return this.GetList<string>("routine_name", schemaName, procedureName).Count() > 0;
        }

        private string CreateOrAlterProcedureScript(MethodInfo method, MappedMethodAttribute info)
        {
            var schema = info.Schema ?? "dbo";
            var name = info.Name ?? method.Name;
            var identifier = this.MakeIdentifier(info.Schema ?? "dbo", info.Name ?? method.Name);
            var parameters = AnalyzeParameters(method);
            var query = AnalyzeQuery(info);
            var createCommand = ProcedureExists(schema, name) ? "alter" : "create";
            var script = new StringBuilder();
            script.AppendFormat(
@"{0} procedure {1}
{2}
as begin
    set nocount on;
    {3}
end",
                createCommand,
                identifier,
                string.Join(",\r\n", parameters.Select(p => p.ToString()).ToArray()),
                query);
            return script.ToString();
        }

        private static string AnalyzeQuery(MappedMethodAttribute info)
        {
            var query = info.Query;
            if (info.EnableTransaction)
            {
                string transactionName = string.Format("TRANS{0}", Guid.NewGuid().ToString().Replace("-", ""));
                string transactionBegin = string.Format("begin try\r\nbegin transaction {0}", transactionName);
                string transactionEnd = string.Format(
@"commit {0}
end try
begin catch
    declare @msg nvarchar(4000), @lvl int, @stt int;
    select @msg = error_message(), @lvl = error_severity(), @stt = error_state();
    rollback {0};
    raiserror(@msg, @lvl, @stt);
end catch;", transactionName);
                query = string.Format("{0}\r\n{1}\r\n{2}", transactionBegin, info.Query, transactionEnd);
            }
            return query;
        }

        private static List<ParameterMapper> AnalyzeParameters(MethodInfo method)
        {
            var parameterSignatures = method.GetParameters();
            var parameters = new List<ParameterMapper>();
            foreach (var parameterSignature in parameterSignatures)
            {
                var p = AnaylzeParameter(parameterSignature);
                parameters.Add(p);
            }
            return parameters;
        }

        private static ParameterMapper AnaylzeParameter(ParameterInfo parameterSignature)
        {
            var parameterInfo = (MappedParameterAttribute)parameterSignature.GetCustomAttribute(typeof(MappedParameterAttribute));
            var p = new ParameterMapper(parameterInfo, parameterSignature);
            return p;
        }
    }
}
