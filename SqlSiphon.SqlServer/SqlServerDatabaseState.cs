using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSiphon.Mapping;

namespace SqlSiphon.SqlServer
{
    internal class SqlServerDatabaseState : DatabaseState
    {
        public Dictionary<string, TableAttribute> UDTTs;
        public SqlServerDatabaseState(DatabaseState state)
            : base(state)
        {
            this.UDTTs = new Dictionary<string, TableAttribute>();
        }
        /*

        public override DatabaseDelta Diff(DatabaseState initial, IAssemblyStateReader asm, IDatabaseScriptGenerator dal)
        {
            var delta = base.Diff(initial, asm, dal);
            var ss = initial as SqlServerDatabaseState;
            if (ss != null)
            {
                ProcessUDTTs(dal, delta, ss.UDTTs);
            }
            return delta;
        }


        private void ProcessUDTTs(IDatabaseObjectHandler dal, DatabaseDelta delta, Dictionary<string, UserDefinedTableType> udtts)
        {
            foreach (var udtt in this.UDTTs)
            {
                if (!udtts.ContainsKey(udtt.Key))
                {
                    var schemaName = dal.MakeIdentifier(udtt.Key);
                    delta.Scripts.Add(new ScriptStatus(
                        ScriptType.CreateUDTT,
                        string.Format("{0} v{1}", udtt.Key, udtt.Value.Version),
                        string.Format("create extension if not exists \"{0}\" with schema {1};",
                            udtt.Key,
                            schemaName)));
                }
                else if (udtts[udtt.Key].Version < udtt.Value.Version)
                {
                    delta.Scripts.Add(new ScriptStatus(
                        ScriptType.InstallExtension,
                        string.Format("{0} v{1}", udtt.Key, udtt.Value.Version),
                        string.Format("alter extension \"{0}\" update;", udtt.Key)));
                }
            }
        }

        public void AddUDTT(UserDefinedTableType udtt)
        {
            this.AddUDTT(ext.extname, ext.extversion);
        }

        public void AddUDTT(string name, string version)
        {
            this.UDTTs.Add(name, new UserDefinedTableType(name, version));
        }

        public void SynchronizeUserDefinedTableTypes()
        {
            var type = this.GetType();
            var methods = type.GetMethods();
            var complexToSync = new List<Type>();
            var simpleToSync = new List<Type>();
            foreach (var method in methods)
            {
                foreach (var parameter in method.GetParameters())
                {
                    if (UserDefinedTableType.IsUDTT(parameter.ParameterType))
                    {
                        var t = parameter.ParameterType;
                        if (t.IsArray)
                            t = t.GetElementType();
                        if (UserDefinedTableType.IsUDTT(t))
                            complexToSync.Add(t);
                        else
                            simpleToSync.Add(t);
                    }
                }
            }

            int total = simpleToSync.Count + complexToSync.Count;
            int current = 0;
            foreach (var t in simpleToSync.Distinct())
            {
                SynchronizeSimpleUDTT(t);
                current++;
            }

            foreach (var c in complexToSync.Distinct())
            {
                MaybeSynchronizeUDTT(c);
                current++;
            }
        }

        private void MaybeSynchronizeUDTT(Type t)
        {
            var attr = DatabaseObjectAttribute.GetAttribute<SqlServerTableAttribute>(t);
            if (attr != null && attr.IsUploadable)
                SynchronizeComplexUDTT(t, attr);
        }

        private void SynchronizeSimpleUDTT(Type t)
        {
            var name = MakeUDTTName(t);
            var fullName = MakeIdentifier(DefaultSchemaName, name);
            if (this.UDTTExists(DefaultSchemaName, name))
            {
                this.DropUDTT(fullName);
            }
            this.CreateSimpleUDTT(fullName, t);
        }

        private void SynchronizeComplexUDTT(Type c, SqlServerTableAttribute attr)
        {
            var schema = DefaultSchemaName;
            if (attr != null && attr.Schema != null)
                schema = attr.Schema;
            var name = MakeUDTTName(c);
            var fullName = MakeIdentifier(schema, name);
            if (this.UDTTExists(schema, name))
            {
                this.DropUDTT(fullName);
            }
            this.CreateComplexUDTT(fullName, c);
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization | MethodImplOptions.PreserveSig)]
        [MappedMethod(CommandType = CommandType.Text,
            Query =
@"SELECT types.name
FROM sys.types
	inner join sys.schemas on types.schema_id = schemas.schema_id
where is_user_defined = 1
	and is_table_type = 1
	and schemas.name = @schemaName
	and types.name = @UDTTName;")]
        protected bool UDTTExists(string schemaName, string UDTTName)
        {
            return this.GetList<string>("name", schemaName, UDTTName).Count >= 1;
        }

        private void DropUDTT(string fullName)
        {
            try
            {
                this.ExecuteQuery(string.Format("DROP TYPE {0}", fullName));
            }
            catch (Exception exp)
            {
                throw new Exception(string.Format("Could not create UDTT: {0}. Reason: {1}", fullName, exp.Message), exp);
            }
        }

        private void CreateComplexUDTT(string fullName, Type mappedClass)
        {
            string script = CreateComplexUDTTScript(fullName, mappedClass);

            if (script != null)
            {
                try
                {
                    this.ExecuteQuery(script);
                }
                catch (Exception exp)
                {
                    throw new Exception(string.Format("Could not create UDTT: {0}. Reason: {1}", fullName, exp.Message), exp);
                }
            }
        }

        public string CreateComplexUDTTScript(string fullName, Type mappedClass)
        {
            var sb = new StringBuilder();
            var columns = GetProperties(mappedClass);
            // don't upload auto-incrementing identity columns
            // or columns that have a default value defined
            var colStrings = columns
                .Where(c => c.Include && !c.IsIdentity && (c.IsIncludeSet || c.DefaultValue == null))
                .Select(c => this.MaybeMakeColumnTypeString(c, true))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
            if (colStrings.Length == 0)
            {
                return null;
            }
            else
            {
                var columnDefinition = string.Join("," + Environment.NewLine + "    ", colStrings);
                return string.Format(
    @"CREATE TYPE {0} AS TABLE(
    {1}
)",
                    fullName,
                    columnDefinition);
            }
        }

        private void CreateSimpleUDTT(string fullName, Type mappedClass)
        {
            var sb = new StringBuilder();
            var columnDefinition = reverseTypeMapping[mappedClass];
            var script = string.Format(@"CREATE TYPE {0} AS TABLE(Value {1})", fullName, columnDefinition);

            try
            {
                this.ExecuteQuery(script);
            }
            catch (Exception exp)
            {
                throw new Exception(string.Format("Could not create UDTT: {0}. Reason: {1}", fullName, exp.Message), exp);
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
*/
    }
}
