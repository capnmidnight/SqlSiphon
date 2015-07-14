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

        private static Dictionary<short, string> PARAMETER_MODES;

        private static Dictionary<string, Type> stringToType;
        private static Dictionary<Type, string> typeToString;
        private static Dictionary<short, Type> shortToType;
        private static Dictionary<Type, short> typeToShort;
        private static Dictionary<short, string> shortToString;
        private static Dictionary<string, short> stringToShort;

        private static void SetTypeMappings<T>(string name, DAO.DataTypeEnum val)
        {
            SetTypeInformation(typeof(T), name, val);
        }

        private static void SetTypeInformation(Type type, string name, DAO.DataTypeEnum val)
        {
            var sVal = (short)val;
            if (!stringToType.ContainsKey(name)) { stringToType.Add(name, type); }
            if (!typeToString.ContainsKey(type)) { typeToString.Add(type, name); }
            if (!shortToType.ContainsKey(sVal)) { shortToType.Add(sVal, type); }
            if (!typeToShort.ContainsKey(type)) { typeToShort.Add(type, sVal); }
            if (!shortToString.ContainsKey(sVal)) { shortToString.Add(sVal, name); }
            if (!stringToShort.ContainsKey(name)) { stringToShort.Add(name, sVal); }
        }

        static OleDBDataAccessLayer()
        {
            stringToType = new Dictionary<string, Type>();
            typeToString = new Dictionary<Type, string>();
            shortToType = new Dictionary<short, Type>();
            typeToShort = new Dictionary<Type, short>();
            shortToString = new Dictionary<short, string>();
            stringToShort = new Dictionary<string, short>();

            SetTypeInformation(typeof(void), "void", (DAO.DataTypeEnum)0);
            SetTypeInformation(typeof(void), "void", (DAO.DataTypeEnum)48);

            SetTypeMappings<byte[]>("binary", DAO.DataTypeEnum.dbBinary);
            SetTypeMappings<byte[]>("longbinary", DAO.DataTypeEnum.dbLongBinary);
            SetTypeMappings<byte[]>("varbinary", DAO.DataTypeEnum.dbVarBinary);

            SetTypeMappings<bool>("bit", DAO.DataTypeEnum.dbBoolean);
            SetTypeMappings<bool?>("bit", DAO.DataTypeEnum.dbBoolean);

            SetTypeMappings<byte>("byte", DAO.DataTypeEnum.dbByte);
            SetTypeMappings<byte?>("byte", DAO.DataTypeEnum.dbByte);
            SetTypeMappings<sbyte>("byte", DAO.DataTypeEnum.dbByte);
            SetTypeMappings<sbyte?>("byte", DAO.DataTypeEnum.dbByte);

            SetTypeMappings<char>("char", DAO.DataTypeEnum.dbChar);
            SetTypeMappings<char?>("char", DAO.DataTypeEnum.dbChar);

            SetTypeMappings<Guid>("guid", DAO.DataTypeEnum.dbGUID);
            SetTypeMappings<Guid?>("guid", DAO.DataTypeEnum.dbGUID);

            SetTypeMappings<int>("int", DAO.DataTypeEnum.dbInteger);
            SetTypeMappings<int?>("int", DAO.DataTypeEnum.dbInteger);
            SetTypeMappings<int>("int", DAO.DataTypeEnum.dbBigInt);
            SetTypeMappings<int?>("int", DAO.DataTypeEnum.dbBigInt);
            SetTypeMappings<int>("int", DAO.DataTypeEnum.dbLong);
            SetTypeMappings<int?>("int", DAO.DataTypeEnum.dbLong);
            SetTypeMappings<uint>("int", DAO.DataTypeEnum.dbInteger);
            SetTypeMappings<uint?>("int", DAO.DataTypeEnum.dbInteger);
            SetTypeMappings<short>("int", DAO.DataTypeEnum.dbInteger);
            SetTypeMappings<short?>("int", DAO.DataTypeEnum.dbInteger);
            SetTypeMappings<ushort>("int", DAO.DataTypeEnum.dbInteger);
            SetTypeMappings<ushort?>("int", DAO.DataTypeEnum.dbInteger);
            SetTypeMappings<long>("int", DAO.DataTypeEnum.dbInteger);
            SetTypeMappings<long?>("int", DAO.DataTypeEnum.dbInteger);
            SetTypeMappings<ulong>("int", DAO.DataTypeEnum.dbInteger);
            SetTypeMappings<ulong?>("int", DAO.DataTypeEnum.dbInteger);

            SetTypeMappings<decimal>("currency", DAO.DataTypeEnum.dbCurrency);
            SetTypeMappings<decimal?>("currency", DAO.DataTypeEnum.dbCurrency);
            SetTypeMappings<decimal>("decimal", DAO.DataTypeEnum.dbDecimal);
            SetTypeMappings<decimal?>("decimal", DAO.DataTypeEnum.dbDecimal);
            SetTypeMappings<decimal>("numeric", DAO.DataTypeEnum.dbNumeric);
            SetTypeMappings<decimal?>("numeric", DAO.DataTypeEnum.dbNumeric);

            SetTypeMappings<double>("double", DAO.DataTypeEnum.dbDouble);
            SetTypeMappings<double?>("double", DAO.DataTypeEnum.dbDouble);

            SetTypeMappings<float>("float", DAO.DataTypeEnum.dbFloat);
            SetTypeMappings<float?>("float", DAO.DataTypeEnum.dbFloat);
            SetTypeMappings<float>("single", DAO.DataTypeEnum.dbSingle);
            SetTypeMappings<float?>("single", DAO.DataTypeEnum.dbSingle);

            SetTypeMappings<string>("text", DAO.DataTypeEnum.dbText);
            SetTypeMappings<string>("memo", DAO.DataTypeEnum.dbMemo);

            SetTypeMappings<DateTime>("datetime", DAO.DataTypeEnum.dbDate);
            SetTypeMappings<DateTime?>("datetime", DAO.DataTypeEnum.dbDate);
            SetTypeMappings<DateTime>("time", DAO.DataTypeEnum.dbTime);
            SetTypeMappings<DateTime?>("time", DAO.DataTypeEnum.dbTime);
            SetTypeMappings<DateTime>("timestamp", DAO.DataTypeEnum.dbTimeStamp);
            SetTypeMappings<DateTime?>("timestamp", DAO.DataTypeEnum.dbTimeStamp);

            PARAMETER_MODES = new Dictionary<short, string>();
            PARAMETER_MODES.Add((short)DAO.ParameterDirectionEnum.dbParamInput, "IN");
            PARAMETER_MODES.Add((short)DAO.ParameterDirectionEnum.dbParamInputOutput, "INOUT");
            PARAMETER_MODES.Add((short)DAO.ParameterDirectionEnum.dbParamOutput, "OUT");
            PARAMETER_MODES.Add((short)DAO.ParameterDirectionEnum.dbParamReturnValue, "RETURN");

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
            if (!string.IsNullOrEmpty(this.DataSource)
                && !System.IO.File.Exists(this.DataSource))
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

        public event EventHandler Disposed;

        public override void Dispose()
        {
            try
            {
                base.Dispose();
            }
            finally
            {
                if (this.Disposed != null)
                {
                    this.Disposed(this, EventArgs.Empty);
                }
            }
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
            if (info.SystemType != null)
            {
                // validates the return type
                DatabaseObjectAttribute.GetAttribute(DataConnector.CoallesceCollectionType(info.SystemType));
            }

            var identifier = this.MakeIdentifier(info.Schema ?? DefaultSchemaName, info.Name);
            var parameterSection = this.MakeParameterSection(info);
            if (parameterSection.Length > 0)
            {
                parameterSection = string.Format("({0})", parameterSection);
            }
            return string.Format(
@"create procedure {0}{1} as {2}",
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
            var identifier = this.MakeIdentifier("_".Combine(schema, info.Name));
            var columnSection = this.MakeColumnSection(info, false);
            return string.Format(
@"create table {0}(
    {1}
);",
                identifier,
                columnSection);
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
                this.MakeIdentifier(p.Name),
                typeStr,
                p.IsOptional || p.IncludeInPrimaryKey ? "" : "NOT NULL")
                .Trim();
        }

        protected override string MakeSqlTypeString(string sqlType, Type systemType, int? size, int? precision, bool isIdentity)
        {
            if (string.IsNullOrEmpty(sqlType) && typeToString.ContainsKey(systemType))
            {
                sqlType = typeToString[systemType];
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

        public override List<InformationSchema.Columns> GetColumns()
        {
            return WithDAO(db =>
            {
                return db.TableDefs.Cast<DAO.TableDef>().SelectMany(table =>
                {
                    return table.Fields.Cast<DAO.Field>().Select(field =>
                    {
                        var attr = (DAO.FieldAttributeEnum)field.Attributes;
                        return new InformationSchema.Columns
                        {
                            table_name = table.Name,
                            column_name = field.Name,
                            data_type = shortToString[field.Type],
                            ordinal_position = field.OrdinalPosition,
                            is_nullable = field.Required ? "no" : "yes",
                            column_default = field.DefaultValue.ToString(),
                            character_maximum_length = field.Size,
                            is_identity = ((attr & DAO.FieldAttributeEnum.dbAutoIncrField) == DAO.FieldAttributeEnum.dbAutoIncrField) ? 1 : 0
                        };
                    });
                }).ToList();
            });
        }

        private object TranslateValue(object value, Type sourceType, Type targetType)
        {
            if (sourceType != targetType)
            {
                if (sourceType == typeof(int))
                {
                    if (targetType == typeof(string))
                    {
                        value = targetType.Name;
                    }
                    else if (targetType == typeof(byte))
                    {
                        value = (byte)(int)value;
                    }
                    else if (targetType == typeof(short))
                    {
                        value = (short)(int)value;
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

        private T WithDAO<T>(Func<DAO.Database, T> thunk)
        {
            // temporarily close the connection to the file
            bool wasOpen = false;
            if (this.Connection.State != ConnectionState.Closed)
            {
                wasOpen = true;
                this.Connection.Close();
            }

            // setup DAO
            var engine = new DAO.DBEngine();
            var db = engine.OpenDatabase(this.Connection.DataSource);

            try
            {
                return thunk(db);
            }
            finally
            {
                db.Close();
                // reopen the connection, if the user is expecting it.
                if (wasOpen)
                {
                    this.Connection.Open();
                }
            }
        }

        private void WithDAO(Action<DAO.Database> thunk)
        {
            WithDAO(db =>
            {
                thunk(db);
                return 0;
            });
        }

        public override void InsertAll(Type t, System.Collections.IEnumerable data)
        {
            WithDAO(db =>
            {
                // get the properties of the type we're inserting
                var attr = DatabaseObjectAttribute.GetAttribute(t);
                if (attr == null)
                {
                    throw new Exception(string.Format("Type {0}.{1} could not be automatically inserted.", t.Namespace, t.Name));
                }
                var props = attr.Properties.Where(p => p.Include && !p.IsIdentity).ToArray();
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
                recordSet.Close();
            });
        }

        public override List<InformationSchema.Routines> GetRoutines()
        {
            return WithDAO(db =>
            {
                return db.QueryDefs.Cast<DAO.QueryDef>().Select(query =>
                {
                    return new InformationSchema.Routines
                    {
                        data_type = shortToString[query.Type],
                        routine_definition = query.SQL,
                        routine_name = query.Name,
                        specific_name = query.Name
                    };
                }).ToList();
            });
        }

        public override string MakeRoutineIdentifier(RoutineAttribute routine)
        {
            return routine.Name;
        }

        public override List<string> GetDatabaseLogins()
        {
            return new List<string>();
        }

        public override List<string> GetSchemata()
        {
            return new List<string>();
        }

        public override List<InformationSchema.IndexColumnUsage> GetIndexColumns()
        {
            return new List<InformationSchema.IndexColumnUsage>();
        }

        public override List<InformationSchema.TableConstraints> GetTableConstraints()
        {
            return new List<InformationSchema.TableConstraints>();
        }

        public override List<InformationSchema.ReferentialConstraints> GetReferentialConstraints()
        {
            return WithDAO(db =>
            {
                return db.Relations.Cast<DAO.Relation>().Select(rel =>
                {
                    var a = (DAO.RelationAttributeEnum)rel.Attributes;
                    var b = rel.Fields.Cast<DAO.Field>().Select(f =>
                    {
                        var c = f.Name;
                        var d = f.Type;
                        return f;
                    }).ToArray();
                    var e = rel.ForeignTable;
                    var g = rel.Name;
                    var h = rel.PartialReplica;
                    var i = rel.Table;
                    return new InformationSchema.ReferentialConstraints
                    {
                        constraint_name = rel.Name,
                        unique_constraint_name = rel.ForeignTable
                    };
                }).ToList();
            });
        }

        public override List<InformationSchema.ConstraintColumnUsage> GetConstraintColumns()
        {
            return new List<InformationSchema.ConstraintColumnUsage>();
        }

        public override List<InformationSchema.KeyColumnUsage> GetKeyColumns()
        {
            return new List<InformationSchema.KeyColumnUsage>();
        }

        public override string DefaultSchemaName
        {
            get { return null; }
        }

        public override List<InformationSchema.Parameters> GetParameters()
        {
            return WithDAO<List<InformationSchema.Parameters>>(db =>
            {
                return db.QueryDefs.Cast<DAO.QueryDef>().SelectMany(query =>
                {
                    return query.Parameters.Cast<DAO.Parameter>().Select(param =>
                    {
                        return new InformationSchema.Parameters
                        {
                            specific_schema = null,
                            specific_name = query.Name,
                            parameter_name = param.Name,
                            parameter_mode = PARAMETER_MODES[param.Direction],
                            data_type = typeToString[shortToType[param.Type]]
                        };
                    });
                }).ToList();
            });
        }

        public override int DefaultTypePrecision(string typeName, int testPrecision)
        {
            throw new NotImplementedException();
        }

        public override Type GetSystemType(string sqlType)
        {
            return stringToType.ContainsKey(sqlType) ? stringToType[sqlType] : null;
        }

        public override bool DescribesIdentity(InformationSchema.Columns column)
        {
            return false;
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

        public override string MakeDropPrimaryKeyScript(PrimaryKey key)
        {
            throw new NotImplementedException();
        }

        public override string MakeCreateRelationshipScript(Relationship relation)
        {
            var fromColumns = string.Join(", ", relation.FromColumns.Select(c => this.MakeIdentifier(c.Name)));
            var toColumns = string.Join(", ", relation.To.PrimaryKey.KeyColumns.Select(c => this.MakeIdentifier(c.Name)));

            return string.Format(@"alter table {0} add foreign key({2}) references {3}({4});",
                    this.MakeIdentifier(relation.From.Schema ?? DefaultSchemaName, relation.From.Name),
                    this.MakeIdentifier(relation.GetName(this)),
                    fromColumns,
                    this.MakeIdentifier(relation.To.Schema ?? DefaultSchemaName, relation.To.Name),
                    toColumns);
        }

        public override string MakeCreateIndexScript(TableIndex index)
        {
            return string.Format("create index {0} on {1}({2});",
                this.MakeIdentifier(index.Name),
                this.MakeIdentifier(index.Table.Name),
                string.Join(", ", index.Columns.Select(k => this.MakeIdentifier(k))));
        }

        public override string MakeCreatePrimaryKeyScript(PrimaryKey key)
        {
            return string.Format("create index {0} on {1}({2}) with primary;",
                this.MakeIdentifier(key.Name),
                this.MakeIdentifier(key.Table.Name),
                string.Join(", ", key.KeyColumns.Select(k => this.MakeIdentifier(k.Name))));
        }

        public override string MakeDropIndexScript(TableIndex index)
        {
            throw new NotImplementedException();
        }

        public override string MakeInsertScript(TableAttribute table, object value)
        {
            var columns = table.Properties
                .Where(p => p.Include && !p.IsIdentity && (p.IsIncludeSet || p.DefaultValue == null))
                .ToArray();

            var columnNames = columns.Select(c => this.MakeIdentifier(c.Name)).ToArray();
            var columnValues = columns.Select(c =>
            {
                var v = c.GetValue(value);
                string val = null;
                if (v == null)
                {
                    value = "NULL";
                }
                else
                {
                    var t = v.GetType();
                    if (DataConnector.IsTypeBarePrimitive(t))
                    {
                        val = v.ToString();
                    }
                    else if (DataConnector.IsTypeQuotedPrimitive(t))
                    {
                        val = string.Format("'{0}'", v);
                    }
                    else
                    {
                        throw new Exception("Can't insert value");
                    }
                }
                return val;
            }).ToArray();

            return string.Format("insert into {0}({1}) values({2});",
                this.MakeIdentifier(table.Schema ?? DefaultSchemaName, table.Name),
                string.Join(", ", columnNames),
                string.Join(", ", columnValues));
        }

        public override bool RunCommandLine(string executablePath, string configurationPath, string server, string database, string adminUser, string adminPass, string query)
        {
            throw new InvalidOperationException("MS Access cannot be queried from the command line");
        }
    }
}
