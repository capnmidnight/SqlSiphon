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
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon.SqlServer
{
    /// <summary>
    /// A base class for building Data Access Layers that connect to MS SQL Server 2005/2008
    /// databases and execute store procedures stored within.
    /// </summary>
    public partial class SqlServerDataAccessLayer : DataAccessLayer<SqlConnection, SqlCommand, SqlParameter, SqlDataAdapter, SqlDataReader>
    {
        /// <summary>
        /// creates a new connection to a MS SQL Server 2005/2008 database and automatically
        /// opens the connection. 
        /// </summary>
        /// <param name="connectionString">a standard MS SQL Server connection string</param>
        public SqlServerDataAccessLayer(string connectionString)
            : base(connectionString)
        {
        }

        public SqlServerDataAccessLayer(SqlConnection connection)
            : base(connection)
        {
        }

        public SqlServerDataAccessLayer(SqlServerDataAccessLayer dal)
            : base(dal)
        {
        }

        protected override string IdentifierPartBegin { get { return "["; } }
        protected override string IdentifierPartEnd { get { return "]"; } }
        public override string DefaultSchemaName { get { return "dbo"; } }

        private static Dictionary<string, Type> typeMapping;
        private static Dictionary<Type, string> reverseTypeMapping;
        static SqlServerDataAccessLayer()
        {
            typeMapping = new Dictionary<string, Type>();
            typeMapping.Add("bigint", typeof(long));
            typeMapping.Add("int", typeof(int));
            typeMapping.Add("smallint", typeof(short));
            typeMapping.Add("tinyint", typeof(byte));
            typeMapping.Add("decimal", typeof(decimal));
            typeMapping.Add("numeric", typeof(decimal));
            typeMapping.Add("money", typeof(decimal));
            typeMapping.Add("smallmoney", typeof(decimal));
            typeMapping.Add("bit", typeof(bool));
            typeMapping.Add("float", typeof(float));
            typeMapping.Add("real", typeof(double));
            typeMapping.Add("datetime2", typeof(DateTime));
            typeMapping.Add("datetime", typeof(DateTime));
            typeMapping.Add("smalldatetime", typeof(DateTime));
            typeMapping.Add("date", typeof(DateTime));
            typeMapping.Add("datetimeoffset", typeof(DateTime));
            typeMapping.Add("time", typeof(DateTime));
            typeMapping.Add("timestamp", typeof(DateTime));
            typeMapping.Add("nvarchar", typeof(string));
            typeMapping.Add("char", typeof(string));
            typeMapping.Add("varchar", typeof(string));
            typeMapping.Add("text", typeof(string));
            typeMapping.Add("nchar", typeof(string));
            typeMapping.Add("ntext", typeof(string));
            typeMapping.Add("varbinary", typeof(byte[]));
            typeMapping.Add("binary", typeof(byte[]));
            typeMapping.Add("image", typeof(byte[]));
            typeMapping.Add("uniqueidentifier", typeof(Guid));

            reverseTypeMapping = typeMapping
                .GroupBy(kv => kv.Value, kv => kv.Key)
                .ToDictionary(g => g.Key, g => g.First());

            reverseTypeMapping.Add(typeof(char[]), "nchar");

            reverseTypeMapping.Add(typeof(int?), "int");
            reverseTypeMapping.Add(typeof(uint), "int");
            reverseTypeMapping.Add(typeof(uint?), "int");

            reverseTypeMapping.Add(typeof(long?), "bigint");
            reverseTypeMapping.Add(typeof(ulong), "bigint");
            reverseTypeMapping.Add(typeof(ulong?), "bigint");

            reverseTypeMapping.Add(typeof(short?), "smallint");
            reverseTypeMapping.Add(typeof(ushort), "smallint");
            reverseTypeMapping.Add(typeof(ushort?), "smallint");

            reverseTypeMapping.Add(typeof(byte?), "tinyint");
            reverseTypeMapping.Add(typeof(sbyte), "tinyint");
            reverseTypeMapping.Add(typeof(sbyte?), "tinyint");
            reverseTypeMapping.Add(typeof(char), "tinyint");
            reverseTypeMapping.Add(typeof(char?), "tinyint");

            reverseTypeMapping.Add(typeof(decimal?), "decimal");
            reverseTypeMapping.Add(typeof(bool?), "bit");
            reverseTypeMapping.Add(typeof(float?), "float");
            reverseTypeMapping.Add(typeof(double?), "real");
            reverseTypeMapping.Add(typeof(DateTime?), "datetime2");
            reverseTypeMapping.Add(typeof(Guid?), "uniqueidentifier");
        }

        protected override void ModifyQuery(MappedMethodAttribute info)
        {
            if (info.EnableTransaction)
            {
                string transactionName = string.Format("TRANS{0}", Guid.NewGuid().ToString().Replace("-", "")).Substring(0, 32);
                string transactionBegin = string.Format("begin try\r\nbegin transaction {0};", transactionName);
                string transactionEnd = string.Format(
@"commit transaction {0};
end try
begin catch
    declare @msg nvarchar(4000), @lvl int, @stt int;
    select @msg = error_message(), @lvl = error_severity(), @stt = error_state();
    rollback transaction {0};
    raiserror(@msg, @lvl, @stt);
end catch;", transactionName);
                info.Query = string.Format("{0}\r\n{1}\r\n{2}", transactionBegin, info.Query, transactionEnd);
            }
        }

        protected override string MakeDropProcedureScript(MappedMethodAttribute info)
        {
            return string.Format("drop procedure {0}", this.MakeIdentifier(info.Schema, info.Name));
        }

        protected override string MakeCreateProcedureScript(MappedMethodAttribute info)
        {
            var identifier = this.MakeIdentifier(info.Schema ?? DefaultSchemaName, info.Name);
            var parameterSection = this.MakeParameterSection(info);
            return string.Format(
@"create procedure {0}
    {1}
as begin
    set nocount on;
    {2}
end",
                identifier,
                parameterSection,
                info.Query);
        }

        public override string MakeCreateTableScript(MappedClassAttribute info)
        {
            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = this.MakeIdentifier(schema, info.Name);
            var columnSection = this.MakeColumnSection(info);
            var pk = info.Properties.Where(p => p.IncludeInPrimaryKey).ToArray();
            var pkString = "";
            if (pk.Length > 0)
            {
                pkString = string.Format(",{0}    constraint PK_{1}_{2} primary key({3}){0}",
                    Environment.NewLine,
                    schema,
                    info.Name,
                    string.Join(",", pk.Select(c => c.Name)));
            }
            return string.Format(
@"if not exists(select * from information_schema.tables where table_schema = '{0}' and table_name = '{1}')
create table {2}(
    {3}{4}
)",
                schema,
                info.Name,
                identifier,
                columnSection,
                pkString);
        }

        public override string MakeDropTableScript(MappedClassAttribute info)
        {
            var schema = info.Schema ?? DefaultSchemaName;
            var identifier = this.MakeIdentifier(schema, info.Name);
            return string.Format(
@"if exists(select * from information_schema.tables where table_schema = '{0}' and table_name = '{1}')
    drop table {2};",
                schema,
                info.Name,
                identifier);
        }

        protected override string MakeParameterString(MappedParameterAttribute p)
        {
            var typeStr = MakeSqlTypeString(p);
            return string.Join(" ",
                "@" + p.Name,
                typeStr,
                p.DefaultValue ?? "").Trim();
        }

        protected override string MakeColumnString(MappedPropertyAttribute p)
        {
            var typeStr = MakeSqlTypeString(p);
            var defaultString = "";
            if (p.DefaultValue != null)
                defaultString = string.Format("DEFAULT ({0})", p.DefaultValue);
            else if (p.IsIdentity)
                defaultString = "IDENTITY(1, 1)";

            return string.Format("{0} {1} {2} {3}",
                p.Name,
                typeStr,
                p.IsOptional ? "" : "NOT NULL",
                defaultString);
        }

        public override string MakeCreateColumnScript(MappedPropertyAttribute prop)
        {
            return string.Format("alter table {0} add {1} {2};",
                this.MakeIdentifier(prop.Table.Schema, prop.Table.Name),
                this.MakeIdentifier(prop.Name),
                prop.SqlType);
        }

        public override string MakeDropColumnScript(MappedPropertyAttribute prop)
        {
            return string.Format("alter table {0} drop column {1};",
                this.MakeIdentifier(prop.Table.Schema, prop.Table.Name),
                this.MakeIdentifier(prop.Name));
        }
        
        public override string MakeAlterColumnScript(MappedPropertyAttribute final, MappedPropertyAttribute initial)
        {
            var temp = final.DefaultValue;
            final.DefaultValue = null;
            var col = string.Format("alter table {0} alter column {1};",
                this.MakeIdentifier(final.Table.Schema ?? DefaultSchemaName, final.Table.Name),
                this.MakeColumnString(final));
            final.DefaultValue = temp;
            return col;
        }

        protected override string MakeDefaultConstraintScript(InformationSchema.Columns c, MappedPropertyAttribute prop)
        {
            return string.Format("alter table {0} add constraint DEF_{1}_{2} default {3} for {2}",
               MakeIdentifier(c.table_schema ?? DefaultSchemaName, c.table_name),
               c.table_name,
               c.column_name,
               prop.DefaultValue);
        }


        protected override string MakeSqlTypeString(string sqlType, Type systemType, int? size, int? precision)
        {
            if (sqlType == null && reverseTypeMapping.ContainsKey(systemType))
            {
                sqlType = reverseTypeMapping[systemType];
            }

            if (sqlType != null)
            {
                if (sqlType[sqlType.Length - 1] == ')') // someone already setup the type name, so skip it
                    return sqlType;
                else
                {
                    var typeStr = new StringBuilder(sqlType);
                    if (size.HasValue)
                    {
                        typeStr.AppendFormat("({0}", size);
                        if (precision.HasValue)
                        {
                            typeStr.AppendFormat(", {0}", precision);
                        }
                        typeStr.Append(")");
                    }

                    if (sqlType.Contains("var")
                        && typeStr[typeStr.Length - 1] != ')')
                    {
                        typeStr.Append("(MAX)");
                    }
                    return typeStr.ToString();
                }
            }
            else
            {
                return null;
            }
        }

        private string MaybeMakeColumnTypeString(MappedPropertyAttribute attr, bool skipDefault = false)
        {
            if (reverseTypeMapping.ContainsKey(attr.SystemType))
            {
                var typeStr = MakeSqlTypeString(attr);
                return string.Format("{0} {1} {2}NULL {3}",
                    attr.Name,
                    typeStr,
                    attr.IsOptional ? "" : "NOT ",
                    !skipDefault ? attr.DefaultValue ?? "" : "").Trim();
            }
            return null;
        }

        protected override object PrepareParameter(object parameterValue)
        {
            if (parameterValue != null)
            {
                var t = parameterValue.GetType();
            }
            return parameterValue;
        }

        private static DataTable MakeDataTable(string tableName, Type t, System.Collections.IEnumerable array)
        {
            var table = new DataTable(tableName);
            if (reverseTypeMapping.ContainsKey(t))
            {
                table.Columns.Add("Value", t);
                foreach (object obj in array)
                {
                    table.Rows.Add(new object[] { obj });
                }
            }
            else
            {
                // don't upload auto-incrementing identity columns
                // or columns that have a default value defined
                var props = GetProperties(t);
                var columns = props
                    .Where(p => p.Include && !p.IsIdentity && (p.IsIncludeSet || p.DefaultValue == null))
                    .ToList();
                foreach (var column in columns)
                {
                    table.Columns.Add(column.Name, column.SystemType);
                }
                foreach (object obj in array)
                {
                    List<object> row = new List<object>();
                    foreach (var column in columns)
                    {
                        var element = column.GetValue<object>(obj);
                        row.Add(element);
                    }
                    table.Rows.Add(row.ToArray());
                }
            }
            return table;
        }

        protected override string MakeFKScript(string tableSchema, string tableName, string tableColumns, string foreignSchema, string foreignName, string foreignColumns)
        {
            var constraintName = string.Join("_",
                "FK",
                tableSchema ?? DefaultSchemaName,
                tableName,
                tableColumns.Replace(',', '_'),
                "to",
                foreignSchema ?? DefaultSchemaName,
                foreignName);

            var tableFullName = MakeIdentifier(
                tableSchema ?? DefaultSchemaName,
                tableName);

            var foreignFullName = MakeIdentifier(
                foreignSchema ?? DefaultSchemaName,
                foreignName);

            return string.Format(
@"if not exists(SELECT *  FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_NAME ='{0}')
    alter table {1} add constraint {2}
    foreign key({3})
    references {4}({5});",
                    constraintName,
                    tableFullName,
                    MakeIdentifier(constraintName),
                    tableColumns,
                    foreignFullName,
                    foreignColumns);
        }

        public override bool DescribesIdentity(ref string defaultValue)
        {
            throw new NotImplementedException();
        }

        public override List<InformationSchema.ConstraintColumnUsage> GetColumnConstraints()
        {
            throw new NotImplementedException();
        }

        public override List<InformationSchema.Columns> GetColumns()
        {
            throw new NotImplementedException();
        }

        public override List<InformationSchema.Parameters> GetParameters()
        {
            throw new NotImplementedException();
        }

        public override List<InformationSchema.Routines> GetRoutines()
        {
            throw new NotImplementedException();
        }

        public override List<InformationSchema.ReferentialConstraints> GetReferentialConstraints()
        {
            throw new NotImplementedException();
        }

        public override List<InformationSchema.TableConstraints> GetTableConstraints()
        {
            throw new NotImplementedException();
        }

        protected override string MakeIndexScript(string indexName, string tableSchema, string tableName, string[] tableColumns)
        {
            var columnSection = string.Join(",", tableColumns.Select(c => c + " ASC"));
            var identifier = MakeIdentifier(tableSchema ?? DefaultSchemaName, tableName);
            return string.Format(
@"if not exists(select * from sys.indexes where name = '{0}')
CREATE NONCLUSTERED INDEX {0} ON {1}({2})",
                indexName,
                identifier,
                columnSection);
        }

        public override void Insert<T>(IEnumerable<T> data)
        {
            if (data != null)
            {
                var t = typeof(T);
                var attr = MappedObjectAttribute.GetAttribute<MappedClassAttribute>(t);
                if (attr == null)
                {
                    throw new Exception(string.Format("Type {0}.{1} could not be automatically inserted.", t.Namespace, t.Name));
                }
                attr.InferProperties(t);
                var tableData = MakeDataTable(attr.Name, t, data);

                //should make it using bulk insert when mono-project fix it for varbinary data
                //see https://bugzilla.xamarin.com/show_bug.cgi?id=20563
                var usesVarBinary = tableData.Columns.Cast<DataColumn>().Any(c => c.DataType == typeof(byte[]));
                if (IsOnMonoRuntime && usesVarBinary)
                {
                    base.Insert(data);
                }
                else
                {
                    if (this.Connection.State == ConnectionState.Closed)
                    {
                        this.Connection.Open();
                    }
                    var bulkCopy = new SqlBulkCopy(this.Connection);
                    foreach (var column in tableData.Columns.Cast<DataColumn>())
                    {
                        bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }
                    bulkCopy.DestinationTableName = attr.Name;
                    bulkCopy.WriteToServer(tableData);
                }
            }
        }

        protected override bool IsTypeChanged(InformationSchema.Columns column, MappedPropertyAttribute property)
        {
            var sizeSet = false;
            var precisionSet = false;
            int size = 0;
            if (column.data_type == "nvarchar"
                || column.data_type == "varchar")
            {
                if (column.character_maximum_length != null
                    && column.character_maximum_length != -1)
                {
                    sizeSet = true;
                    size = column.character_maximum_length.Value;
                }
            }
            else
            {
                if (column.numeric_precision != null
                    && !((column.data_type == "int"
                            || column.data_type == "integer")
                        && column.numeric_precision == 10)
                    && !(column.data_type == "real"
                        && column.numeric_precision == 24)
                    && column.numeric_precision != 0)
                {
                    precisionSet = true;
                }
                if (column.numeric_scale != null
                        && column.numeric_scale != 0)
                {
                    sizeSet = true;
                    size = column.numeric_scale.Value;
                }
            }

            var newType = this.MakeSqlTypeString(property);

            var changed = (column.is_nullable.ToLower() == "yes") != property.IsOptional
                || column.data_type != newType
                || sizeSet != property.IsSizeSet
                || size != property.Size
                || precisionSet != property.IsPrecisionSet
                || column.numeric_precision.Value != property.Precision;

            return changed;
        }

        public override Type GetSystemType(string sqlType)
        {
            return typeMapping.ContainsKey(sqlType) ? typeMapping[sqlType] : null;
        }
    }
}