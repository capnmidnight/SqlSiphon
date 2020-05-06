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
using SqlSiphon.Model;

namespace SqlSiphon
{
    /// <summary>
    /// A base class for building Data Access Layers that connect to MS SQL Server 2005/2008
    /// databases and execute store procedures stored within.
    /// </summary>
    public abstract class SqlSiphon<ConnectionT, CommandT, ParameterT, DataAdapterT, DataReaderT> :
        ISqlSiphon
        where ConnectionT : DbConnection, new()
        where CommandT : DbCommand, new()
        where ParameterT : DbParameter, new()
        where DataAdapterT : DbDataAdapter, new()
        where DataReaderT : DbDataReader
    {

        protected static readonly bool IsOnMonoRuntime = Type.GetType("Mono.Runtime") != null;
        protected static List<ColumnAttribute> GetProperties(Type type)
        {
            var attr = DatabaseObjectAttribute.GetTable(type) ?? new TableAttribute(type);
            return attr.Properties;
        }

        private static void CopyOutputParameters(object[] parameters, DbParameterCollection sqlParameters)
        {
            for (var i = 0; sqlParameters != null && i < sqlParameters.Count; ++i)
            {
                if (sqlParameters[i].Direction == ParameterDirection.InputOutput ||
                    sqlParameters[i].Direction == ParameterDirection.Output)
                {
                    parameters[i] = sqlParameters[i].Value;
                }
            }
        }

        private readonly bool isConnectionOwned;

        protected virtual string IdentifierPartBegin => "";
        protected virtual string IdentifierPartEnd => "";
        protected virtual string IdentifierPartSeperator => ".";
        protected virtual string UDTTDeclarationPrefix => "";

        public ConnectionT Connection { get; private set; }

        /// <summary>
        /// creates a new connection to a MS SQL Server 2005/2008 database and automatically
        /// opens the connection. 
        /// </summary>
        /// <param name="connectionString">a standard MS SQL Server connection string</param>
        private SqlSiphon(bool isConnectionOwned)
        {
            this.isConnectionOwned = isConnectionOwned;
        }

        protected SqlSiphon()
        {

        }

        protected SqlSiphon(string connectionString)
            : this(true)
        {
            SetConnection(connectionString);
        }

        protected SqlSiphon(string server, string database, string userName, string password)
            : this(true)
        {
            SetConnection(MakeConnectionString(server, database, userName, password));
        }

        /// <summary>
        /// Creates a query wrapper around an existing database connection and opens the connection
        /// if it is not already open.
        /// </summary>
        /// <param name="connection"></param>
        protected SqlSiphon(ConnectionT connection)
            : this(false)
        {
            SetConnection(connection);
        }

        protected SqlSiphon(SqlSiphon<ConnectionT, CommandT, ParameterT, DataAdapterT, DataReaderT> dal)
            : this(false)
        {
            SetConnection(dal?.Connection);
        }

        private void SetConnection(string connectionString)
        {
            SetConnection(new ConnectionT { ConnectionString = connectionString });
        }

        private void SetConnection(ConnectionT connection)
        {
            Connection = connection;
        }


        public abstract string MakeConnectionString(string server, string database, string user, string password);

        /// <summary>
        /// Cleans up the connection with the database.
        /// </summary>

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isConnectionOwned && Connection != null && disposing)
            {
                if (Connection.State == ConnectionState.Open)
                {
                    Connection.Close();
                }

                Connection.Dispose();
                Connection = null;
            }
        }

        private void Open()
        {
            try
            {
                if (Connection.State == ConnectionState.Closed)
                {
                    Connection.Open();
                    OnOpened();
                }
            }
            catch (Exception exp)
            {
                throw new ConnectionFailedException("Could not connect to the database at : " + Connection.ConnectionString, exp);
            }
        }

        protected virtual void OnOpened()
        {
            // do nothing in the base case
        }

        public virtual string MakeIdentifier(params string[] parts)
        {
            var identifier = string.Join(IdentifierPartSeperator, parts
                .Where(p => p != null)
                .Select(p =>
                {
                    if (p.StartsWith(IdentifierPartBegin, StringComparison.InvariantCulture))
                    {
                        return p;
                    }
                    else
                    {
                        return $"{IdentifierPartBegin}{p}{IdentifierPartEnd}";
                    }
                })
                .ToArray());
            return identifier;
        }

        public abstract string MakeRoutineIdentifier(RoutineAttribute routine, bool withParamNames);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text,
            Query = "select count(*) from ScriptStatus")]
        public int GetDatabaseVersion()
        {
            return Get<int>();
        }

        public void AlterDatabase(ScriptStatus script)
        {
            if (script is null)
            {
                throw new ArgumentNullException(nameof(script));
            }

            ExecuteQuery(script.Script);
            if (script.ScriptType == ScriptType.InitializeData)
            {
                try
                {
                    MarkScriptAsRan(script);
                }
                catch (DbException exp)
                {
                    if (!exp.Message.ToLowerInvariant().Contains("scriptstatus"))
                    {
                        throw;
                    }
                }
            }
        }

        public void MarkScriptAsRan(ScriptStatus script)
        {
            if (script is null)
            {
                throw new ArgumentNullException(nameof(script));
            }

            if (script.ScriptType == ScriptType.InitializeData)
            {
                this.InsertOne(script);
            }
        }

        /// <summary>
        /// Performs a basic insert operation for a collection of data. By default, this will perform poorly, as it does
        /// a naive INSERT statement for each element of the array. It is meant as a base implementation that can be overridden
        /// in deriving classes to support their vendor's bulk-insert operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public virtual void InsertAll(Type t, System.Collections.IEnumerable data)
        {
            if (t is null)
            {
                throw new ArgumentNullException(nameof(t));
            }

            if (data != null)
            {
                var attr = DatabaseObjectAttribute.GetTable(t);
                if (attr == null)
                {
                    throw new Exception($"Type {t.Namespace}.{t.Name} could not be automatically inserted.");
                }

                // don't upload auto-incrementing identity columns
                // or columns that have a default value defined
                var props = GetProperties(t);
                var columns = props
                    .Where(p => p.Include && !p.IsIdentity && (p.IsIncludeSet || p.DefaultValue == null))
                    .ToArray();
                var methParams = columns.Select(c => c.ToParameter()).ToArray();

                var columnNames = columns.Select(c => c.Name);
                var parameterNames = columnNames.Select(c => "@" + c);

                var tableName = MakeIdentifier(attr.Schema ?? DefaultSchemaName, attr.Name);
                var columnNamesStr = string.Join(", ", columnNames);
                var parameterNamesStr = string.Join(", ", parameterNames);
                var query = $"insert into {tableName}({columnNamesStr}) values({parameterNamesStr})";

                using var command = BuildCommand(query, CommandType.Text, methParams);
                foreach (var obj in data)
                {
                    var parameterValues = new object[columns.Length];
                    for (var i = 0; i < columns.Length; ++i)
                    {
                        parameterValues[i] = columns[i].GetValue<object>(obj);
                    }
                    CopyParameterValues(command, parameterValues.ToArray());
                    _ = command.ExecuteNonQuery();
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
        private CommandT ConstructCommand(params object[] parameterValues) { return ConstructCommand(false, parameterValues); }
        private CommandT ConstructCommand(bool isPrimitive, params object[] parameterValues)
        {
            // because this method is private, it will never be called directly by a method tagged
            // with RoutineAttribute. Therefore, we can skip two methods in the stack trace to
            // try to get to the one we're interested in--this one and the caller of this one.
            var method = (from frame in new StackTrace(2, false).GetFrames()
                          let meth = frame.GetMethod()
                          where meth is MethodInfo
                            && RoutineAttribute.GetCommandDescription((MethodInfo)meth) != null
                          select (MethodInfo)meth).FirstOrDefault();

            // We absolutely need to find a method with this attribute, because we won't know where in
            // the stack trace to stop, so that we can inspect the method for parameter name info.
            if (method == null)
            {
                throw new Exception("Could not find a mapped method!");
            }

            var meta = RoutineAttribute.GetCommandDescription(method);

            // since the parameters array was passed to this method by the mapped method, the mapped
            // method has the responsibility to pass the parameters in the same order they are
            // specified in the method signature
            if (meta.Parameters.Count != parameterValues.Length)
            {
                if (isPrimitive && meta.Parameters.Count == parameterValues.Length + 1)
                {
                    throw new Exception("When querying for a single value, the first parameter passed to the processing method will be treated as a column name or column index to map out of the query result set.");
                }
                else if (meta.Parameters.Count < parameterValues.Length)
                {
                    throw new Exception("More parameters were passed to the processing method than were specified in the mapped method signature.");
                }
                else
                {
                    throw new Exception("Fewer parameters were passed to the processing method than were specified in the mapped method signature.");
                }
            }

            var procName = MakeIdentifier(meta.Schema ?? DefaultSchemaName, meta.Name);
            var methParams = meta.Parameters.ToArray();
            var command = BuildCommand(meta.CommandType == CommandType.Text ? meta.Query : procName, meta.CommandType, methParams);
            CopyParameterValues(command, parameterValues);

            if (meta.Timeout > -1)
            {
                command.CommandTimeout = meta.Timeout;
            }

            return command;
        }

        private CommandT BuildCommand(string procName, CommandType commandType, ParameterAttribute[] methParams)
        {
            // the mapped method must match the name of a stored procedure in the database, or the
            // query or procedure name must be provided explicitly in the RoutineAttribute's
            // Query property.
            var command = new CommandT
            {
                Connection = Connection,
                CommandType = commandType,
                // meta.Query defaults to null, so default behavior is to use the procedure name
                CommandText = procName
            };
            command.Parameters.AddRange(MakeProcedureParameters(methParams, commandType == CommandType.StoredProcedure));
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
                JiggerParameter((ParameterT)command.Parameters[i], command.CommandType == CommandType.StoredProcedure);
            }
        }

        private ParameterT[] MakeProcedureParameters(ParameterAttribute[] methParams, bool isProc)
        {
            var procedureParams = new List<ParameterT>();
            for (var i = 0; methParams != null && i < methParams.Length; ++i)
            {
                var p = new ParameterT
                {
                    ParameterName = methParams[i].Name,
                    Direction = methParams[i].Direction
                };

                procedureParams.Add(p);
            }
            return procedureParams.ToArray();
        }

        protected virtual void JiggerParameter(ParameterT p, bool isProc)
        {
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
        public EntityT Get<EntityT>(params object[] parameters)
        {
            // the ToList call is necessary to consume the entire enumerator
            return GetEnumerator<EntityT>(parameters).ToList().FirstOrDefault();
        }

        public EntityT Return<EntityT>(params object[] parameters)
        {
            using var cmd = ConstructCommand(parameters);
            var p = new ParameterT
            {
                Direction = ParameterDirection.ReturnValue
            };
            _ = cmd.Parameters.Add(p);
            _ = cmd.ExecuteNonQuery();
            return (EntityT)p.Value;
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
                _ = command.ExecuteNonQuery();
                CopyOutputParameters(parameters, command.Parameters);
            }
            catch (Exception exp)
            {
                throw new Exception($"Could not execute command: {command.CommandText}. Reason: {exp.Message}.", exp);
            }
        }

        protected virtual void ExecuteQuery(string query)
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                using var command = BuildCommand(query, CommandType.Text, null);
                Execute(command);
            }
        }

        public void Execute(params object[] parameters)
        {
            using var command = ConstructCommand(parameters);
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
        public List<EntityT> GetList<EntityT>(params object[] parameters)
        {
            return GetEnumerator<EntityT>(parameters).ToList();
        }

        private DataSet GetDataSet(CommandT command, params object[] parameters)
        {
            using var adapter = new DataAdapterT
            {
                SelectCommand = command
            };
            //Open(); // open is not necessary for DataAdapter.Fill()

            var result = new DataSet();
            _ = adapter.Fill(result);
            CopyOutputParameters(parameters, command.Parameters);
            return result;
        }

        public DataSet GetDataSet(params object[] parameters)
        {
            using var command = ConstructCommand(parameters);
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
                    throw new Exception($"Could not execute: {command.CommandText}. Reason: {exp.Message}.", exp);
                }
            }
            return (DataReaderT)reader;
        }

        public DbDataReader GetReader(params object[] parameters)
        {
            using var command = ConstructCommand(parameters);
            return GetReader(command, parameters);
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
        public IEnumerable<EntityT> GetEnumerator<EntityT>(params object[] parameters)
        {
            var entityType = typeof(EntityT);

            // If we're returning a primitive value, we assume it's in the first position
            // of the result set.
            var key = DataConnector.IsTypePrimitive(entityType) ? (object)0 : entityType;

            using var command = ConstructCommand(parameters);
            using var reader = GetReader(command, parameters);
            Func<object> getter = null;

            if (key is string)
            {
                throw new Exception("String keys are no longer supported. Use integer index instead.");
            }
            else if (key is int)
            {
                getter = () => reader[(int)key];
            }
            else if (key is Type type) // A raw entity type must have a default constructor. We need to be able to create an
                                       // uninitialized version of the object so that we can set the fields to the
                                       // mapped values later
            {
                var columnNames = new string[reader.FieldCount];
                var columnTypes = new Type[reader.FieldCount];
                for (var i = 0; i < reader.FieldCount; ++i)
                {
                    columnNames[i] = reader.GetName(i);
                    columnTypes[i] = reader.GetFieldType(i);
                }

                var useTypedConstructor = true;
                var constructor = type.GetConstructor(columnTypes);
                if (constructor == null)
                {
                    useTypedConstructor = false;
                    constructor = type.GetConstructor(Type.EmptyTypes);
                    if (constructor == null)
                    {
                        var columnDefs = columnNames.Select((n, i) => $"{i}: {n} {columnTypes[i].Name}");
                        var columnDefString = string.Join("\n", columnDefs);
                        throw new Exception($"Entity classes need a default constructor or a constructor that matches the result set. This data reader had columns:\n{columnDefString}");
                    }
                }

                if (useTypedConstructor)
                {
                    var values = new object[reader.FieldCount];
                    getter = delegate ()
                    {
                        _ = reader.GetValues(values);
                        return (EntityT)constructor.Invoke(values);
                    };
                }
                else
                {
                    var props = GetProperties(type).ToDictionary(p => p.Name);
                    getter = delegate ()
                    {
                        var inst = (EntityT)constructor.Invoke(null);

                        for (var i = 0; i < columnNames.Length; ++i)
                        {
                            if (props.ContainsKey(columnNames[i]))
                            {
                                props[columnNames[i]].SetValue(inst, reader[columnNames[i]]);
                            }
                        }

                        return inst;
                    };
                }
            }
            else
            {
                throw new ArgumentException("When retrieving a primitive data type, the first parameter to the procedure must be an integer column index. Given: " + key.GetType().Name);
            }
            while (reader.Read())
            {
                var val = getter();
                if (val == DBNull.Value)
                {
                    yield return default;
                }
                else
                {
                    yield return (EntityT)val;
                }
            }
        }

        public virtual DatabaseState GetInitialState(string catalogueName, Regex filter)
        {
            return new DatabaseState(catalogueName, filter, this);
        }

        public virtual DatabaseState GetFinalState(Type dalType, string userName, string password, string database)
        {
            var currentType = dalType;
            var types = new List<Type>();
            while (currentType != typeof(object)
                && currentType is object)
            {
                var asm = currentType.Assembly;
                types.AddRange(asm.GetTypes());
                currentType = currentType.BaseType;
            }

            return new DatabaseState(types, this, this, userName, password, database);
        }

        protected string ArgumentList<T>(IEnumerable<T> collect, Func<T, string> format, string separator = null)
        {
            var arr = collect.ToArray();
            separator ??= $",{Environment.NewLine}    ";
            var str = string.Join(separator, arr.Select(format));
            return str;
        }

        protected string MakeColumnSection(TableAttribute info, bool isReturnType)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var columns = info.Properties.Where(p => p.Include).ToArray();
            return ArgumentList(columns, p => MakeColumnString(p, isReturnType).Trim());
        }

        protected string MakeSqlTypeString(DatabaseObjectAttribute p, bool skipSize, bool isArray)
        {
            if (p != null && p.Include)
            {
                var isIdentity = p is ColumnAttribute && ((ColumnAttribute)p).IsIdentity;
                var systemType = p.SystemType;
                if (systemType != null)
                {
                    if (systemType.IsEnum)
                    {
                        systemType = typeof(int);
                    }
                    else if (systemType == typeof(void))
                    {
                        systemType = null;
                    }
                }
                return MakeSqlTypeString(
                    p.SqlType,
                    systemType,
                    p.IsSizeSet ? new int?(p.Size) : null,
                    p.IsPrecisionSet ? new int?(p.Precision) : null,
                    isIdentity,
                    skipSize,
                    isArray);
            }
            else
            {
                return null;
            }
        }

        public virtual string RoutineChanged(RoutineAttribute a, RoutineAttribute b)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b is null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            var scriptA = MakeCreateRoutineScript(a, false);
            var scriptB = MakeCreateRoutineScript(b, false);
            if (scriptA == scriptB)
            {
                return null;
            }
            else
            {
                var changedParameters = a.Parameters.Count != b.Parameters.Count;
                if (!changedParameters)
                {
                    var paramChanges = a.Parameters.Select((p1, i) =>
                    {
                        var p2 = b.Parameters[i];
                        var tests = new bool[]{
                            p1.Name.ToLowerInvariant() != p2.Name.ToLowerInvariant(),
                            p1.DefaultValue != p2.DefaultValue,
                            p1.Direction != p2.Direction,
                            p1.IsOptional != p2.IsOptional,
                            p1.SystemType != p2.SystemType,
                            p1.Precision != p2.Precision,
                            p1.Size != p2.Size
                        };
                        var changed2 = tests.Any(t => t);
                        return changed2;
                    }).ToArray();
                    changedParameters = paramChanges.Any(t => t);
                }
                var finalScript = MakeRoutineBody(a).Trim();
                var initialScript = b.Query.Trim();
                var changedQuery = finalScript != initialScript;
                return changedParameters
                    ? "Parameters have changed"
                    : changedQuery
                        ? "Routine body has changed"
                        : null;
            }
        }

        public virtual string ColumnChanged(ColumnAttribute final, ColumnAttribute initial)
        {
            if (final is null)
            {
                throw new ArgumentNullException(nameof(final));
            }

            if (initial is null)
            {
                throw new ArgumentNullException(nameof(initial));
            }

            string changeReason = null;
            var finalType = final.SystemType;
            if (finalType.IsEnum)
            {
                finalType = typeof(int);
            }

            if (final.Include && !initial.Include)
            {
                changeReason = "Column doesn't exist";
            }
            else if (!final.Include && initial.Include)
            {
                changeReason = "Column no longer exists";
            }
            else if (final.IsOptional && !initial.IsOptional)
            {
                changeReason = "Column is now optional";
            }
            else if (!final.IsOptional && initial.IsOptional)
            {
                changeReason = "Column is no longer optional";
            }
            else if (final.Name.ToLowerInvariant() != initial.Name.ToLowerInvariant())
            {
                // this should never happen, because we match columns by name anyway
                changeReason = $"Column name has changed. Was {initial.Name}, now {final.Name}";
            }
            else if (finalType != initial.SystemType)
            {
                changeReason = $"Column type has changed. Was {initial.SystemType?.Name ?? initial.SqlType}, now {finalType.Name}.";
            }
            else if (final.Size != initial.Size)
            {
                changeReason = $"Column size has changed. Was {initial.Size}, now {final.Size}.";
            }
            else if (final.Precision != initial.Precision)
            {
                changeReason = $"Column precision has changed. Was {initial.Precision}, now {final.Precision}.";
            }
            else if (final.DefaultValue != initial.DefaultValue)
            {
                changeReason = CheckDefaultValueDifference(final, initial);
            }
            return changeReason;
        }

        public virtual string RelationshipChanged(Relationship a, Relationship b)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b is null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            var aFromTableName = MakeIdentifier(a.From.Schema, a.From.Name).ToLowerInvariant();
            var aFromColumnNames = a.FromColumns.Select(c => c.Name.ToLowerInvariant());
            var aToTableName = MakeIdentifier(a.To.Schema, a.To.Name).ToLowerInvariant();
            var bFromTableName = MakeIdentifier(b.From.Schema, b.From.Name).ToLowerInvariant();
            var bFromColumnNames = b.FromColumns.Select(c => c.Name.ToLowerInvariant());
            var bToTableName = MakeIdentifier(b.To.Schema, b.To.Name).ToLowerInvariant();
            var changed = aFromTableName != bFromTableName
                || aToTableName != bToTableName
                || aFromColumnNames.Any(c => !bFromColumnNames.Contains(c))
                || bFromColumnNames.Any(c => !aFromColumnNames.Contains(c));
            return changed ? "IDK" : null;
        }

        public virtual string IndexChanged(TableIndex a, TableIndex b)
        {
            if (a is null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b is null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            var aTableName = MakeIdentifier(a.Table.Schema, a.Table.Name);
            var bTableName = MakeIdentifier(b.Table.Schema, b.Table.Name);
            var aColumnNames = a.Columns.Select(c => c.ToLowerInvariant());
            var bColumnNames = b.Columns.Select(c => c.ToLowerInvariant());
            var changed = aTableName != bTableName
                || aColumnNames.Any(c => !bColumnNames.Contains(c))
                || bColumnNames.Any(c => !aColumnNames.Contains(c));
            return changed ? "IDK" : null;
        }

        public virtual string KeyChanged(PrimaryKey final, PrimaryKey initial)
        {
            if (final is null)
            {
                throw new ArgumentNullException(nameof(final));
            }

            if (initial is null)
            {
                throw new ArgumentNullException(nameof(initial));
            }

            if (final.Name.ToLowerInvariant() != initial.Name.ToLowerInvariant())
            {
                return "Primary key name changed";
            }
            else if (final.KeyColumns.Length < initial.KeyColumns.Length)
            {
                return "Primary key columns were removed";
            }
            else if (final.KeyColumns.Length > initial.KeyColumns.Length)
            {
                return "Primary key columns were added";
            }
            else
            {
                var mismatchedColumns = final.KeyColumns
                    .Select((f, i) => ColumnChanged(f, initial.KeyColumns[i]))
                    .Where(r => r != null)
                    .ToArray();
                return mismatchedColumns.Length > 0 ? string.Join(", ", mismatchedColumns) : null;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select schema_name from information_schema.schemata;")]
        public virtual List<string> GetSchemata()
        {
            return GetList<string>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query =
@"select script from ScriptStatus;")]
        public virtual List<string> GetScriptStatus()
        {
            return GetList<string>();
        }

        public virtual string MakeCreateCatalogueScript(string catalogueName)
        {
            return $"create database {catalogueName};";
        }

        private string MakeSchemaScript(string op, string schemaName)
        {
            schemaName = MakeIdentifier(schemaName);
            return $"{op} schema {schemaName};";
        }

        public virtual string MakeCreateSchemaScript(string schemaName)
        {
            return MakeSchemaScript("create", schemaName);
        }

        public virtual string MakeDropSchemaScript(string schemaName)
        {
            return MakeSchemaScript("drop", schemaName);
        }

        public event IOEventHandler OnStandardOutput;
        public event IOEventHandler OnStandardError;
        protected virtual void ToOutput(string value)
        {
            OnStandardOutput?.Invoke(this, new IOEventArgs(value));
        }
        protected void ToError(string value)
        {
            OnStandardError?.Invoke(this, new IOEventArgs(value));
        }

        protected bool RunProcess(string path, string[] args)
        {
            var succeeded = true;
            var shortName = new System.IO.FileInfo(path).Name;
            var argsString = string.Join(" ", args);
            ToOutput($":> {shortName} {argsString}\r\n");
            var procInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = string.Join(" ", args.Where(s => s != null)),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                ErrorDialog = true,
            };

            using var proc = new Process
            {
                StartInfo = procInfo,
                EnableRaisingEvents = true
            };

            if (succeeded = proc.Start())
            {
                proc.WaitForExit();
                while (proc.StandardOutput.Peek() != -1)
                {
                    ToOutput(proc.StandardOutput.ReadLine());
                }
                while (proc.StandardError.Peek() != -1)
                {
                    succeeded = false;
                    ToError(proc.StandardError.ReadLine());
                }
            }

            return succeeded;
        }

        public abstract List<string> GetDatabaseLogins();
        public abstract List<InformationSchema.Columns> GetTableColumns();
        public abstract List<InformationSchema.Columns> GetUDTTColumns();
        public abstract List<InformationSchema.Views> GetViews();
        public abstract List<InformationSchema.Columns> GetViewColumns();
        public abstract List<InformationSchema.IndexColumnUsage> GetIndexColumns();
        public abstract List<InformationSchema.TableConstraints> GetTableConstraints();
        public abstract List<InformationSchema.ReferentialConstraints> GetReferentialConstraints();
        public abstract List<InformationSchema.Routines> GetRoutines();
        public abstract List<InformationSchema.Parameters> GetParameters();
        public abstract List<InformationSchema.ConstraintColumnUsage> GetConstraintColumns();
        public abstract List<InformationSchema.KeyColumnUsage> GetKeyColumns();

        public abstract string DefaultSchemaName { get; }
        public abstract int GetDefaultTypePrecision(string typeName, int testPrecision);
        public abstract bool HasDefaultTypeSize(Type type);
        public abstract int GetDefaultTypeSize(Type type);
        public abstract Type GetSystemType(string sqlType);
        public abstract string NormalizeSqlType(string sqlType);
        public abstract bool DescribesIdentity(InformationSchema.Columns column);
        protected abstract string CheckDefaultValueDifference(ColumnAttribute final, ColumnAttribute initial);
        public virtual void AnalyzeQuery(string routineText, RoutineAttribute routine)
        {
            if (routineText is null)
            {
                throw new ArgumentNullException(nameof(routineText));
            }

            if (routine is null)
            {
                throw new ArgumentNullException(nameof(routine));
            }

            routine.Query = routineText;
        }

        protected virtual string MakeParameterSection(RoutineAttribute info, bool withNames)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var parameters = info.Parameters.Select(p => MakeParameterString(p, withNames));
            var parameterSection = string.Join(", ", parameters);
            return parameterSection;
        }

        public string MakeCreateUDTTScript(TableAttribute info)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var columns = info.Properties
                .Where(p => p.Include
                && !p.IsIdentity
                && (p.DefaultValue == null || p.IsIncludeSet))
                .ToArray();
            if (columns.Length == 0)
            {
                throw new TableHasNoColumnsException(info);
            }
            var columnSection = ArgumentList(columns, p => MakeColumnString(p, false).Trim());

            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = MakeIdentifier(schema, info.Name);
            return $@"create type {identifier} as {UDTTDeclarationPrefix}(
    {columnSection}
);";
        }

        public string MakeDropUDTTScript(TableAttribute info)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = MakeIdentifier(schema, info.Name);
            return $@"drop type {identifier};";
        }


        protected abstract string MakeSqlTypeString(string sqlType, Type systemType, int? size, int? precision, bool isIdentity, bool skipSize, bool isArray);
        protected abstract string MakeColumnString(ColumnAttribute p, bool isReturnType);
        protected abstract string MakeParameterString(ParameterAttribute p, bool withName);

        public abstract bool SupportsScriptType(ScriptType type);
        public abstract bool IsUDTT(Type systemType);
        public abstract TableAttribute MakeUDTTTableAttribute(Type type);
        public abstract string MakeInsertScript(TableAttribute table, object value);
        public abstract string MakeCreateDatabaseLoginScript(string userName, string password, string database);

        public abstract string MakeCreateTableScript(TableAttribute table);
        public abstract string MakeDropTableScript(TableAttribute table);

        public abstract string MakeCreateViewScript(ViewAttribute view);
        public abstract string MakeDropViewScript(ViewAttribute view);

        public abstract string MakeCreateColumnScript(ColumnAttribute column);
        public abstract string MakeDropColumnScript(ColumnAttribute column);
        public abstract string MakeAlterColumnScript(ColumnAttribute final, ColumnAttribute initial);

        public abstract string MakeDropRoutineScript(RoutineAttribute routine);
        public abstract string MakeRoutineBody(RoutineAttribute routine);
        public abstract string MakeCreateRoutineScript(RoutineAttribute routine, bool createBody = true);

        public abstract string MakeDropRelationshipScript(Relationship relation);
        public abstract string MakeCreateRelationshipScript(Relationship relation);

        public abstract string MakeDropPrimaryKeyScript(PrimaryKey key);
        public abstract string MakeCreatePrimaryKeyScript(PrimaryKey key);
        public abstract string MakeDropIndexScript(TableIndex index);
        public abstract string MakeCreateIndexScript(TableIndex index);

        public abstract string DataSource { get; }

        public abstract bool RunCommandLine(string executablePath, string configurationPath, string server, string database, string adminUser, string adminPass, string query);
    }
}