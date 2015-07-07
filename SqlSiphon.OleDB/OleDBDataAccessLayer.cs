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
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Data.OleDb;
using SqlSiphon;
using SqlSiphon.Mapping;
using SqlSiphon.Model;

namespace SqlSiphon.OleDB
{
    public class OleDBDataAccessLayer : SqlSiphon<OleDbConnection, OleDbCommand, OleDbParameter, OleDbDataAdapter, OleDbDataReader>
    {
        public override string DataSource { get { return this.Connection.DataSource; } }

        private static Dictionary<string, Type> typeMapping;
        private static Dictionary<Type, string> reverseTypeMapping;

        static OleDBDataAccessLayer()
        {
            typeMapping = new Dictionary<string, Type>();
            typeMapping.Add("varbinary", typeof(byte[]));
            typeMapping.Add("longbinary", typeof(byte[]));
            typeMapping.Add("binary", typeof(byte[]));
            typeMapping.Add("bit", typeof(bool));
            typeMapping.Add("currency", typeof(decimal));
            typeMapping.Add("numeric", typeof(decimal));
            typeMapping.Add("datetime", typeof(DateTime));
            typeMapping.Add("date", typeof(DateTime));
            typeMapping.Add("guid", typeof(Guid));
            typeMapping.Add("text", typeof(string));
            typeMapping.Add("varchar", typeof(string));
            typeMapping.Add("longtext", typeof(string));
            typeMapping.Add("single", typeof(float));
            typeMapping.Add("double", typeof(double));
            typeMapping.Add("unsigned byte", typeof(byte));
            typeMapping.Add("short", typeof(short));
            typeMapping.Add("long", typeof(int));
            typeMapping.Add("autoincrement", typeof(int));
            typeMapping.Add("counter", typeof(int));

            reverseTypeMapping = typeMapping
                .GroupBy(kv => kv.Value, kv => kv.Key)
                .ToDictionary(g => g.Key, g => g.First());

            reverseTypeMapping.Add(typeof(int?), "long");
            reverseTypeMapping.Add(typeof(uint), "long");
            reverseTypeMapping.Add(typeof(uint?), "long");

            reverseTypeMapping.Add(typeof(short?), "short");
            reverseTypeMapping.Add(typeof(ushort), "short");
            reverseTypeMapping.Add(typeof(ushort?), "short");

            reverseTypeMapping.Add(typeof(byte?), "byte");
            reverseTypeMapping.Add(typeof(sbyte), "byte");
            reverseTypeMapping.Add(typeof(sbyte?), "byte");

            reverseTypeMapping.Add(typeof(double?), "double");
            reverseTypeMapping.Add(typeof(float?), "single");
            reverseTypeMapping.Add(typeof(decimal?), "currency");

            reverseTypeMapping.Add(typeof(DateTime?), "datetime");

            reverseTypeMapping.Add(typeof(bool?), "bit");
        }

        protected override string IdentifierPartBegin
        {
            get
            {
                return "[";
            }
        }

        protected override string IdentifierPartEnd
        {
            get
            {
                return "]";
            }
        }

        protected override string IdentifierPartSeperator
        {
            get
            {
                return ".";
            }
        }

        private void FillInFile()
        {
            if (!System.IO.File.Exists(this.DataSource))
            {
                ADOX.Catalog cat = new ADOX.Catalog();
                cat.Create(string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0}; Jet OLEDB:Engine Type=5", this.DataSource));
            }
        }

        /// <summary>
        /// creates a new connection to an MS Access database and automatically
        /// opens the connection. 
        /// </summary>
        /// <param name="connectionString"></param>
        public OleDBDataAccessLayer(string fileName)
            : base(string.Format("Provider=Microsoft.Jet.OleDb.4.0;Data Source={0}", fileName))
        {
            FillInFile();
        }

        public OleDBDataAccessLayer(string server, string database, string user, string password)
            : base(server, database, user, password)
        {
            FillInFile();
        }

        public OleDBDataAccessLayer(OleDbConnection connection)
            : base(connection)
        {
            FillInFile();
        }

        public OleDBDataAccessLayer(OleDBDataAccessLayer database)
            : base(database)
        {
            FillInFile();
        }

        protected OleDBDataAccessLayer()
            : base()
        {
            FillInFile();
        }

        public override string MakeConnectionString(string server, string database, string user, string password)
        {
            FillInFile();
            var builder = new OleDbConnectionStringBuilder
            {
                Provider = "Microsoft.Jet.OleDb.4.0",
                FileName = server
            };
            if (!string.IsNullOrEmpty(user))
            {
                builder.Add("User Id", user);
                if (!string.IsNullOrEmpty(password))
                {
                    builder.Add("Password", password);
                }
            }
            return builder.ConnectionString;
        }

        private T[] GetSchemaRowValues<T>(Guid objectID, string columnName)
        {
            if (this.Connection.State != ConnectionState.Open)
            {
                this.Connection.Open();
            }
            return this.Connection.GetOleDbSchemaTable(objectID, null)
                .Rows
                .OfType<DataRow>()
                .Select(r => (T)r[columnName])
                .ToArray();
        }

        private string[] GetSchemaColumnNames(Guid objectID)
        {
            if (this.Connection.State != ConnectionState.Open)
            {
                this.Connection.Open();
            }
            return this.Connection.GetOleDbSchemaTable(objectID, null)
                .Columns
                .OfType<DataColumn>()
                .Select(c => string.Format("{0} {1}", c.DataType, c.ColumnName))
                .ToArray();
        }

        private string[] GetSchemaObjectNames()
        {
            if (this.Connection.State != ConnectionState.Open)
            {
                this.Connection.Open();
            }
            return this.Connection.GetSchema()
                .Rows
                .OfType<DataRow>()
                .Select(r => (string)r[0])
                .ToArray();
        }

        public override string MakeDropRoutineScript(RoutineAttribute info)
        {
            return string.Format("drop procedure {0}", info.Name);
        }

        public override string MakeCreateRoutineScript(RoutineAttribute info, bool createBody = true)
        {
            var identifier = this.MakeIdentifier(info.Schema ?? DefaultSchemaName, info.Name);
            var parameterSection = this.MakeParameterSection(info);
            if (parameterSection.Length > 0)
            {
                parameterSection = string.Format("({0})", parameterSection);
            }
            return string.Format(
@"CREATE PROCEDURE {0} {1} AS {2}",
                identifier,
                parameterSection,
                info.Query);
        }

        public override string MakeRoutineBody(RoutineAttribute info)
        {
            var query = info.Query;
            if (info.CommandType == CommandType.Text && info.Parameters.Count > 0)
            {
                var q = info.Query.Trim().ToUpperInvariant();
                if (!q.StartsWith("PARAMETERS"))
                {
                    var p = this.MakeParameterSection(info);
                    query = string.Format("PARAMETERS {0};\n{1}", p, info.Query);
                }
            }
            return query;
        }

        public override string MakeCreateTableScript(TableAttribute info)
        {
            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = this.MakeIdentifier(schema, info.Name);
            var columnSection = this.MakeColumnSection(info, false);
            var pk = info.Properties.Where(p => p.IncludeInPrimaryKey).ToArray();
            var pkString = "";
            if (pk.Length > 0)
            {
                pkString = string.Format(",{3}    constraint PK_{0}_{1} primary key({2}){3}",
                    schema,
                    info.Name,
                    string.Join(",", pk.Select(c => c.Name)),
                    Environment.NewLine);
            }
            return string.Format(
@"create table {0}(
    {1}{2}
)",
                identifier,
                columnSection,
                pkString);
        }

        protected override string MakeParameterString(ParameterAttribute p)
        {
            var typeStr = MakeSqlTypeString(p);
            return string.Format("{0} {1}", p.Name, typeStr);
        }

        protected override string MakeColumnString(ColumnAttribute p, bool isReturnType)
        {
            var typeStr = MakeSqlTypeString(p);
            if (p.IsIdentity)
            {
                typeStr = "autoincrement";
            }
            return string.Join(" ",
                p.Name,
                typeStr,
                p.IsOptional || p.IncludeInPrimaryKey ? "" : "NOT NULL")
                .Trim();
        }

        protected override string MakeSqlTypeString(string sqlType, Type systemType, int? size, int? precision, bool isIdentity)
        {
            if (string.IsNullOrEmpty(sqlType) && reverseTypeMapping.ContainsKey(systemType))
            {
                sqlType = reverseTypeMapping[systemType];
            }

            if (sqlType != null)
            {
                if (sqlType[sqlType.Length - 1] == ')') // someone already setup the type name, so skip it
                    return sqlType;
                else
                {
                    if (sqlType != "text" && size.HasValue)
                    {
                        sqlType += "(" + size.Value.ToString();
                        if (precision.HasValue)
                        {
                            sqlType += ", " + precision.Value.ToString();
                        }
                        sqlType += ")";
                    }

                    if (sqlType.Contains("var")
                        && sqlType[sqlType.Length - 1] != ')')
                    {
                        sqlType += "(MAX)";
                    }
                    return sqlType;
                }
            }
            else
            {
                return null;
            }
        }


        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [Routine(CommandType = CommandType.Text, Query = "SELECT * FROM Columns")]
        protected virtual DataSet GetColumns2()
        {
            return GetDataSet();
        }

        Type[] TYPE_CODE_CACHE = new Type[1000];
        public override List<InformationSchema.Columns> GetColumns()
        {
            if (this.Connection.State != ConnectionState.Open)
            {
                this.Connection.Open();
            }

            var columnSchema = this.Connection.GetSchema("Columns");
            var columnInfoType = typeof(InformationSchema.Columns);
            var columnInfoProperties = columnInfoType.GetProperties();
            var columns = columnSchema.Rows.OfType<DataRow>()
                .Select(r =>
                {
                    var col = new InformationSchema.Columns();
                    foreach (var prop in columnInfoProperties)
                    {
                        var value = r[prop.Name.ToUpperInvariant()];
                        if (value != DBNull.Value)
                        {
                            value = TranslateValue(value, value.GetType(), GetBaseType(prop.PropertyType));
                            prop.SetValue(col, value);
                        }
                    }
                    return col;
                }).ToList();

            return columns;
        }

        private static Type GetBaseType(Type t)
        {
            var targetType = t;
            if (targetType.IsGenericType
                && targetType.Name.StartsWith("Nullable"))
            {
                targetType = targetType.GetGenericArguments()[0];
            }
            return targetType;
        }

        private object TranslateValue(object value, Type sourceType, Type targetType)
        {
            if (sourceType != targetType)
            {
                if (sourceType == typeof(int))
                {
                    TYPE_CODE_CACHE[(int)value] = targetType;
                    if (targetType == typeof(string))
                    {
                        value = targetType.Name;
                    }
                    else if (targetType == typeof(byte))
                    {
                        value = (byte)(int)value;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (sourceType == typeof(long))
                {
                    if (targetType == typeof(int))
                    {
                        value = (int)(long)value;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (sourceType == typeof(bool))
                {
                    if (targetType == typeof(string))
                    {
                        value = value.ToString().ToLowerInvariant();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            return value;
        }
        
        public override void InsertAll(Type t, System.Collections.IEnumerable data)
        {
            // temporarily close the connection to the file
            bool wasOpen = false;
            if (this.Connection.State != ConnectionState.Closed)
            {
                wasOpen = true;
                this.Connection.Close();
            }

            // get the properties of the type we're inserting
            var attr = DatabaseObjectAttribute.GetAttribute(t);
            if (attr == null)
            {
                throw new Exception(string.Format("Type {0}.{1} could not be automatically inserted.", t.Namespace, t.Name));
            }
            var props = attr.Properties.Where(p => p.Include).ToArray();

            // setup DAO
            var engine = new DAO.DBEngine();
            var db = engine.OpenDatabase(this.Connection.DataSource);
            var recordSet = db.OpenRecordset(attr.Name);
            var fields = new DAO.Field[props.Length];
            var fieldNames = recordSet.Fields
                .OfType<DAO.Field>()
                .Select(f => f.Name);
            for (int i = 0; i < fields.Length; ++i)
            {
                fields[i] = recordSet.Fields[props[i].Name];
            }

            // copy
            foreach (var obj in data)
            {
                recordSet.AddNew();
                for (int j = 0; j < fields.Length; ++j)
                {
                    var value = props[j].GetValue(obj);
                    if (props[j].SystemType == typeof(Guid))
                    {
                        value = string.Format("{{{0}}}", value);
                    }
                    fields[j].Value = value;
                }
                recordSet.Update();
            }

            // clean up
            recordSet.Clone();
            db.Close();

            // reopen the connection, if the user is expecting it.
            if (wasOpen)
            {
                this.Connection.Open();
            }
        }

        public override List<InformationSchema.Routines> GetRoutines()
        {
            var procedureColumns = GetSchemaColumnNames(OleDbSchemaGuid.Procedures);
            return GetSchemaRowValues<string>(OleDbSchemaGuid.Procedures, "PROCEDURE_NAME")
                .Union(GetSchemaRowValues<string>(OleDbSchemaGuid.Views, "TABLE_NAME"))
                .Select(name => new InformationSchema.Routines
                {
                    routine_name = name
                })
                .ToList();
        }

        public override string MakeRoutineIdentifier(RoutineAttribute routine)
        {
            throw new NotImplementedException();
        }

        public override List<string> GetDatabaseLogins()
        {
            throw new NotImplementedException();
        }

        public override List<InformationSchema.IndexColumnUsage> GetIndexColumns()
        {
            throw new NotImplementedException();
        }

        public override List<InformationSchema.TableConstraints> GetTableConstraints()
        {
            throw new NotImplementedException();
        }

        public override List<InformationSchema.ReferentialConstraints> GetReferentialConstraints()
        {
            throw new NotImplementedException();
        }

        public override List<InformationSchema.Parameters> GetParameters()
        {
            throw new NotImplementedException();
        }

        public override List<InformationSchema.ConstraintColumnUsage> GetConstraintColumns()
        {
            throw new NotImplementedException();
        }

        public override List<InformationSchema.KeyColumnUsage> GetKeyColumns()
        {
            throw new NotImplementedException();
        }

        public override string DefaultSchemaName
        {
            get { throw new NotImplementedException(); }
        }

        public override int DefaultTypePrecision(string typeName, int testPrecision)
        {
            throw new NotImplementedException();
        }

        public override Type GetSystemType(string sqlType)
        {
            throw new NotImplementedException();
        }

        public override bool DescribesIdentity(InformationSchema.Columns column)
        {
            throw new NotImplementedException();
        }

        protected override string CheckDefaultValueDifference(ColumnAttribute final, ColumnAttribute initial)
        {
            throw new NotImplementedException();
        }

        public override string MakeCreateDatabaseLoginScript(string userName, string password, string database)
        {
            throw new NotImplementedException();
        }

        public override string MakeDropTableScript(TableAttribute table)
        {
            throw new NotImplementedException();
        }

        public override string MakeCreateColumnScript(ColumnAttribute column)
        {
            throw new NotImplementedException();
        }

        public override string MakeDropColumnScript(ColumnAttribute column)
        {
            throw new NotImplementedException();
        }

        public override string MakeAlterColumnScript(ColumnAttribute final, ColumnAttribute initial)
        {
            throw new NotImplementedException();
        }

        public override string MakeDropRelationshipScript(Relationship relation)
        {
            throw new NotImplementedException();
        }

        public override string MakeCreateRelationshipScript(Relationship relation)
        {
            throw new NotImplementedException();
        }

        public override string MakeDropPrimaryKeyScript(PrimaryKey key)
        {
            throw new NotImplementedException();
        }

        public override string MakeCreatePrimaryKeyScript(PrimaryKey key)
        {
            throw new NotImplementedException();
        }

        public override string MakeDropIndexScript(TableIndex index)
        {
            throw new NotImplementedException();
        }

        public override string MakeCreateIndexScript(TableIndex index)
        {
            throw new NotImplementedException();
        }

        public override bool RunCommandLine(string executablePath, string configurationPath, string server, string database, string adminUser, string adminPass, string query)
        {
            throw new NotImplementedException();
        }
    }
}
