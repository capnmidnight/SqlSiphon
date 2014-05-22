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
    public interface ISqlSiphon : IDisposable
    {
        void CreateTables();
        void CreateForeignKeys();
        void RunAllManualScripts();
        void DropProcedures();
        void SynchronizeUserDefinedTableTypes();
        void CreateProcedures();
        void CreateIndices();
        void InitializeData();
        void Analyze();
        void AlterDatabase(string script);

        event DataProgressEventHandler Progress;

        string MOTD { get; }

        KeyValuePair<string, string>[] CreateScripts { get; }
        KeyValuePair<string, string>[] DropScripts { get; }
        KeyValuePair<string, string>[] AlterScripts { get; }
        KeyValuePair<string, string>[] OtherScripts { get; }
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
        private bool isConnectionOwned;

        private MappedClassAttribute meta;
        private Regex FKNameRegex;

        protected virtual string IdentifierPartBegin { get { return ""; } }
        protected virtual string IdentifierPartEnd { get { return ""; } }
        protected virtual string IdentifierPartSeperator { get { return "."; } }
        protected virtual string DefaultSchemaName { get { return null; } }

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
                string.Format("add constraint \\{0}([\\w_]+)\\{1}", IdentifierPartBegin, IdentifierPartEnd),
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            this.Connection = connection;
            this.isConnectionOwned = isConnectionOwned;
            var type = this.GetType();
            this.meta = MappedObjectAttribute.GetAttribute<MappedClassAttribute>(type) ?? new MappedClassAttribute();
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

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(CommandType = CommandType.Text,
            Query = "select * from ScriptStatus")]
        public List<ScriptStatus> GetScriptStatus()
        {
            return this.GetList<ScriptStatus>();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(CommandType = CommandType.Text,
            Query = "select count(*) from ScriptStatus")]
        public int GetDatabaseVersion()
        {
            return this.Get<int>(0);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(CommandType = CommandType.Text,
            Query = "insert into ScriptStatus(Script) values(@script);")]
        public void AlterDatabase(string script)
        {
            this.ExecuteQuery(script);
            this.Execute(script);
        }

        /// <summary>
        /// Creates a stored procedure command call from a variable array of parameters. The stored
        /// procedure to call is determined by walking the Stack Trace to find a method with the
        /// MappedMethodAttribute attribute applied to it. This method, once found, must have the same
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
            {
                if (isPrimitive && meta.Parameters.Count == parameterValues.Length + 1)
                    throw new Exception("When querying for a single value, the first parameter passed to the processing method will be treated as a column name or column index to map out of the query result set.");
                else if (meta.Parameters.Count < parameterValues.Length)
                    throw new Exception("More parameters were passed to the processing method than were specified in the mapped method signature.");
                else
                    throw new Exception("Fewer parameters were passed to the processing method than were specified in the mapped method signature.");
            }

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
                var columnNames = GetColumnNames(reader).Select(c => c.ToUpper()).ToArray();
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

        internal static string[] GetColumnNames(DbDataReader reader)
        {
            var columnNames = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; ++i)
                columnNames[i] = reader.GetName(i);
            return columnNames;
        }

        protected static List<MappedPropertyAttribute> GetProperties(Type type)
        {
            var attr = MappedObjectAttribute.GetAttribute<MappedClassAttribute>(type)
                ?? new MappedClassAttribute();
            attr.InferProperties(type);
            return attr.Properties;
        }

        /// <summary>
        /// copies the values of every field in the row to the field in the object that has the same name. 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="reader"></param>
        /// <param name="columnNames"></param>
        /// <param name="props"></param>
        internal static void DoMapping(object obj, DataReaderT reader, string[] columnNames, List<MappedPropertyAttribute> props)
        {
            props.Where(p => columnNames.Contains(p.Name.ToUpper()))
                .ToList()
                .ForEach(p => p.SetValue(obj, reader[p.Name.ToUpper()]));
        }

        static protected bool IsTypePrimitive(Type type)
        {
            return type.IsPrimitive
                || type == typeof(float)
                || type == typeof(float?)
                || type == typeof(decimal)
                || type == typeof(decimal?)
                || type == typeof(string)
                || type == typeof(DateTime)
                || type == typeof(DateTime?)
                || type == typeof(Guid)
                || type == typeof(Guid?)
                || type == typeof(byte[]);
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
                meta.InferProperties(method);

                if (meta.Schema == null)
                    meta.Schema = DefaultSchemaName;

                if (meta.SqlType == null)
                    meta.SqlType = this.MakeSqlTypeString(meta);

                foreach (var parameter in meta.Parameters)
                    if (parameter.SqlType == null)
                        parameter.SqlType = this.MakeSqlTypeString(parameter);
                this.ModifyQuery(meta);
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

        /// <summary>
        /// Scans the Data Access Layer for public methods that have stored procedure
        /// definitions, and creates/alters procedures as necessary to bring them all
        /// up to date.
        /// </summary>
        public void DropProcedures()
        {
            this.Process(meta.Methods, m => string.Format("drop procedure {0}", m.Name), this.DropProcedure);
        }

        public void CreateProcedures()
        {
            this.Process(meta.Methods, m => string.Format("create procedure {0}", m.Name), this.CreateProcedure);
        }

        /// <summary>
        /// If any mapped classes do not have matching tables in the
        /// database, this method creates them after a connection is
        /// made to the server.
        /// 
        /// This will only create tables for classes that are in the
        /// same namespace as this data access layer.
        /// </summary>
        public void CreateTables()
        {
            this.Analyze();
            // tables that don't already exist can be created without fear.
            this.Process(this.newTables, c => string.Format("create table {0}", c.Name), this.CreateTable);
            this.Process(this.newNullableColumns, c => string.Format("add column {0} to {1}", c.Value.Name, c.Key), this.AddColumn);
        }

        public void Analyze()
        {
            var columns = this.GetListQuery<ColumnInfo>("SELECT * FROM INFORMATION_SCHEMA.COLUMNS");
            var existingSchema = columns
                .GroupBy(column => MakeIdentifier(column.table_schema ?? DefaultSchemaName, column.table_name))
                .ToDictionary(g => g.Key, g => g.ToDictionary(v => v.column_name.ToLower()));

            var t = this.GetType();
            var allMappedTypes = t.Assembly.GetTypes()
                .Where(x => x.Namespace == t.Namespace)
                .Union(new Type[] { typeof(ScriptStatus) })
                .Select(x =>
                {
                    var y = MappedObjectAttribute.GetAttribute<MappedClassAttribute>(x);
                    if (y != null)
                    {
                        y.InferProperties(x);
                        if (y.Schema == null)
                            y.Schema = DefaultSchemaName;
                        y.Properties.ForEach(p => p.SqlType = p.SqlType ?? MakeSqlTypeString(p));
                    }
                    return y;
                })
                .Where(m => m != null && m.Include)
                .ToDictionary(table => MakeIdentifier(table.Schema, table.Name));

            this.newTables = allMappedTypes
                .Where(table => !existingSchema.ContainsKey(table.Key)).Select(table => table.Value)
                .ToList();
            var changedTables = allMappedTypes
                .Where(table => existingSchema.ContainsKey(table.Key))
                .ToDictionary(table => table.Key);
            this.tablesToDrop = existingSchema
                .Where(table => !allMappedTypes.ContainsKey(table.Key)
                    && !table.Key.Contains("aspnet"))
                .Select(table => table.Key).ToList();

            this.newNullableColumns = new List<KeyValuePair<string, MappedPropertyAttribute>>();
            this.newNonNullableColumns = new List<KeyValuePair<string, MappedPropertyAttribute>>();
            this.columnsToAlter = new List<KeyValuePair<ColumnInfo, MappedPropertyAttribute>>();
            this.defaultsToAdd = new List<KeyValuePair<ColumnInfo, MappedPropertyAttribute>>();
            this.columnsToDrop = new List<ColumnInfo>();
            this.PopulateEnumTableScripts = allMappedTypes.SelectMany(kv =>
                kv.Value.EnumValues.Select(v =>
                    new KeyValuePair<string, string>(kv.Key + "_" + v.Value,
                        string.Format("if not exists(select * from {0} where Value = {1}) insert into {0}(Value, Description) values({1}, '{2}')",
                            kv.Key,
                            v.Key,
                            v.Value.Replace("'", "''"))))).ToArray();

            // for tables that already exist, figure out if they have any overlapping columns
            foreach (var tableName in changedTables.Keys)
            {
                var type = allMappedTypes[tableName];
                var table = existingSchema[tableName];
                var props = type.Properties.Where(p => p.Include).ToDictionary(p => p.Name.ToLower());
                var cols = props
                    .Where(p => !table.ContainsKey(p.Key.ToLower()))
                    .Select(p => p.Value);
                newNullableColumns.AddRange(cols.Where(c => c.IsOptional).Select(c => new KeyValuePair<string, MappedPropertyAttribute>(tableName, c)));
                this.newNonNullableColumns.AddRange(cols.Where(c => !c.IsOptional).Select(c => new KeyValuePair<string, MappedPropertyAttribute>(tableName, c)));
                this.columnsToAlter.AddRange(props
                    .Where(p => table.ContainsKey(p.Key.ToLower())
                        && IsTypeChanged(table[p.Key.ToLower()], p.Value))
                    .Select(p => new KeyValuePair<ColumnInfo, MappedPropertyAttribute>(table[p.Key.ToLower()], p.Value)));
                this.defaultsToAdd.AddRange(props
                    .Where(p => table.ContainsKey(p.Key.ToLower())
                        && IsDefaultChanged(table[p.Key.ToLower()], p.Value))
                    .Select(p => new KeyValuePair<ColumnInfo, MappedPropertyAttribute>(table[p.Key.ToLower()], p.Value)));
                this.columnsToDrop.AddRange(table.Values
                    .Where(c => !props.ContainsKey(c.column_name.ToLower())));
            }
        }



        private List<MappedClassAttribute> newTables;
        private List<string> tablesToDrop;

        private List<KeyValuePair<string, MappedPropertyAttribute>> newNullableColumns;
        private List<KeyValuePair<string, MappedPropertyAttribute>> newNonNullableColumns;
        private List<ColumnInfo> columnsToDrop;
        private List<KeyValuePair<ColumnInfo, MappedPropertyAttribute>> columnsToAlter;
        private List<KeyValuePair<ColumnInfo, MappedPropertyAttribute>> defaultsToAdd;

        public KeyValuePair<string, string>[] AddNullableColumnScripts
        {
            get
            {
                return newNullableColumns.Select(x =>
                    new KeyValuePair<string, string>(x.Key + this.IdentifierPartSeperator + MakeIdentifier(x.Value.Name),
                    this.MakeAddColumnScript(x.Key, x.Value))).ToArray();
            }
        }

        public KeyValuePair<string, string>[] AddNonNullableColumnScripts
        {
            get
            {
                return newNonNullableColumns.Select(x =>
                    new KeyValuePair<string, string>(x.Key + this.IdentifierPartSeperator + MakeIdentifier(x.Value.Name),
                    this.MakeAddColumnScript(x.Key, x.Value))).ToArray();
            }
        }

        public KeyValuePair<string, string>[] DropColumnScripts
        {
            get
            {
                return columnsToDrop.Select(x =>
                    new KeyValuePair<string, string>(MakeIdentifier(x.table_schema ?? DefaultSchemaName, x.table_name, x.column_name),
                    this.MakeDropColumnScript(x))).ToArray();
            }
        }

        public KeyValuePair<string, string>[] DropTableScripts
        {
            get
            {
                return tablesToDrop.Select(t => new KeyValuePair<string, string>(t, "drop table " + t)).ToArray();
            }
        }

        public KeyValuePair<string, string>[] DropForeignKeyScripts
        {
            get
            {
                return FKScripts.Select(s =>
                {
                    var m = FKNameRegex.Match(s);
                    var script = s.Substring(0, m.Captures[0].Index + m.Captures[0].Length)
                        .Replace("add constraint", "drop constraint")
                        .Replace("not exists", "exists");
                    return new KeyValuePair<string, string>(m.Groups[1].Value, script);
                }).ToArray();
            }
        }

        public KeyValuePair<string, string>[] AddForeignKeyScripts
        {
            get
            {
                return FKScripts.Select(s =>
                {
                    var m = FKNameRegex.Match(s);
                    return new KeyValuePair<string, string>(m.Groups[1].Value, s);
                }).ToArray();
            }
        }

        public KeyValuePair<string, string>[] AlterColumnScripts
        {
            get
            {
                return columnsToAlter.Select(x =>
                    new KeyValuePair<string, string>(MakeIdentifier(x.Key.table_schema ?? DefaultSchemaName, x.Key.table_name, x.Key.column_name),
                    this.MakeAlterColumnScript(x.Key, x.Value)))
                    .Union(this.defaultsToAdd.Select(x =>
                    new KeyValuePair<string, string>(MakeIdentifier(x.Key.table_schema ?? DefaultSchemaName, x.Key.table_name, x.Key.column_name),
                    this.MakeDefaultConstraintScript(x.Key, x.Value)))).ToArray();
            }
        }


        public KeyValuePair<string, string>[] AddTableScripts
        {
            get
            {
                return newTables.Select(t => new KeyValuePair<string, string>(MakeIdentifier(t.Schema ?? DefaultSchemaName, t.Name),
                    this.MakeCreateTableScript(t))).ToArray();
            }
        }

        public KeyValuePair<string, string>[] CreateScripts
        {
            get
            {
                return AddTableScripts
                    .Union(AddNullableColumnScripts).ToArray();
            }
        }

        public KeyValuePair<string, string>[] AlterScripts
        {
            get
            {
                return AddNonNullableColumnScripts.Union(AlterColumnScripts).ToArray();
            }
        }

        public KeyValuePair<string, string>[] DropScripts
        {
            get
            {
                return DropTableScripts
                    .Union(DropColumnScripts).ToArray();
            }
        }

        public KeyValuePair<string, string>[] PopulateEnumTableScripts { get; private set; }

        public KeyValuePair<string, string>[] OtherScripts
        {
            get
            {
                var toRun = this.PopulateEnumTableScripts
                    .Select(kv=>kv.Value)
                    .Union(this.InitialScripts)
                    .ToList();
                var alreadyRan = this.GetScriptStatus().Select(g=>g.Script);
                foreach (var script in alreadyRan)
                    if (toRun.Contains(script))
                        toRun.Remove(script);
                return toRun.Select((s, i) => new KeyValuePair<string, string>(
                    string.Format("manual script {0}", i), s))
                    .ToArray();
            }
        }

        private static Regex UnwrappingPattern = new Regex("^\\((.+)\\)$", RegexOptions.Compiled);
        private bool IsDefaultChanged(ColumnInfo column, MappedPropertyAttribute property)
        {
            var unwrapped = column.column_default;
            while (unwrapped != null
                && UnwrappingPattern.IsMatch(unwrapped))
                unwrapped = UnwrappingPattern.Match(unwrapped).Groups[1].Value;
            var changed = unwrapped != property.DefaultValue;
            if (changed)
                changed = true;
            return changed;
        }

        private bool IsTypeChanged(ColumnInfo column, MappedPropertyAttribute property)
        {
            var temp = new MappedPropertyAttribute(column);
            var origType = this.MakeSqlTypeString(temp);
            var newType = this.MakeSqlTypeString(property);

            var changed = temp.IsOptional != property.IsOptional
                || origType != newType
                || temp.IsSizeSet != property.IsSizeSet
                || temp.Size != property.Size
                || temp.IsPrecisionSet != property.IsPrecisionSet
                || temp.Precision != property.Precision;

            return changed;
        }

        private enum Change
        {
            Old,
            New,
            Change
        }

        private void ExecuteScripts(string message, List<string> scripts)
        {
            if (scripts != null)
            {
                this.Process(scripts, s => message, this.ExecuteQuery);
            }
        }

        public void CreateForeignKeys()
        {
            this.ExecuteScripts("Foreign keys", this.FKScripts.ToList());
        }

        public void RunAllManualScripts()
        {
            this.ExecuteScripts("Manual scripts", this.OtherScripts.Select(kv => kv.Value).ToList());
        }

        public void CreateIndices()
        {
            this.ExecuteScripts("Indices", this.IndexScripts.ToList());
        }

        public void InitializeData()
        {
            this.ExecuteScripts("Initial data", this.InitialScripts.ToList());
        }

        private void DropProcedure(MappedMethodAttribute info)
        {
            if (info.CommandType == CommandType.StoredProcedure
                && !string.IsNullOrEmpty(info.Query))
            {
                info.Schema = info.Schema ?? DefaultSchemaName;
                var identifier = this.MakeIdentifier(info.Schema, info.Name);
                if (this.ProcedureExists(info))
                {
                    var script = MakeDropProcedureScript(info);
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

        private void CreateTable(MappedClassAttribute type)
        {
            var schema = type.Schema ?? DefaultSchemaName;
            var identifier = this.MakeIdentifier(schema, type.Name);
            var script = MakeCreateTableScript(type);
            try
            {
                this.ExecuteQuery(script);
            }
            catch (Exception exp)
            {
                throw new Exception(string.Format("Could not create table: {0}. Reason: {1}", identifier, exp.Message), exp);
            }
        }

        private void AddColumn(KeyValuePair<string, MappedPropertyAttribute> column)
        {
            var tableID = column.Key;
            var script = this.MakeAddColumnScript(tableID, column.Value);
            try
            {
                this.ExecuteQuery(script);
            }
            catch (Exception exp)
            {
                throw new Exception(string.Format("Could not add column: {0} to table: {1}. Reason: {2}", column.Value.Name, tableID, exp.Message), exp);
            }
        }

        private void CreateProcedure(MappedMethodAttribute info)
        {
            if (info.CommandType == CommandType.StoredProcedure
                && !string.IsNullOrEmpty(info.Query))
            {
                var schema = info.Schema ?? DefaultSchemaName;
                var identifier = this.MakeIdentifier(schema, info.Name);
                var script = MakeCreateProcedureScript(info);
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

        protected string FK<T, F>()
        {
            var f = typeof(F);
            var foreign = MappedClassAttribute.GetAttribute<MappedClassAttribute>(f);
            foreign.InferProperties(f);
            var foreignColumns = this.ArgumentList(
                foreign.Properties.Where(prop => prop.IncludeInPrimaryKey),
                prop => prop.Name);

            return FK<T>(foreignColumns, foreign.Schema, foreign.Name, foreignColumns);
        }

        protected string FK<T, F>(string tableColumns)
        {
            var f = typeof(F);
            var foreign = MappedClassAttribute.GetAttribute<MappedClassAttribute>(f);
            foreign.InferProperties(f);
            var foreignColumns = this.ArgumentList(
                foreign.Properties.Where(prop => prop.IncludeInPrimaryKey),
                prop => prop.Name);

            return FK<T>(tableColumns, foreign.Schema, foreign.Name, foreignColumns);
        }

        protected string FK<T, F>(string tableColumns, string foreignColumns)
        {
            var f = typeof(F);
            var foreign = MappedClassAttribute.GetAttribute<MappedClassAttribute>(f);
            foreign.InferProperties(f);

            return FK<T>(tableColumns, foreign.Schema, foreign.Name, foreignColumns);
        }

        protected string FK<T>(string foreignName, string foreignColumns)
        {
            return FK<T>(foreignColumns, null, foreignName, foreignColumns);
        }

        protected string FK<T>(string foreignSchema, string foreignName, string foreignColumns)
        {
            return FK<T>(foreignColumns, foreignSchema, foreignName, foreignColumns);
        }

        protected string FK<T>(string tableColumns, string foreignSchema, string foreignName, string foreignColumns)
        {
            var t = typeof(T);
            var table = MappedClassAttribute.GetAttribute<MappedClassAttribute>(t);
            table.InferProperties(t);

            return MakeFKScript(table.Schema, table.Name, tableColumns, foreignSchema, foreignName, foreignColumns);
        }

        private string BuildIndex<T>(Func<MappedClassAttribute, string[]> getColumns, Func<MappedClassAttribute, string[], string> getIndexName)
        {
            var t = typeof(T);
            var table = MappedClassAttribute.GetAttribute<MappedClassAttribute>(t);
            table.InferProperties(t);
            var columns = getColumns(table);
            var indexName = getIndexName(table, columns);
            return this.MakeIndexScript(indexName, table.Schema ?? DefaultSchemaName, table.Name, columns);
        }

        protected string NamedIndex<T>(string indexName)
        {
            return BuildIndex<T>(
                table => table.Properties
                    .Where(c => c.Include
                        && (c.SystemType != typeof(string)
                            || c.Size > 0))
                    .Select(c => c.Name)
                    .ToArray(),
                (table, columns) => indexName);
        }

        protected string NamedIndex<T>(string indexName, params string[] columns)
        {
            return BuildIndex<T>(
                table => columns,
                (table, cols) => indexName);
        }

        protected string Index<T>()
        {
            return BuildIndex<T>(
                table => table.Properties
                    .Where(c => c.Include
                        && (c.SystemType != typeof(string)
                            || c.Size > 0))
                    .Select(c => c.Name)
                    .ToArray(),
                (table, columns) => string.Format("IDX_{0}_{1}_{2}",
                    table.Schema ?? DefaultSchemaName,
                    table.Name,
                    string.Join(",", columns).GetHashCode().ToString().Replace('-', 'S')));
        }

        protected string Index<T>(params string[] columns)
        {
            return BuildIndex<T>(
                table => columns,
                (table, cols) => string.Format("IDX_{0}_{1}_{2}",
                    table.Schema ?? DefaultSchemaName,
                    table.Name,
                    string.Join(",", columns).GetHashCode().ToString().Replace('-', 'S')));
        }

        private string ArgumentList<T>(IEnumerable<T> collect, Func<T, string> format, string separator = null)
        {
            separator = separator ?? "," + Environment.NewLine + "    ";
            var str = string.Join(separator, collect.Select(format));
            return str;
        }

        protected string MakeParameterSection(MappedMethodAttribute info)
        {
            return ArgumentList(info.Parameters, this.MakeParameterString);
        }

        protected string MakeColumnSection(MappedClassAttribute info)
        {
            return ArgumentList(info.Properties.Where(p => p.Include), this.MakeColumnString);
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

        protected virtual void ExecuteCreateProcedureScript(string script)
        {
            //this allows us to hook in and perhaps modify
            // the script before executing it.
            this.ExecuteQuery(script);
        }

        protected virtual void PreCreateProcedures()
        {
            //do nothing in the base case
        }

        protected string MakeSqlTypeString(MappedTypeAttribute p)
        {
            var systemType = p.SystemType;
            if (systemType != null && systemType.IsEnum)
                systemType = typeof(int);
            return MakeSqlTypeString(p.SqlType, systemType, p.IsCollection, p.IsSizeSet, p.Size, p.IsPrecisionSet, p.Precision);
        }

        protected virtual string[] FKScripts { get { return null; } }
        protected virtual string[] IndexScripts { get { return null; } }
        protected virtual string[] InitialScripts { get { return null; } }

        public virtual string MOTD { get { return ""; } }

        protected virtual bool ProcedureExists(MappedMethodAttribute info)
        {
            throw new NotImplementedException();
        }
        protected virtual string MakeDropProcedureScript(MappedMethodAttribute info)
        {
            throw new NotImplementedException();
        }
        protected virtual string MakeCreateProcedureScript(MappedMethodAttribute info)
        {
            throw new NotImplementedException();
        }
        protected virtual string MakeCreateTableScript(MappedClassAttribute info)
        {
            throw new NotImplementedException();
        }
        protected virtual string MakeColumnString(MappedPropertyAttribute p)
        {
            throw new NotImplementedException();
        }
        protected virtual string MakeDropColumnScript(ColumnInfo c)
        {
            throw new NotImplementedException();
        }
        protected virtual string MakeAddColumnScript(string tableID, MappedPropertyAttribute prop)
        {
            throw new NotImplementedException();
        }
        protected virtual string MakeAlterColumnScript(ColumnInfo columnInfo, MappedPropertyAttribute prop)
        {
            throw new NotImplementedException();
        }
        protected virtual string MakeDefaultConstraintScript(ColumnInfo columnInfo, MappedPropertyAttribute prop)
        {
            throw new NotImplementedException();
        }
        protected virtual string MakeFKScript(string tableSchema, string tableName, string tableColumns, string foreignSchema, string foreignName, string foreignColumns)
        {
            throw new NotImplementedException();
        }
        protected virtual string MakeIndexScript(string indexName, string tableSchema, string tableName, string[] tableColumns)
        {
            throw new NotImplementedException();
        }
        protected virtual string MakeParameterString(MappedParameterAttribute p)
        {
            throw new NotImplementedException();
        }
        public virtual void SynchronizeUserDefinedTableTypes()
        {
            throw new NotImplementedException();
        }

        protected abstract string MakeSqlTypeString(string sqlType, Type systemType, bool isCollection, bool isSizeSet, int size, bool isPrecisionSet, int precision);
    }
}