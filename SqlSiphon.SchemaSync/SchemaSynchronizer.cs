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

namespace SqlSiphon.SchemaSync
{
    public abstract class SchemaSynchronizer
        <DataAccessLayerT, ConnectionT, CommandT, ParameterT, DataAdapterT, DataReaderT>
        where DataAccessLayerT : DataAccessLayer<ConnectionT, CommandT, ParameterT, DataAdapterT, DataReaderT>
        where ConnectionT : DbConnection, new()
        where CommandT : DbCommand, new()
        where ParameterT : DbParameter, new()
        where DataAdapterT : DbDataAdapter, new()
        where DataReaderT : DbDataReader
    {
        private DataAccessLayerT database;

        public SchemaSynchronizer(DataAccessLayerT dal)
        {
            this.database = dal;
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
                if (this.ProcedureExists(schema, info.Name))
                {
                    var script = BuildDropProcedureScript(identifier);
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

        

        protected virtual void ExcecuteCreateProcedureScript(string script)
        {
            this.ExecuteQuery(script);
        }

        protected abstract bool ProcedureExists(string schemaName, string routineName);
        protected abstract string BuildDropProcedureScript(string identifier);
        protected abstract string BuildCreateProcedureScript(MappedMethodAttribute info);
        protected abstract string MakeParameterString(MappedParameterAttribute p);
    }
}
