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

using System.Data.OleDb;
using System.IO;

namespace SqlSiphon
{
    public abstract class OleDbDataAccessLayer : DataAccessLayer<OleDbConnection, OleDbCommand, OleDbParameter, OleDbDataAdapter, OleDbDataReader>
    {
        private static string MakeConnectionString(FileSystemInfo container, string options, string provider = "Microsoft.Jet.OLEDB.4.0")
        {
            if (container.Exists)
            {
                return string.Format(@"Provider={2};Data Source=""{0}"";{1};", container.FullName, options, provider);
            }
            return null;
        }

        public static string MakeExcel97ConnectionString(string filename)
        {
            return MakeConnectionString(new FileInfo(filename), @"Extended Properties=""Excel 8.0;HDR=Yes"""); // add IMEX=1 to extended properties if columns have mixed data
        }

        public static string MakeExcel2007ConnectionString(string filename)
        {
            return MakeConnectionString(new FileInfo(filename), @"Extended Properties=""Excel 12.0;HDR=Yes""", "Microsoft.ACE.OLEDB.12.0");
        }

        public static string MakeAccess97ConnectionString(string filename)
        {
            return MakeConnectionString(new FileInfo(filename), "Persist Security Info=True");
        }

        public static string MakeCsvConnectionString(string directoryName)
        {
            return MakeConnectionString(new DirectoryInfo(directoryName), @"Extended Properties=""Text""");
        }
        /// <summary>
        /// creates a new connection to a OleDb database and automatically
        /// opens the connection. 
        /// </summary>
        /// <param name="connectionString">a standard MS SQL Server connection string</param>
        public OleDbDataAccessLayer(string connectionString)
            : base(connectionString)
        {
        }

        public OleDbDataAccessLayer(OleDbConnection connection)
            : base(connection)
        {
        }
    }
}