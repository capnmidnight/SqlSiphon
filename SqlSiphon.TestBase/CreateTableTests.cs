using System;
using System.Collections.Generic;

using NUnit.Framework;

using SqlSiphon.Mapping;

namespace SqlSiphon.TestBase
{
    [TestFixture]
    public abstract class CreateTableTests
    {
        protected abstract ISqlSiphon MakeConnector();

        protected string GetScriptFor<T>()
        {
            // We aren't opening a connection, we're just trying to generate scripts
            // so it shouldn't be a problem to provide no connection string.
            using var ss = MakeConnector();
            var t = typeof(T);
            var table = DatabaseObjectAttribute.GetTable(ss, t);
            var sb = new System.Text.StringBuilder();
            sb.Append(ss.MakeCreateTableScript(table));
            if (table.PrimaryKey != null)
            {
                sb.AppendLine();
                sb.AppendLine("--");
                sb.Append(ss.MakeCreatePrimaryKeyScript(table.PrimaryKey));
            }

            var relationships = table.GetRelationships(ss);
            if (relationships.Count > 0)
            {
                var tables = new Dictionary<Type, TableAttribute>
                    {
                        { t, table }
                    };
                sb.AppendLine();
                sb.Append("--");
                foreach (var rel in relationships)
                {
                    if (!tables.ContainsKey(rel.ToType))
                    {
                        tables.Add(rel.ToType, DatabaseObjectAttribute.GetTable(ss, rel.ToType));
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

        public abstract void CantCreateEmptyTables();

        public abstract void CantCreateNullablePK();

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
        public abstract void CreateTableWithLongFK();
    }
}
