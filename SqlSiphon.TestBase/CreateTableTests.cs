﻿using System;
using System.Collections.Generic;
using SqlSiphon;
using SqlSiphon.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSiphon.TestBase
{
    [TestClass]
    public abstract class CreateTableTests<DataConnectorFactoryType>
        where DataConnectorFactoryType : IDataConnectorFactory, new()
    {
        private DataConnectorFactoryType factory;
        public CreateTableTests()
        {
            factory = new DataConnectorFactoryType();
        }

        protected string GetScriptFor<T>()
        {
            // We aren't opening a connection, we're just trying to generate scripts
            // so it shouldn't be a problem to provide no connection string.
            using (var ss = (ISqlSiphon)this.factory.MakeConnector((string)null))
            {
                var t = typeof(T);
                var table = DatabaseObjectAttribute.GetAttribute(t);
                var sb = new System.Text.StringBuilder();
                sb.Append(ss.MakeCreateTableScript(table));
                if (table.PrimaryKey != null)
                {
                    sb.AppendLine();
                    sb.AppendLine("--");
                    sb.Append(ss.MakeCreatePrimaryKeyScript(table.PrimaryKey));
                }
                
                var relationships = table.GetRelationships();
                if (relationships.Count > 0)
                {
                    var tables = new Dictionary<Type, TableAttribute>();
                    tables.Add(t, table);
                    sb.AppendLine();
                    sb.Append("--");
                    foreach (var rel in relationships)
                    {
                        if (!tables.ContainsKey(rel.ToType))
                        {
                            tables.Add(rel.ToType, DatabaseObjectAttribute.GetAttribute(rel.ToType));
                        }
                        rel.ResolveColumns(tables, ss);
                        sb.AppendLine();
                        sb.Append(ss.MakeCreateRelationshipScript(rel));
                    }
                }

                if (table.Indexes.Count > 0)
                {
                    sb.AppendLine();
                    sb.Append("--");
                    foreach (var val in table.Indexes.Values)
                    {
                        sb.AppendLine();
                        sb.Append(ss.MakeCreateIndexScript(val));
                    }
                }
                if (table.EnumValues.Count > 0)
                {
                    sb.AppendLine();
                    sb.Append("--");
                    foreach (var val in table.EnumValues)
                    {
                        sb.AppendLine();
                        sb.Append(ss.MakeInsertScript(table, val));
                    }
                }
                return sb.ToString();
            }
        }
        
        [ExpectedException(typeof(TableHasNoColumnsException))]
        public abstract void CantCreateEmptyTables();

        [ExpectedException(typeof(PrimaryKeyColumnNotNullableException))]
        public abstract void CantCreateNullablePK();

        [ExpectedException(typeof(MustSetStringSizeInPrimaryKeyException))]
        public abstract void CantCreatePKWithMAXString();

        public abstract void CreateSingleColumnTable();
        public abstract void CreateSingleColumnTableWithSchema();
        public abstract void CreateTwoColumnTable();
        public abstract void CreateTwoColumnTableAsChild();
        public abstract void CreateOneNullableColumn();
        public abstract void CreateWithPK();
        public abstract void CreateLongerPrimaryKey();
        public abstract void CreateWithIdentity();
        public abstract void CreateTableFromEnumeration();
        public abstract void CreateTableWithSimpleIndex();
        public abstract void CreateTableWithLongIndex();
        public abstract void CreateTableWithFK();
    }
}