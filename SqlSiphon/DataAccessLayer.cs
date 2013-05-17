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

namespace SqlSiphon
{
    public interface IDataAccessLayer<ParameterT, DataReaderT>
        where ParameterT : DbParameter, new()
        where DataReaderT : DbDataReader
    {
        ParameterT[] MakeProcedureParameters(ParameterInfo[] methParams, object[] parametersValues);
        MappedMethodAttribute GetCommandDescription(MethodBase method);
        EntityT GetQuery<EntityT>(string query);
        void ExecuteQuery(string query);
        DataReaderT GetReaderQuery(string query);
    }

    /// <summary>
    /// A base class for building Data Access Layers that connect to MS SQL Server 2005/2008
    /// databases and execute store procedures stored within.
    /// </summary>
    public abstract class DataAccessLayer<ConnectionT, CommandT, ParameterT, DataAdapterT, DataReaderT> :
        IDataAccessLayer<ParameterT, DataReaderT>,
        IDisposable
        where ConnectionT : DbConnection, new()
        where CommandT : DbCommand, new()
        where ParameterT : DbParameter, new()
        where DataAdapterT : DbDataAdapter, new()
        where DataReaderT : DbDataReader
    {
        private ConnectionT connections;
        private bool isConnectionOwned;

        private static Dictionary<Type, string> ConnectionStrings;
        private static Dictionary<Type, int> ConnectionCount;

        static DataAccessLayer()
        {
            ConnectionStrings = new Dictionary<Type, string>();
            ConnectionCount = new Dictionary<Type, int>();
        }

        public ConnectionT Connection
        {
            get
            {
                return connections;
            }
        }

        /// <summary>
        /// creates a new connection to a MS SQL Server 2005/2008 database and automatically
        /// opens the connection. 
        /// </summary>
        /// <param name="connectionString">a standard MS SQL Server connection string</param>
        private DataAccessLayer(ConnectionT connection, bool isConnectionOwned)
        {
            this.connections = connection;
            this.isConnectionOwned = isConnectionOwned;
        }

        protected DataAccessLayer(string connectionString)
            : this(new ConnectionT { ConnectionString = connectionString }, true)
        {
            var t = this.GetType();
            if (ConnectionStrings.ContainsKey(t))
                ConnectionStrings[t] = connectionString;
            else
                ConnectionStrings.Add(t, connectionString);
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

        /// <summary>
        /// Cleans up the connection with the database.
        /// </summary>
        public virtual void Dispose()
        {
            if (this.isConnectionOwned && this.connections != null)
            {
                if (connections.State == ConnectionState.Open)
                    connections.Close();
                connections.Dispose();
                this.connections = null;
            }
        }

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
                          where meth.GetCustomAttributes(typeof(MappedMethodAttribute), true).Length > 0
                          select meth).FirstOrDefault();

            // We absolutely need to find a method with this attribute, because we won't know where in
            // the stack trace to stop, so that we can inspect the method for parameter name info.
            if (method == null)
                throw new Exception("Could not find a mapped method!");

            var meta = GetCommandDescription(method);

            // since the parameters array was passed to this method by the mapped method, the mapped
            // method has the responsibility to pass the parameters in the same order they are
            // specified in the method signature
            var methParams = method.GetParameters();

            if (methParams.Length != parameterValues.Length)
                throw new Exception("The mapped method's parameter list is a different length than what was passed to the processing method.");

            // To support calling stored procedures from non-default schemas:
            var procName = meta.Name ?? method.Name;
            if (meta.Schema != null)
                procName = meta.Schema + "." + procName;

            var command = ConstructCommand(
                meta.CommandType == CommandType.Text ? meta.Query : procName,
                meta.CommandType,
                methParams,
                parameterValues);

            if (meta.Timeout > -1)
                command.CommandTimeout = meta.Timeout;
            return command;
        }

        public MappedMethodAttribute GetCommandDescription(MethodBase method)
        {
            var meta = (MappedMethodAttribute)method.GetCustomAttributes(typeof(MappedMethodAttribute), true).FirstOrDefault();
            if (meta != null && meta.CommandType == CommandType.TableDirect)
                throw new NotImplementedException("Table-Direct queries are not supported by SqlSiphon");
            return meta;
        }

        protected virtual CommandT ConstructCommand(string procName, CommandType commandType, ParameterInfo[] methParams, object[] parameterValues)
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

        public ParameterT[] MakeProcedureParameters(ParameterInfo[] methParams, object[] parametersValues)
        {
            List<ParameterT> procedureParams = new List<ParameterT>();
            for (int i = 0; methParams != null && i < methParams.Length; ++i)
            {
                var p = new ParameterT();
                p.ParameterName = methParams[i].Name;
                if (parametersValues != null)
                    p.Value = parametersValues[i] ?? DBNull.Value;
                if (methParams[i].IsOut)
                    p.Direction = ParameterDirection.Output;
                else if (methParams[i].ParameterType.IsByRef)
                    p.Direction = ParameterDirection.InputOutput;
                else
                    p.Direction = ParameterDirection.Input;
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

        public EntityT GetQuery<EntityT>(string query)
        {
            return this.GetListQuery<EntityT>(query).FirstOrDefault();
        }

        protected List<EntityT> GetListQuery<EntityT>(string query)
        {
            using (var command = ConstructCommand(query, CommandType.Text, null, null))
            {
                using (var reader = GetReader(command))
                {
                    var type = typeof(EntityT);
                    var countExpected = command.CommandType == CommandType.Text;
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
            command.ExecuteNonQuery();
            CopyOutputParameters(parameters, command.Parameters);
        }

        public void ExecuteQuery(string query)
        {
            if (query != null)
            {
                if (query.Trim().Length > 0)
                {
                    using (var command = ConstructCommand(query, CommandType.Text, null, null))
                    {
                        if (command.Connection.State == ConnectionState.Closed)
                            command.Connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
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
            using (var command = ConstructCommand(query, CommandType.Text, null, null))
                return GetDataSet(command);
        }

        private void Open()
        {
            if (Connection.State == ConnectionState.Closed)
                Connection.Open();
        }

        private DataReaderT GetReader(CommandT command, params object[] parameters)
        {
            Open();
            var reader = command.ExecuteReader();
            CopyOutputParameters(parameters, command.Parameters);
            return (DataReaderT)reader;
        }

        protected DataReaderT GetReader(params object[] parameters)
        {
            using (var command = ConstructCommand(parameters))
                return GetReader(command, parameters);
        }

        public DataReaderT GetReaderQuery(string query)
        {
            using (var command = ConstructCommand(query, CommandType.Text, null, null))
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

        private static BindingFlags PATTERN = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        internal static UnifiedSetter[] GetSettableMembers(Type type)
        {
            return (from f in type.GetFields(PATTERN) select new UnifiedSetter(f))
                .Union(from p in type.GetProperties(PATTERN) select new UnifiedSetter(p))
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
        public IEnumerable<EntityT> GetEnumerator<EntityT>(params object[] parameters)
        {
            var type = typeof(EntityT);

            // The most basic case is returning values from one column in the table.
            // The first parameter is skipped because it's the column name to retrieve,
            // not a parameter to the stored procedure
            bool isPrimitive = type.IsPrimitive
                || type == typeof(decimal)
                || type == typeof(string)
                || type == typeof(DateTime)
                || type == typeof(DateTime?)
                || type == typeof(Guid)
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
    }
}