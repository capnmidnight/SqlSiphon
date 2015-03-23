/*
https://www.github.com/capnmidnight/SqlSiphon
Copyright (c) 2009 - 2014 Sean T. McBeth
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
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using SqlSiphon.Mapping;

namespace SqlSiphon
{
    public class ConnectionFailedException : Exception
    {
        public ConnectionFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
    /// <summary>
    /// A base class for building Data Access Layers that connect to MS SQL Server 2005/2008
    /// databases and execute store procedures stored within.
    /// </summary>
    public abstract class DataAccessLayer<ConnectionT, CommandT, ParameterT, DataAdapterT, DataReaderT> :
        ISqlSiphon
        where ConnectionT : DbConnection, new()
        where CommandT : DbCommand, new()
        where ParameterT : DbParameter, new()
        where DataAdapterT : DbDataAdapter, new()
        where DataReaderT : DbDataReader
    {
        protected static readonly bool IsOnMonoRuntime;
        static DataAccessLayer()
        {
            IsOnMonoRuntime = Type.GetType("Mono.Runtime") != null;
        }

        /// <summary>
        /// copies the values of every field in the row to the field in the object that has the same name. 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="reader"></param>
        /// <param name="columnNames"></param>
        /// <param name="props"></param>
        internal static void DoMapping(object obj, DataReaderT reader, string[] columnNames, List<ColumnAttribute> props)
        {
            props.Where(p => columnNames.Contains(p.Name.ToUpper()))
                .ToList()
                .ForEach(p => p.SetValue(obj, reader[p.Name.ToUpper()]));
        }

        protected static bool IsTypePrimitive(Type type)
        {
            return type.IsPrimitive
                || type == typeof(decimal)
                || type == typeof(string)
                || type == typeof(DateTime)
                || type == typeof(Guid)
                || type == typeof(byte[])
                || (type.IsGenericType
                    && IsTypePrimitive(type.GetGenericArguments().First()));
        }

        protected static List<ColumnAttribute> GetProperties(Type type)
        {
            var attr = DatabaseObjectAttribute.GetAttribute<TableAttribute>(type)
                ?? new TableAttribute();
            attr.InferProperties(type);
            return attr.Properties;
        }

        private static void CopyOutputParameters(object[] parameters, DbParameterCollection sqlParameters)
        {
            for (int i = 0; sqlParameters != null && i < sqlParameters.Count; ++i)
                if (sqlParameters[i].Direction == ParameterDirection.InputOutput ||
                    sqlParameters[i].Direction == ParameterDirection.Output)
                    parameters[i] = sqlParameters[i].Value;
        }

        private bool isConnectionOwned;

        private TableAttribute meta;
        private Regex FKNameRegex;

        protected virtual string IdentifierPartBegin { get { return ""; } }
        protected virtual string IdentifierPartEnd { get { return ""; } }
        protected virtual string IdentifierPartSeperator { get { return "."; } }

        public ConnectionT Connection { get; private set; }

        public event DataProgressEventHandler Progress;
        protected void MakeProgress(int currentRow, int rowCount, string message)
        {
            if (this.Progress != null)
                this.Progress(this, new DataProgressEventArgs(currentRow, rowCount, message));
        }

        /// <summary>
        /// creates a new connection to a MS SQL Server 2005/2008 database and automatically
        /// opens the connection. 
        /// </summary>
        /// <param name="connectionString">a standard MS SQL Server connection string</param>
        private DataAccessLayer(ConnectionT connection, bool isConnectionOwned)
        {
            this.FKNameRegex = new Regex(
                string.Format(@"add constraint \{0}([\w_]+)\{1}", IdentifierPartBegin, IdentifierPartEnd),
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            this.Connection = connection;
            this.isConnectionOwned = isConnectionOwned;
            var type = this.GetType();
            this.meta = DatabaseObjectAttribute.GetAttribute<TableAttribute>(type) ?? new TableAttribute();
            this.meta.InferProperties(type);
        }

        protected DataAccessLayer(string connectionString)
            : this(new ConnectionT { ConnectionString = connectionString }, true)
        {
        }

        /// <summary>
        /// Creates a query wrapper around an existing database connection and opens the connection
        /// if it is not already open.
        /// </summary>
        /// <param name="connection"></param>
        protected DataAccessLayer(ConnectionT connection)
            : this(connection, false)
        {
        }

        protected DataAccessLayer(DataAccessLayer<ConnectionT, CommandT, ParameterT, DataAdapterT, DataReaderT> dal)
            : this(dal != null ? dal.Connection : null, false)
        {
        }

        /// <summary>
        /// Cleans up the connection with the database.
        /// </summary>
        public virtual void Dispose()
        {
            if (this.isConnectionOwned && this.Connection != null)
            {
                if (this.Connection.State == ConnectionState.Open)
                    this.Connection.Close();
                this.Connection.Dispose();
                this.Connection = null;
            }
        }

        private void Open()
        {
            try
            {
                if (Connection.State == ConnectionState.Closed)
                    Connection.Open();
            }
            catch (Exception exp)
            {
                throw new ConnectionFailedException("Could not connect to the database at : " + this.Connection.ConnectionString, exp);
            }
        }

        public virtual string MakeIdentifier(params string[] parts)
        {
            var identifier = string.Join(IdentifierPartSeperator, parts
                .Where(p => p != null)
                .Select(p => string.Format("{0}{1}{2}", IdentifierPartBegin, p, IdentifierPartEnd))
                .ToArray());
            return identifier;
        }

        public abstract string MakeRoutineIdentifier(RoutineAttribute routine);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text,
            Query = "select * from ScriptStatus")]
        public List<ScriptStatus> GetScriptStatus()
        {
            return this.GetList<ScriptStatus>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text,
            Query = "select count(*) from ScriptStatus")]
        public int GetDatabaseVersion()
        {
            return this.Get<int>(0);
        }

        public void AlterDatabase(ScriptStatus script)
        {
            this.ExecuteQuery(script.Script);
            this.MarkScriptAsRan(script);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text,
            Query = @"if exists(select *
from information_schema.tables 
where table_name = 'ScriptStatus')
begin
    insert into ScriptStatus
    (Script) values
    (@script);
end")]
        public void MarkScriptAsRan(ScriptStatus script)
        {
            this.Insert(new ScriptStatus[] { script });
        }


        /// <summary>
        /// Performs a basic insert operation for a collection of data. By default, this will perform poorly, as it does
        /// a naive INSERT statement for each element of the array. It is meant as a base implementation that can be overridden
        /// in deriving classes to support their vendor's bulk-insert operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public virtual void Insert<T>(IEnumerable<T> data)
        {
            if (data != null)
            {
                var t = typeof(T);
                var attr = DatabaseObjectAttribute.GetAttribute<TableAttribute>(t);
                if (attr == null)
                {
                    throw new Exception(string.Format("Type {0}.{1} could not be automatically inserted.", t.Namespace, t.Name));
                }
                attr.InferProperties(t);

                // don't upload auto-incrementing identity columns
                // or columns that have a default value defined
                var props = GetProperties(t);
                var columns = props
                    .Where(p => p.Include && !p.IsIdentity && (p.IsIncludeSet || p.DefaultValue == null))
                    .ToArray();
                var methParams = columns.Select(c => c.ToParameter()).ToArray();

                var columnNames = columns.Select(c => c.Name);
                var parameterNames = columnNames.Select(c => "@" + c);

                string query = string.Format("insert into {0}({1}) values({2})",
                    this.MakeIdentifier(attr.Schema ?? DefaultSchemaName, attr.Name),
                    string.Join(", ", columnNames),
                    string.Join(", ", parameterNames));

                using (var command = BuildCommand(query, CommandType.Text, methParams))
                {
                    foreach (T obj in data)
                    {
                        var parameterValues = new object[columns.Length];
                        for (int i = 0; i < columns.Length; ++i)
                        {
                            parameterValues[i] = columns[i].GetValue<object>(obj);
                        }
                        this.CopyParameterValues(command, parameterValues.ToArray());
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Creates a stored procedure command call from a variable array of parameters. The stored
        /// procedure to call is determined by walking the Stack Trace to find a method with the
        /// RoutineAttribute attribute applied to it. This method, once found, must have the same
        /// name as a stored procedure in the database.
        /// </summary>
        /// <param name="isPrimitive">If the query routine returns a single value, the method will generate
        /// a more specific error message if the parameter count is not correct.</param>
        /// <param name="parameterValues">a variable number of parameters to pass to the stored procedure.
        /// </param>
        /// <returns>a SqlCommand structure for encapsulating a stored procedure call</returns>
        protected CommandT ConstructCommand(params object[] parameterValues) { return ConstructCommand(false, parameterValues); }
        private CommandT ConstructCommand(bool isPrimitive, params object[] parameterValues)
        {
            // because this method is private, it will never be called directly by a method tagged
            // with RoutineAttribute. Therefore, we can skip two methods in the stack trace to
            // try to get to the one we're interested in--this one and the caller of this one.
            var method = (from frame in new StackTrace(2, false).GetFrames()
                          let meth = frame.GetMethod()
                          where meth is MethodInfo
                            && DatabaseObjectAttribute.GetAttribute<RoutineAttribute>(meth) != null
                          select (MethodInfo)meth).FirstOrDefault();

            // We absolutely need to find a method with this attribute, because we won't know where in
            // the stack trace to stop, so that we can inspect the method for parameter name info.
            if (method == null)
                throw new Exception("Could not find a mapped method!");

            var meta = GetCommandDescription(method);

            // since the parameters array was passed to this method by the mapped method, the mapped
            // method has the responsibility to pass the parameters in the same order they are
            // specified in the method signature
            if (meta.Parameters.Count != parameterValues.Length)
            {
                if (isPrimitive && meta.Parameters.Count == parameterValues.Length + 1)
                    throw new Exception("When querying for a single value, the first parameter passed to the processing method will be treated as a column name or column index to map out of the query result set.");
                else if (meta.Parameters.Count < parameterValues.Length)
                    throw new Exception("More parameters were passed to the processing method than were specified in the mapped method signature.");
                else
                    throw new Exception("Fewer parameters were passed to the processing method than were specified in the mapped method signature.");
            }

            var procName = MakeIdentifier(meta.Schema ?? DefaultSchemaName, meta.Name);
            var methParams = meta.Parameters.ToArray();
            var command = BuildCommand(meta.CommandType == CommandType.Text ? meta.Query : procName, meta.CommandType, methParams);
            this.CopyParameterValues(command, parameterValues);

            if (meta.Timeout > -1)
                command.CommandTimeout = meta.Timeout;
            return command;
        }

        protected virtual CommandT BuildCommand(string procName, CommandType commandType, ParameterAttribute[] methParams)
        {
            // the mapped method must match the name of a stored procedure in the database, or the
            // query or procedure name must be provided explicitly in the RoutineAttribute's
            // Query property.
            var command = new CommandT();
            command.Connection = Connection;
            command.CommandType = commandType;
            // meta.Query defaults to null, so default behavior is to use the procedure name
            command.CommandText = procName;
            command.Parameters.AddRange(MakeProcedureParameters(methParams));
            return command;
        }

        protected virtual object PrepareParameter(object val)
        {
            //do nothing in the base case
            return val;
        }

        private void CopyParameterValues(CommandT command, object[] parameterValues)
        {
            for (var i = 0; i < command.Parameters.Count; ++i)
            {
                command.Parameters[i].Value = PrepareParameter(parameterValues[i]) ?? DBNull.Value;
            }
        }

        private ParameterT[] MakeProcedureParameters(ParameterAttribute[] methParams)
        {
            List<ParameterT> procedureParams = new List<ParameterT>();
            for (int i = 0; methParams != null && i < methParams.Length; ++i)
            {
                var p = new ParameterT();
                p.ParameterName = methParams[i].Name;
                p.Direction = methParams[i].Direction;
                procedureParams.Add(p);
            }
            return procedureParams.ToArray();
        }

        /// <summary>
        /// A convenience method when a stored procedure is only expected to return one row.
        /// </summary>
        /// <typeparam name="EntityT">The mapped type of the rows of the table. May be a primitive type or 
        /// a class with public fields</typeparam>
        /// <param name="parameters">the parameters to pass to the stored procedure that will be
        /// executed. If the Type parameter EntityT is a primitive type, then the first parameter is treated
        /// as the column name in the table from which to retrieve the desired value</param>
        /// <returns>an object that represents the row from the database</returns>
        protected EntityT Get<EntityT>(params object[] parameters)
        {
            // the ToList call is necessary to consume the entire enumerator
            return GetEnumerator<EntityT>(parameters).ToList().FirstOrDefault();
        }

        protected EntityT Return<EntityT>(params object[] parameters)
        {
            using (var cmd = this.ConstructCommand(parameters))
            {
                var p = new ParameterT();
                p.Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(p);
                cmd.ExecuteNonQuery();
                return (EntityT)p.Value;
            }
        }

        /// <summary>
        /// Builds a stored procedure command call and executes it without returning any result set
        /// </summary>
        /// <param name="parameters">a variable number of parameters to pass to the stored procedure.
        /// Null values are ignored.</param>
        private void Execute(CommandT command, params object[] parameters)
        {
            Open();
            try
            {
                command.ExecuteNonQuery();
                CopyOutputParameters(parameters, command.Parameters);
            }
            catch (Exception exp)
            {
                throw new Exception(string.Format("Could not execute command: {0}. Reason: {1}.", command.CommandText, exp.Message), exp);
            }
        }

        protected virtual void ExecuteQuery(string query)
        {
            if (!string.IsNullOrWhiteSpace(query))
                using (var command = BuildCommand(query, CommandType.Text, null))
                    Execute(command);
        }

        protected void Execute(params object[] parameters)
        {
            using (var command = ConstructCommand(parameters))
                Execute(command, parameters);
        }

        /// <summary>
        /// Constructs a representation of table data returned from a stored procedure as a List of
        /// objects that represent the individual rows of the table.
        /// </summary>
        /// <typeparam name="EntityT">The mapped type of the rows of the table. May be a primitive type or 
        /// a class with public fields</typeparam>
        /// <param name="parameters">the parameters to pass to the stored procedure that will be
        /// executed. If the Type parameter EntityT is a primitive type, then the first parameter is treated
        /// as the column name in the table from which to retrieve the desired value</param>
        /// <returns>the populated list of objects that represent the rows in the database</returns>
        protected List<EntityT> GetList<EntityT>(params object[] parameters)
        {
            return GetEnumerator<EntityT>(parameters).ToList();
        }

        private DataSet GetDataSet(CommandT command, params object[] parameters)
        {
            var result = new DataSet();
            using (var adapter = new DataAdapterT())
            {
                adapter.SelectCommand = command;
                //Open(); // open is not necessary for DataAdapter.Fill()
                adapter.Fill(result);
            }
            CopyOutputParameters(parameters, command.Parameters);
            return result;
        }

        protected DataSet GetDataSet(params object[] parameters)
        {
            using (var command = ConstructCommand(parameters))
                return GetDataSet(command, parameters);
        }

        private DataReaderT GetReader(CommandT command, params object[] parameters)
        {
            DbDataReader reader = null;
            if (Connection != null)
            {
                Open();
                try
                {
                    reader = command.ExecuteReader();
                    CopyOutputParameters(parameters, command.Parameters);
                }
                catch (Exception exp)
                {
                    throw new Exception(string.Format("Could not execute: {0}. Reason: {1}.", command.CommandText, exp.Message), exp);
                }
            }
            return (DataReaderT)reader;
        }

        protected DataReaderT GetReader(params object[] parameters)
        {
            using (var command = ConstructCommand(parameters))
                return GetReader(command, parameters);
        }

        /// <summary>
        /// IteratePrimitive maps a single column from a result set to a collection of primitive .NET data types.
        /// </summary>
        /// <typeparam name="EntityT"></typeparam>
        /// <param name="reader"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private IEnumerable<EntityT> Iterate<EntityT>(DataReaderT reader, object key)
        {
            Func<object> getter = null;

            if (key is string)
                getter = () => reader[(string)key];
            else if (key is int)
                getter = () => reader[(int)key];
            else if (key is Type)
            {
                // A raw entity type must have a default constructor. We need to be able to create an
                // uninitialized version of the object so that we can set the fields to the
                // mapped values later
                var type = (Type)key;
                var constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor == null)
                    throw new Exception("Entity classes need default constructors!");
                var columnNames = new string[reader.FieldCount];
                for (int i = 0; i < reader.FieldCount; ++i)
                    columnNames[i] = reader.GetName(i).ToUpper();
                var fields = GetProperties(type);
                getter = delegate()
                {
                    var inst = (EntityT)constructor.Invoke(null);
                    DoMapping(inst, reader, columnNames, fields);
                    return inst;
                };
            }

            if (getter != null)
            {
                while (reader.Read())
                {
                    var val = getter();
                    if (val == DBNull.Value)
                        yield return default(EntityT);
                    else
                        yield return (EntityT)val;
                }
            }

            reader.Dispose();

            // this is done here to make sure the reader gets disposed first
            if (getter == null)
                throw new ArgumentException("When retrieving a primitive data type, the first parameter to the procedure must be a string Column Name or an integer Column Index. Given: " + key.GetType().Name);
        }


        /// <summary>
        /// Constructs a representation of table data returned from a stored procedure as an enumerable
        /// iterator via the "yield return" construct, generating objects that represent individual
        /// rows of the table.
        /// </summary>
        /// <typeparam name="EntityT">The mapped type of the rows of the table. May be a primitive type or
        /// a class with public fields, AKA "Entity Class"</typeparam>
        /// <param name="parameters">the parameters to pass to the stored procedure that will be
        /// executed. If the Type parameter EntityT is a primitive type, then the first parameter is treated
        /// as the column name or column index in the table from which to retrieve the desired value</param>
        /// <returns>the populated list of objects that represent the rows in the database</returns>
        protected IEnumerable<EntityT> GetEnumerator<EntityT>(params object[] parameters)
        {
            var type = typeof(EntityT);

            // The most basic case is returning values from one column in the table.
            // The first parameter is skipped because it's the column name to retrieve,
            // not a parameter to the stored procedure
            bool isPrimitive = IsTypePrimitive(type);

            object key = type;

            if (isPrimitive)
            {
                key = parameters.First();
                parameters = parameters.Skip(1).ToArray();
            }

            using (var command = ConstructCommand(parameters))
            {
                var reader = GetReader(command, parameters);
                return Iterate<EntityT>(reader, key);
            }
        }

        protected List<RoutineAttribute> FindProcedureDefinitions()
        {
            var t = this.GetType();
            var methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            var results = new List<RoutineAttribute>();
            foreach (var method in methods)
            {
                var info = this.GetCommandDescription(method);
                if (info != null)
                    results.Add(info);
            }
            return results;
        }

        public RoutineAttribute GetCommandDescription(MethodInfo method)
        {
            var meta = DatabaseObjectAttribute.GetAttribute<RoutineAttribute>(method);
            if (meta != null)
            {
                if (meta.CommandType == CommandType.TableDirect)
                    throw new NotImplementedException("Table-Direct queries are not supported by SqlSiphon");
                meta.InferProperties(method);

                if (meta.Schema == null)
                    meta.Schema = DefaultSchemaName;
            }
            return meta;
        }

        private void Process<T>(List<T> collection, Func<T, string> makeMessage, Action<T> action)
        {
            for (int i = 0; i < collection.Count; ++i)
            {
                var message = makeMessage(collection[i]);
                action(collection[i]);
                this.MakeProgress(i, collection.Count, message);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"SELECT *
FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE
WHERE TABLE_SCHEMA = @tableSchema
AND TABLE_NAME = @tableName
AND COLUMN_NAME = @columnName;")]
        public List<InformationSchema.ConstraintColumnUsage> GetColumnConstraints(string tableSchema, string tableName, string columnName)
        {
            return GetList<InformationSchema.ConstraintColumnUsage>(tableSchema, tableName, columnName);
        }

        public virtual DatabaseState GetInitialState(string catalogueName, Regex filter)
        {
            return new DatabaseState(catalogueName, filter, this);
        }

        public virtual DatabaseState GetFinalState(string userName, string password)
        {
            var asm = this.GetType().Assembly;
            var types = asm.GetTypes().ToList();
            types.Insert(0, typeof(ScriptStatus));
            var final = new DatabaseState(types, this, userName, password);
            return final;
        }

        private string ArgumentList<T>(IEnumerable<T> collect, Func<T, string> format, string separator = null)
        {
            var arr = collect.ToArray();
            separator = separator ?? "," + Environment.NewLine + "    ";
            var str = string.Join(separator, arr.Select(format));
            return str;
        }

        protected string MakeParameterSection(RoutineAttribute info)
        {
            return ArgumentList(info.Parameters, this.MakeParameterString);
        }

        protected string MakeColumnSection(TableAttribute info, bool isReturnType)
        {
            return ArgumentList(info.Properties.Where(p => p.Include), p => this.MakeColumnString(p, isReturnType));
        }

        protected string MakeSqlTypeString(DatabaseObjectAttribute p)
        {
            if (p.Include)
            {
                var isIdentity = p is ColumnAttribute && ((ColumnAttribute)p).IsIdentity;
                var systemType = p.SystemType;
                if (systemType != null && systemType.IsEnum)
                    systemType = typeof(int);
                return MakeSqlTypeString(
                    p.SqlType,
                    systemType,
                    p.IsSizeSet ? new Nullable<int>(p.Size) : null,
                    p.IsPrecisionSet ? new Nullable<int>(p.Precision) : null,
                    isIdentity);
            }
            else
            {
                return null;
            }
        }

        public virtual bool RoutineChanged(RoutineAttribute a, RoutineAttribute b)
        {
            var q1 = this.MakeCreateRoutineScript(a);
            var q2 = this.MakeCreateRoutineScript(b);
            var changed = q1 != q2;
            if (changed)
            {
                bool changedReturnType = a.SqlType != b.SqlType;
                bool changedParameters = a.Parameters.Count != b.Parameters.Count;
                if (!changedParameters)
                {
                    var paramChanges = a.Parameters.Select((p1, i) =>
                    {
                        var p2 = b.Parameters[i];
                        var tests = new bool[]{
                            p1.Name.ToLower() != p2.Name.ToLower(),
                            p1.DefaultValue != p2.DefaultValue,
                            p1.Direction != p2.Direction,
                            p1.IsOptional != p2.IsOptional,
                            p1.SystemType != p2.SystemType,
                            p1.IsPrecisionSet != p2.IsPrecisionSet,
                            p1.IsSizeSet != p2.IsSizeSet,
                            p1.Size != p2.Size,
                            p1.Precision != p2.Precision
                        };
                        var changed2 = tests.Any(t => t);
                        return changed2;
                    }).ToArray();
                    changedParameters = paramChanges.Any(t => t);
                }
                bool changedQuery = a.Query.Trim() != b.Query.Trim();
                changed = changedReturnType
                    || changedParameters
                    || changedQuery;
            }
            return changed;
        }

        public virtual bool RelationshipChanged(Relationship a, Relationship b)
        {
            var aFromTableName = this.MakeIdentifier(a.From.Schema, a.From.Name).ToLower();
            var aFromColumnNames = a.FromColumns.Select(c => c.Name.ToLower());
            var aToTableName = this.MakeIdentifier(a.To.Schema, a.To.Name).ToLower();
            var bFromTableName = this.MakeIdentifier(b.From.Schema, b.From.Name).ToLower();
            var bFromColumnNames = b.FromColumns.Select(c => c.Name.ToLower());
            var bToTableName = this.MakeIdentifier(b.To.Schema, b.To.Name).ToLower();
            var changed = aFromTableName != bFromTableName
                || aToTableName != bToTableName
                || aFromColumnNames.Any(c => !bFromColumnNames.Contains(c))
                || bFromColumnNames.Any(c => !aFromColumnNames.Contains(c));
            return changed;
        }

        public virtual bool IndexChanged(Index a, Index b)
        {
            var aTableName = this.MakeIdentifier(a.Table.Schema, a.Table.Name).ToLower();
            var bTableName = this.MakeIdentifier(b.Table.Schema, b.Table.Name).ToLower();
            var aColumnNames = a.Columns.Select(c => c.ToLower());
            var bColumnNames = b.Columns.Select(c => c.ToLower());
            var changed = aTableName != bTableName
                || aColumnNames.Any(c => !bColumnNames.Contains(c))
                || bColumnNames.Any(c => !aColumnNames.Contains(c));
            return changed;
        }

        public virtual bool KeyChanged(PrimaryKey final, PrimaryKey initial)
        {
            var f = this.MakeCreatePrimaryKeyScript(final);
            var i = this.MakeCreatePrimaryKeyScript(initial);
            var changed = f.ToLower() != i.ToLower();
            return changed;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select schema_name from information_schema.schemata;")]
        public virtual List<string> GetSchemata()
        {
            return this.GetList<string>();
        }

        public virtual string MakeCreateCatalogueScript(string catalogueName)
        {
            return string.Format("create database {0};", catalogueName);
        }

        private string MakeSchemaScript(string op, string schemaName)
        {
            return string.Format("{0} schema {1};", op, schemaName);
        }

        public virtual string MakeCreateSchemaScript(string schemaName)
        {
            return MakeSchemaScript("create", schemaName);
        }

        public virtual string MakeDropSchemaScript(string schemaName)
        {
            return MakeSchemaScript("drop", schemaName);
        }

        public abstract List<string> GetDatabaseLogins();
        public abstract List<InformationSchema.Columns> GetColumns();
        public abstract List<InformationSchema.IndexColumnUsage> GetIndexColumns();
        public abstract List<InformationSchema.TableConstraints> GetTableConstraints();
        public abstract List<InformationSchema.ReferentialConstraints> GetReferentialConstraints();
        public abstract List<InformationSchema.Routines> GetRoutines();
        public abstract List<InformationSchema.Parameters> GetParameters();
        public abstract List<InformationSchema.ConstraintColumnUsage> GetConstraintColumns();
        public abstract List<InformationSchema.KeyColumnUsage> GetKeyColumns();

        public abstract string DefaultSchemaName { get; }
        public abstract Type GetSystemType(string sqlType);
        public abstract bool DescribesIdentity(InformationSchema.Columns column);
        public abstract bool ColumnChanged(ColumnAttribute final, ColumnAttribute initial);
        public abstract void AnalyzeQuery(string routineText, RoutineAttribute routine);

        protected abstract string MakeSqlTypeString(string sqlType, Type systemType, int? size, int? precision, bool isIdentity);
        protected abstract string MakeColumnString(ColumnAttribute p, bool isReturnType);
        protected abstract string MakeParameterString(ParameterAttribute p);

        public abstract string MakeCreateDatabaseLoginScript(string userName, string password, string database);

        public abstract string MakeCreateTableScript(TableAttribute table);
        public abstract string MakeDropTableScript(TableAttribute table);

        public abstract string MakeCreateColumnScript(ColumnAttribute column);
        public abstract string MakeDropColumnScript(ColumnAttribute column);
        public abstract string MakeAlterColumnScript(ColumnAttribute final, ColumnAttribute initial);

        public abstract string MakeDropRoutineScript(RoutineAttribute routine);
        public abstract string MakeCreateRoutineScript(RoutineAttribute routine);

        public abstract string MakeDropRelationshipScript(Relationship relation);
        public abstract string MakeCreateRelationshipScript(Relationship relation);

        public abstract string MakeDropPrimaryKeyScript(PrimaryKey key);
        public abstract string MakeCreatePrimaryKeyScript(PrimaryKey key);
        public abstract string MakeDropIndexScript(Index index);
        public abstract string MakeCreateIndexScript(Index index);
    }
}