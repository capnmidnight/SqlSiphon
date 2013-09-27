/*
https://www.github.com/capnmidnight/SqlSiphon
Copyright (c) 2009, 2010, 2011, 2012, 2013 Sean T. McBeth
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this 
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or 
  other materials provided with the distribution.

* Neither the name of McBeth Software Systems nor the names of its contributors
  may be used to endorse or promote products derived from this software without 
  specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE 
OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED 
OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.SqlServer
{
    /// <summary>
    /// A base class for building Data Access Layers that connect to MS SQL Server 2005/2008
    /// databases and execute store procedures stored within.
    /// </summary>
    public abstract class SqlServerDataAccessLayer : DataAccessLayer<SqlConnection, SqlCommand, SqlParameter, SqlDataAdapter, SqlDataReader>
    {
        private static List<Type> Synced = new List<Type>();
        /// <summary>
        /// creates a new connection to a MS SQL Server 2005/2008 database and automatically
        /// opens the connection. 
        /// </summary>
        /// <param name="connectionString">a standard MS SQL Server connection string</param>
        public SqlServerDataAccessLayer(string connectionString)
            : base(connectionString)
        {
            var t = this.GetType();
            if (!Synced.Contains(t))
            {
                this.DropProcedures();
                this.SynchronizeUserDefinedTableTypes();
                this.CreateProcedures();
                Synced.Add(t);
            }
        }

        public SqlServerDataAccessLayer(SqlConnection connection)
            : base(connection)
        {
        }

        public SqlServerDataAccessLayer(SqlServerDataAccessLayer dal)
            : base(dal)
        {
        }
        protected override string IdentifierPartBegin { get { return "["; } }
        protected override string IdentifierPartEnd { get { return "]"; } }
        protected override string DefaultSchemaName { get { return "dbo"; } }

        protected override SqlCommand BuildCommand(string procName, CommandType commandType, MappedParameterAttribute[] methParams, object[] parameterValues)
        {
            var command = base.BuildCommand(procName, commandType, methParams, parameterValues);
            return command;
        }

        private static Dictionary<string, Type> typeMapping;
        private static Dictionary<Type, string> reverseTypeMapping;
        static SqlServerDataAccessLayer()
        {
            typeMapping = new Dictionary<string, Type>();
            typeMapping.Add("bigint", typeof(long));
            typeMapping.Add("int", typeof(int));
            typeMapping.Add("smallint", typeof(short));
            typeMapping.Add("tinyint", typeof(byte));
            typeMapping.Add("decimal", typeof(decimal));
            typeMapping.Add("numeric", typeof(decimal));
            typeMapping.Add("money", typeof(decimal));
            typeMapping.Add("smallmoney", typeof(decimal));
            typeMapping.Add("bit", typeof(bool));
            typeMapping.Add("float", typeof(float));
            typeMapping.Add("real", typeof(double));
            typeMapping.Add("datetime2", typeof(DateTime));
            typeMapping.Add("datetime", typeof(DateTime));
            typeMapping.Add("smalldatetime", typeof(DateTime));
            typeMapping.Add("date", typeof(DateTime));
            typeMapping.Add("datetimeoffset", typeof(DateTime));
            typeMapping.Add("time", typeof(DateTime));
            typeMapping.Add("timestamp", typeof(DateTime));
            typeMapping.Add("nvarchar", typeof(string));
            typeMapping.Add("char", typeof(string));
            typeMapping.Add("varchar", typeof(string));
            typeMapping.Add("text", typeof(string));
            typeMapping.Add("nchar", typeof(string));
            typeMapping.Add("ntext", typeof(string));
            typeMapping.Add("varbinary", typeof(byte[]));
            typeMapping.Add("binary", typeof(byte[]));
            typeMapping.Add("image", typeof(byte[]));
            typeMapping.Add("uniqueidentifier", typeof(Guid));

            reverseTypeMapping = typeMapping
                .GroupBy(kv => kv.Value, kv => kv.Key)
                .ToDictionary(g => g.Key, g => g.First());

            reverseTypeMapping.Add(typeof(char[]), "nchar");

            reverseTypeMapping.Add(typeof(int?), "int");
            reverseTypeMapping.Add(typeof(uint), "int");
            reverseTypeMapping.Add(typeof(uint?), "int");

            reverseTypeMapping.Add(typeof(long?), "bigint");
            reverseTypeMapping.Add(typeof(ulong), "bigint");
            reverseTypeMapping.Add(typeof(ulong?), "bigint");

            reverseTypeMapping.Add(typeof(short?), "smallint");
            reverseTypeMapping.Add(typeof(ushort), "smallint");
            reverseTypeMapping.Add(typeof(ushort?), "smallint");

            reverseTypeMapping.Add(typeof(byte?), "tinyint");
            reverseTypeMapping.Add(typeof(sbyte), "tinyint");
            reverseTypeMapping.Add(typeof(sbyte?), "tinyint");
            reverseTypeMapping.Add(typeof(char), "tinyint");
            reverseTypeMapping.Add(typeof(char?), "tinyint");

            reverseTypeMapping.Add(typeof(decimal?), "decimal");
            reverseTypeMapping.Add(typeof(bool?), "bit");
            reverseTypeMapping.Add(typeof(float?), "float");
            reverseTypeMapping.Add(typeof(double?), "real");
            reverseTypeMapping.Add(typeof(DateTime?), "datetime2");
            reverseTypeMapping.Add(typeof(Guid?), "uniqueidentifier");
        }

        public static object Convert(string sqlType, object value)
        {
            return System.Convert.ChangeType(value, typeMapping[sqlType]);
        }

        protected override void ModifyQuery(MappedMethodAttribute info)
        {
            if (info.EnableTransaction)
            {
                string transactionName = string.Format("TRANS{0}", Guid.NewGuid().ToString().Replace("-", "")).Substring(0, 32);
                string transactionBegin = string.Format("begin try\r\nbegin transaction {0};", transactionName);
                string transactionEnd = string.Format(
@"commit transaction {0};
end try
begin catch
    declare @msg nvarchar(4000), @lvl int, @stt int;
    select @msg = error_message(), @lvl = error_severity(), @stt = error_state();
    rollback transaction {0};
    raiserror(@msg, @lvl, @stt);
end catch;", transactionName);
                info.Query = string.Format("{0}\r\n{1}\r\n{2}", transactionBegin, info.Query, transactionEnd);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(CommandType = CommandType.Text,
            Query =
@"select routine_name 
from information_schema.routines 
where routine_schema = @schemaName 
    and routine_name = @routineName")]
        protected override bool ProcedureExists(string schemaName, string routineName)
        {
            return this.GetList<string>("routine_name", schemaName, routineName).Count >= 1;
        }

        protected override string BuildDropProcedureScript(string identifier)
        {
            return string.Format("drop procedure {0}", identifier);
        }

        protected override string BuildCreateProcedureScript(MappedMethodAttribute info)
        {
            var identifier = this.MakeIdentifier(info.Schema ?? DefaultSchemaName, info.Name);
            var parameterSection = this.MakeParameterSection(info);
            return string.Format(
@"create procedure {0}
    {1}
as begin
    set nocount on;
    {2}
end",
                identifier,
                parameterSection,
                info.Query);
        }

        protected override string MakeParameterString(MappedParameterAttribute p)
        {
            if (p.SqlType == null)
            {
                p.SqlType = reverseTypeMapping[p.SystemType];
            }
            var typeStr = MakeSqlTypeString(p);
            return string.Join(" ",
                "@" + p.Name,
                typeStr,
                p.DefaultValue ?? "",
                p.SystemType.IsArray ? "readonly" : "").Trim();
        }

        protected override string MakeSqlTypeString(MappedTypeAttribute p)
        {
            if (p.SqlType == null)
            {
                if (reverseTypeMapping.ContainsKey(p.SystemType))
                    p.SqlType = reverseTypeMapping[p.SystemType];
                else if (p.SystemType.IsArray)
                {
                    var elementT = p.SystemType.GetElementType();
                    p.SqlType = MakeUDTTName(elementT);
                }
            }

            if (p.SqlType != null)
            {
                var typeStr = new StringBuilder(p.SqlType);
                if (p.IsSizeSet)
                {
                    typeStr.AppendFormat("({0}", p.Size);
                    if (p.IsPrecisionSet)
                    {
                        typeStr.AppendFormat(", {0}", p.Precision);
                    }
                    typeStr.Append(")");
                }

                if (p.SqlType.Contains("var") && !p.SqlType.EndsWith(")"))
                {
                    typeStr.Append("(MAX)");
                }
                return typeStr.ToString();
            }
            else
            {
                return null;
            }
        }

        static bool IsUDTT(Type t)
        {
            return t.GetCustomAttributes(typeof(SqlServerMappedClassAttribute), true)
                .Cast<SqlServerMappedClassAttribute>()
                .Where(attr => attr.IsUploadable)
                .Count() > 0;
        }

        public void SynchronizeUserDefinedTableTypes()
        {
            //primatives
            foreach (var t in reverseTypeMapping.Keys)
            {
                SynchronizeSimpleUDTT(t);
            }

            //complex types
            var type = this.GetType();
            var methods = type.GetMethods();
            var toSync = new List<Type>();
            foreach (var method in methods)
            {
                var temp = method
                    .GetParameters()
                    .Select(p=>p.ParameterType)
                    .ToList();
                temp.Add(method.ReturnType);
                toSync.AddRange(temp.Where(IsUDTT));
            }

            foreach (var c in toSync.Distinct())
            {
                MaybeSynchronizeUDTT(c);
            }
        }

        private void MaybeSynchronizeUDTT(Type c)
        {
            var attr = c.GetCustomAttributes(typeof(SqlServerMappedClassAttribute), true)
                .Cast<SqlServerMappedClassAttribute>()
                .FirstOrDefault();
            if (attr != null && attr.IsUploadable)
            {
                SynchronizeComplexUDTT(c, attr);
            }
        }

        public static string MakeUDTTName(Type c)
        {
            var name = c.GetCustomAttributes(typeof(SqlServerMappedClassAttribute), true)
                .Cast<SqlServerMappedClassAttribute>()
                .Select(attr => attr.Name)
                .FirstOrDefault();
            if (name == null)
            {
                if (c.IsArray)
                {
                    name = c.GetElementType().Name;
                }
                else
                {
                    name = c.Name;
                }
            }
            return name + "UDTT";
        }

        private void SynchronizeSimpleUDTT(Type t)
        {
            var name = MakeUDTTName(t);
            var fullName = MakeIdentifier(DefaultSchemaName, name);
            if (this.UDTTExists(DefaultSchemaName, name))
            {
                this.DropUDTT(fullName);
            }
            this.CreateSimpleUDTT(fullName, t);
        }

        private void SynchronizeComplexUDTT(Type c, SqlServerMappedClassAttribute attr)
        {
            var schema = DefaultSchemaName;
            if (attr != null && attr.Schema != null)
                schema = attr.Schema;
            var name = MakeUDTTName(c);
            var fullName = MakeIdentifier(schema, name);
            if (this.UDTTExists(schema, name))
            {
                this.DropUDTT(fullName);
            }
            this.CreateComplexUDTT(fullName, c);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(CommandType = CommandType.Text,
            Query =
@"SELECT types.name
FROM sys.types
	inner join sys.schemas on types.schema_id = schemas.schema_id
where is_user_defined = 1
	and is_table_type = 1
	and schemas.name = @schemaName
	and types.name = @UDTTName;")]
        protected bool UDTTExists(string schemaName, string UDTTName)
        {
            return this.GetList<string>("name", schemaName, UDTTName).Count >= 1;
        }

        private void DropUDTT(string fullName)
        {
            try
            {
                this.ExecuteQuery(string.Format("DROP TYPE {0}", fullName));
            }
            catch (Exception exp)
            {
                throw new Exception(string.Format("Could not create UDTT: {0}. Reason: {1}", fullName, exp.Message), exp);
            }
        }

        private void CreateComplexUDTT(string fullName, Type mappedClass)
        {
            var sb = new StringBuilder();
            var columns = GetSettableMembers(mappedClass);
            var colStrings = columns
                .Select(c => this.MaybeMakeColumnTypeString(c))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
            if (colStrings.Length > 0)
            {
                var columnDefinition = string.Join("," + Environment.NewLine + "    ", colStrings);
                var script = string.Format(
    @"CREATE TYPE {0} AS TABLE(
    {1}
)",
                    fullName,
                    columnDefinition);

                try
                {
                    this.ExecuteQuery(script);
                }
                catch (Exception exp)
                {
                    throw new Exception(string.Format("Could not create UDTT: {0}. Reason: {1}", fullName, exp.Message), exp);
                }
            }
        }

        private void CreateSimpleUDTT(string fullName, Type mappedClass)
        {
            var sb = new StringBuilder();
            var columnDefinition = reverseTypeMapping[mappedClass];
            var script = string.Format(@"CREATE TYPE {0} AS TABLE(Value {1})", fullName, columnDefinition);

            try
            {
                this.ExecuteQuery(script);
            }
            catch (Exception exp)
            {
                throw new Exception(string.Format("Could not create UDTT: {0}. Reason: {1}", fullName, exp.Message), exp);
            }
        }

        private string MaybeMakeColumnTypeString(UnifiedSetter c)
        {
            if (reverseTypeMapping.ContainsKey(c.TypeToSet))
            {
                var attr = c.TypeToSet
                    .GetCustomAttributes(typeof(MappedPropertyAttribute), true)
                    .Cast<MappedPropertyAttribute>()
                    .FirstOrDefault();
                if (attr == null)
                {
                    attr = new MappedPropertyAttribute();
                    attr.IsOptional = true;
                }

                if (attr.Name == null)
                    attr.Name = c.Name;

                if (attr.SqlType == null)
                    attr.SqlType = reverseTypeMapping[c.TypeToSet];

                var typeStr = MakeSqlTypeString(attr);
                return string.Format("{0} {1} {2}NULL {3}",
                    attr.Name,
                    typeStr,
                    attr.IsOptional ? "" : "NOT",
                    attr.DefaultValue ?? "").Trim();
            }
            return null;
        }

        protected override object PrepareParameter(object parameterValue)
        {
            if (parameterValue != null)
            {
                var t = parameterValue.GetType();
                if (t.IsArray)
                {
                    var elementT = t.GetElementType();
                    var array = (System.Collections.IEnumerable)parameterValue;
                    var table = new DataTable(MakeUDTTName(elementT));
                    if (reverseTypeMapping.ContainsKey(elementT))
                    {
                        table.Columns.Add("Value", elementT);
                        foreach (object obj in array)
                        {
                            table.Rows.Add(new object[] { obj });
                        }
                    }
                    else
                    {
                        var columns = GetSettableMembers(elementT);
                        foreach (var column in columns)
                        {
                            table.Columns.Add(column.Name, column.TypeToSet);
                        }
                        foreach (object obj in array)
                        {
                            List<object> row = new List<object>();
                            foreach (var column in columns)
                            {
                                var element = column.GetValue(obj);
                                row.Add(element);
                            }
                            table.Rows.Add(row.ToArray());
                        }
                    }
                    return table;
                }
            }
            return parameterValue;
        }
    }
}