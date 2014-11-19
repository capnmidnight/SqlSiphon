using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon
{
    public interface ISqlSiphon : IDisposable
    {
        DatabaseDelta Analyze();
        void AlterDatabase(string script);
        void MarkScriptAsRan(string script);

        event DataProgressEventHandler Progress;

        string DefaultSchemaName { get; }

        string FK<T, F>();
        string FK<T, F>(string tableColumns);
        string FK<T, F>(string tableColumns, string foreignColumns);
        string FK<T>(string foreignName, string foreignColumns);
        string FK<T>(string foreignSchema, string foreignName, string foreignColumns);
        string FK<T>(string tableColumns, string foreignSchema, string foreignName, string foreignColumns);

        List<InformationSchema.Columns> GetColumns();
        List<InformationSchema.Routines> GetRoutines();
        List<InformationSchema.Parameters> GetParameters();
        List<InformationSchema.ConstraintColumnUsage> GetColumnConstraints();
        List<InformationSchema.TableConstraints> GetTableConstraints();
        List<InformationSchema.ReferentialConstraints> GetReferentialConstraints();

        string MakeIdentifier(params string[] parts);
        bool DescribesIdentity(ref string defaultValue);

        Type GetSystemType(string sqlType);

        string MakeCreateTableScript(Mapping.MappedClassAttribute table);
        string MakeDropTableScript(Mapping.MappedClassAttribute table);
        string MakeCreateColumnScript(Mapping.MappedPropertyAttribute column);
    }
}
