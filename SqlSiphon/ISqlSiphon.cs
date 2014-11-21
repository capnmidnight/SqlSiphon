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

        List<InformationSchema.Columns> GetColumns();
        List<InformationSchema.Routines> GetRoutines();
        List<InformationSchema.Parameters> GetParameters();
        List<InformationSchema.ConstraintColumnUsage> GetColumnConstraints();
        List<InformationSchema.TableConstraints> GetTableConstraints();
        List<InformationSchema.ReferentialConstraints> GetReferentialConstraints();

        string MakeIdentifier(params string[] parts);
        bool DescribesIdentity(ref string defaultValue);
        bool ColumnChanged(Mapping.MappedPropertyAttribute fc, Mapping.MappedPropertyAttribute ic);

        Type GetSystemType(string sqlType);

        string MakeCreateTableScript(Mapping.MappedClassAttribute table);
        string MakeDropTableScript(Mapping.MappedClassAttribute table);
        string MakeCreateColumnScript(Mapping.MappedPropertyAttribute column);
        string MakeDropColumnScript(Mapping.MappedPropertyAttribute column);
        string MakeAlterColumnScript(Mapping.MappedPropertyAttribute final, Mapping.MappedPropertyAttribute initial);
        string MakeDropRoutineScript(Mapping.MappedMethodAttribute routine);
        string MakeCreateRoutineScript(Mapping.MappedMethodAttribute routine);
        string MakeDropRelationshipScript(Mapping.Relationship relation);
        string MakeCreateRelationshipScript(Mapping.Relationship relation);
    }
}
