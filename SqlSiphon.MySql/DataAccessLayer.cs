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

using System.Data;
using System.Runtime.CompilerServices;
using MySql.Data.MySqlClient;
using SqlSiphon.Mapping;

namespace SqlSiphon.MySql
{
    /// <summary>
    /// A base class for building Data Access Layers that connect to MySQL
    /// databases and execute store procedures stored within.
    /// </summary>
    public abstract class DataAccessLayer : DataAccessLayer<MySqlConnection, MySqlCommand, MySqlParameter, MySqlDataAdapter, MySqlDataReader>
    {
        /// <summary>
        /// creates a new connection to a MySQL database and automatically
        /// opens the connection. 
        /// </summary>
        /// <param name="connectionString">a standard MS SQL Server connection string</param>
        public DataAccessLayer(string connectionString)
            : base(connectionString)
        {
        }

        public DataAccessLayer(MySqlConnection connection)
            : base(connection)
        {
        }

        protected override string IdentifierPartBegin { get { return "`"; } }
        protected override string IdentifierPartEnd { get { return "`"; } }

		protected override string DropProcedureScript (string identifier)
		{
			return string.Format("drop procedure {0}", identifier);
		}

		protected override string CreateProcedureScript (string identifier, string parameterSection, string body)
		{
			return string.Format(
@"create procedure {0}
    ({1})
begin
    {2}
end//",
                identifier,
                parameterSection,
                body);
		}

		protected override void ExecuteCreateProcedure (string script)
		{
			var withDelim = new MySqlScript(this.Connection, script);
			withDelim.Delimiter = "//";
			withDelim.Execute();
		}

		
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(CommandType=CommandType.Text,
            Query = 
@"select routine_name 
from information_schema.routines 
where routine_schema = @schemaName 
    and routine_name = @routineName")]
		protected override bool ProcedureExists (SqlSiphon.Mapping.MappedMethodAttribute method)
		{
			throw new System.NotImplementedException ();
		}

		protected override string MakeParameterString (SqlSiphon.Mapping.MappedParameterAttribute p)
		{
			throw new System.NotImplementedException ();
		}
    }
}