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
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using SqlSiphon.Mapping;

namespace SqlSiphon
{
    /// <summary>
    /// A base class for building Data Access Layers that connect to MS SQL Server 2005/2008
    /// databases and execute store procedures stored within.
    /// </summary>
    public abstract class DataAccessLayer<ConnectionT, CommandT, ParameterT, DataAdapterT, DataReaderT> :
        IDisposable
        where ConnectionT : DbConnection, new()
        where CommandT : DbCommand, new()
        where ParameterT : DbParameter, new()
        where DataAdapterT : DbDataAdapter, new()
        where DataReaderT : DbDataReader
    {
        private static List<Type> Synced = new List<Type>();
        private bool isConnectionOwned;

        protected virtual string IdentifierPartBegin { get { return ""; } }
        protected virtual string IdentifierPartEnd { get { return ""; } }
        protected virtual string IdentifierPartSeperator { get { return "."; } }
        protected virtual string DefaultSchemaName { get { return null; } }

        public ConnectionT Connection { get; private set; }

        /// <summary>
        /// creates a new connection to a MS SQL Server 2005/2008 database and automatically
        /// opens the connection. 
        /// </summary>
        /// <param name="connectionString">a standard MS SQL Server connection string</param>
        private DataAccessLayer(ConnectionT connection, bool isConnectionOwned)
        {
            this.Connection = connection;
            this.isConnectionOwned = isConnectionOwned;
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

        protected void SynchronizeProcedures()
        {
            var t = this.GetType();
            if (!Synced.Contains(t))
            {
                this.DropProcedures();
                this.PreCreateProcedures();
                this.CreateProcedures();
                Synced.Add(t);
            }
        }

        protected virtual void PreCreateProcedures() { }

        /// <summary>
        /// Creates a stored procedure command call from a variable array of parameters. The stored
        /// procedure to call is determined by walking the Stack Trace to find a method with the
        /// MappedMethodAttribute attribute applied to it. This method, once found, must have the same
        /// name as a stored procedure in the database.
        /// </summary>
        /// <param name="parameterValues">a variable number of parameters to pass to the stored procedure.
        /// </param>
        /// <returns>a SqlCommand structure for encapsulating a stored procedure call</returns>
        private CommandT ConstructCommand(params object[] parameterValues)
        {
            // because this method is private, it will never be called directly by a method tagged
            // with MappedMethodAttribute. Therefore, we can skip two methods in the stack trace to
            // try to get to the one we're interested in--this one and the caller of this one.
            var method = (from frame in new StackTrace(2, false).GetFrames()
                          let meth = frame.GetMethod()
                          where meth is MethodInfo
                            && MappedObjectAttribute.GetAttribute<MappedMethodAttribute>(meth) != null
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
                throw new Exception("The mapped method's parameter list is a different length than what was passed to the processing method.");

            // To support calling stored procedures from non-default schemas:
            var procName = MakeIdentifier(meta.Schema, meta.Name);

            var command = BuildCommand(
                meta.CommandType == CommandType.Text ? meta.Query : procName,
                meta.CommandType,
                meta.Parameters.ToArray(),
                parameterValues);

            if (meta.Timeout > -1)
                command.CommandTimeout = meta.Timeout;
            return command;
        }

        protected string MakeIdentifier(params string[] parts)
        {
            return string.Join(IdentifierPartSeperator, parts
                .Where(p => p != null)
                .Select(p => string.Format("{0}{1}{2}", IdentifierPartBegin, p, IdentifierPartEnd))
                .ToArray());
        }

        protected virtual CommandT BuildCommand(string procName, CommandType commandType, MappedParameterAttribute[] methParams, object[] parameterValues)
        {
            // the mapped method must match the name of a stored procedure in the database, or the
            // query or procedure name must be provided explicitly in the MappedMethodAttribute's
            // Query property.
            var command = new CommandT();
            command.Connection = Connection;
            command.CommandType = commandType;
            // meta.Query defaults to null, so default behavior is to use the procedure name
            command.CommandText = procName;
            command.Parameters.AddRange(MakeProcedureParameters(methParams, parameterValues));
            return command;
        }

        private ParameterT[] MakeProcedureParameters(MappedParameterAttribute[] methParams, object[] parametersValues)
        {
            List<ParameterT> procedureParams = new List<ParameterT>();
            for (int i = 0; methParams != null && i < methParams.Length; ++i)
            {
                var p = new ParameterT();
                p.ParameterName = methParams[i].Name;
                p.Value = PrepareParameter(parametersValues[i]) ?? DBNull.Value;
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

        protected EntityT GetQuery<EntityT>(string query)
        {
            return this.GetListQuery<EntityT>(query).FirstOrDefault();
        }

        protected List<EntityT> GetListQuery<EntityT>(string query)
        {
            using (var command = BuildCommand(query, CommandType.Text, null, null))
            {
                using (var reader = GetReader(command))
                {
                    var type = typeof(EntityT);
                    // The most basic case is returning values from one column in the table.
                    // The first parameter is skipped because it's the column name to retrieve,
                    // not a parameter to the stored procedure
                    IEnumerable<EntityT> iter = null;
                    if (type.IsPrimitive
                        || type == typeof(decimal)
                        || type == typeof(string)
                        || type == typeof(DateTime)
                        || type == typeof(Guid))
                    {
                        iter = Iterate<EntityT>(reader, 0);
                    }
                    else
                    {
                        iter = Iterate<EntityT>(reader, type);
                    }
                    // the ToList call is necessary to consume the entire enumerator
                    return iter.ToList();
                }
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
                using (var command = BuildCommand(query, CommandType.Text, null, null))
                    Execute(command);
        }

        protected void Execute(params object[] parameters)
        {
            using (var command = ConstructCommand(parameters))
                Execute(command, parameters);
        }

        private static void CopyOutputParameters(object[] parameters, DbParameterCollection sqlParameters)
        {
            for (int i = 0; sqlParameters != null && i < sqlParameters.Count; ++i)
                if (sqlParameters[i].Direction == ParameterDirection.InputOutput ||
                    sqlParameters[i].Direction == ParameterDirection.Output)
                    parameters[i] = sqlParameters[i].Value;
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

        protected DataSet GetDataSetQuery(string query)
        {
            using (var command = BuildCommand(query, CommandType.Text, null, null))
                return GetDataSet(command);
        }

        private void Open()
        {
            if (Connection.State == ConnectionState.Closed)
                Connection.Open();
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

        protected DataReaderT GetReaderQuery(string query)
        {
            using (var command = BuildCommand(query, CommandType.Text, null, null))
                return GetReader(command);
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
                var columnNames = GetColumnNames(reader);
                var fields = GetSettableMembers(type);
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


        internal static string[] GetColumnNames(DbDataReader reader)
        {
            var columnNames = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; ++i)
                columnNames[i] = reader.GetName(i);
            return columnNames;
        }

        private static BindingFlags PATTERN = BindingFlags.Public | BindingFlags.Instance;
        protected static UnifiedSetter[] GetSettableMembers(Type type)
        {
            return type
                .GetProperties(PATTERN)
                .Where(p =>
                {
                    var attr = MappedObjectAttribute.GetAttribute<MappedPropertyAttribute>(p);
                    return attr == null || !attr.Ignore;
                })
                .Select(p => new UnifiedSetter(p))
                .ToArray();
        }

        /// <summary>
        /// copies the values of every field in the row to the field in the object that has the same name. 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="reader"></param>
        /// <param name="columnNames"></param>
        /// <param name="fields"></param>
        internal static void DoMapping(object obj, DataReaderT reader, string[] columnNames, UnifiedSetter[] fields)
        {
            fields.Where(f => columnNames.Contains(f.Name))
                .ToList()
                .ForEach(f => f.SetValue(obj, reader[f.Name]));
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
            bool isPrimitive = type.IsPrimitive
                || type == typeof(decimal)
                || type == typeof(decimal?)
                || type == typeof(string)
                || type == typeof(DateTime)
                || type == typeof(DateTime?)
                || type == typeof(Guid)
                || type == typeof(Guid?)
                || type == typeof(byte[]);

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

        protected List<MappedMethodAttribute> FindProcedureDefinitions()
        {
            var t = this.GetType();
            var methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            var results = new List<MappedMethodAttribute>();
            foreach (var method in methods)
            {
                var info = this.GetCommandDescription(method);
                if (info != null)
                    results.Add(info);
            }
            return results;
        }

        private MappedMethodAttribute GetCommandDescription(MethodInfo method)
        {
            var meta = MappedObjectAttribute.GetAttribute<MappedMethodAttribute>(method);
            if (meta != null)
            {
                if (meta.CommandType == CommandType.TableDirect)
                    throw new NotImplementedException("Table-Direct queries are not supported by SqlSiphon");
                meta.Study(method);
                if (meta.SqlType == null)
                    meta.SqlType = this.MakeSqlTypeString(meta);

                if (meta.Schema == null)
                    meta.Schema = this.DefaultSchemaName;
                foreach (var parameter in meta.Parameters)
                    if (parameter.SqlType == null)
                        parameter.SqlType = this.MakeSqlTypeString(parameter);
                this.ModifyQuery(meta);
            }
            return meta;
        }

        /// <summary>
        /// Scans the Data Access Layer for public methods that have stored procedure
        /// definitions, and creates/alters procedures as necessary to bring them all
        /// up to date.
        /// </summary>
        public void DropProcedures()
        {
            var t = this.GetType();
            var procSignatures = t.GetMethods();
            foreach (var procSignature in procSignatures)
            {
                DropProcedure(procSignature);
            }
        }

        public void CreateProcedures()
        {
            var t = this.GetType();
            var procSignatures = t.GetMethods();
            foreach (var procSignature in procSignatures)
            {
                CreateProcedure(procSignature);
            }
        }

        private void DropProcedure(MethodInfo method)
        {
            var info = GetCommandDescription(method);
            if (info != null
                && info.CommandType == CommandType.StoredProcedure
                && !string.IsNullOrEmpty(info.Query))
            {
                var schema = info.Schema ?? DefaultSchemaName;
                var identifier = this.MakeIdentifier(schema, info.Name);
                if (this.ProcedureExists(info))
                {
                    var script = BuildDropProcedureScript(info);
                    try
                    {
                        this.ExecuteQuery(script);
                    }
                    catch (Exception exp)
                    {
                        throw new Exception(string.Format("Could not drop procedure: {0}. Reason: {1}", identifier, exp.Message), exp);
                    }
                }
            }
        }

        private void CreateProcedure(MethodInfo method)
        {
            var info = GetCommandDescription(method);
            if (info != null
                && info.CommandType == CommandType.StoredProcedure
                && !string.IsNullOrEmpty(info.Query))
            {
                var schema = info.Schema ?? DefaultSchemaName;
                var identifier = this.MakeIdentifier(schema, info.Name);
                var script = BuildCreateProcedureScript(info);
                try
                {
                    this.ExecuteQuery(script);
                }
                catch (Exception exp)
                {
                    throw new Exception(string.Format("Could not create procedure: {0}. Reason: {1}", identifier, exp.Message), exp);
                }
            }
        }

        protected string MakeParameterSection(MappedMethodAttribute info)
        {
            var parameterSection = string.Join(
                         "," + Environment.NewLine + "    ",
                         info.Parameters
                             .Select(p => this.MakeParameterString(p))
                             .ToArray());
            return parameterSection;
        }


        protected virtual void ModifyQuery(MappedMethodAttribute info)
        {
            //do nothing in the base case
        }

        protected virtual object PrepareParameter(object val)
        {
            //do nothing in the base case
            return val;
        }

        protected virtual void ExcecuteCreateProcedureScript(string script)
        {
            this.ExecuteQuery(script);
        }

        protected abstract string MakeSqlTypeString(MappedTypeAttribute type);
        protected abstract bool ProcedureExists(MappedMethodAttribute info);
        protected abstract string BuildDropProcedureScript(MappedMethodAttribute info);
        protected abstract string BuildCreateProcedureScript(MappedMethodAttribute info);
        protected abstract string MakeParameterString(MappedParameterAttribute p);
    }
}