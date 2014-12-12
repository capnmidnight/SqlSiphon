using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlSiphon
{
    public interface ISqlSiphon : IDisposable
    {
        DatabaseDelta Analyze(Regex filter);
        void AlterDatabase(string script);
        void MarkScriptAsRan(string script);

        event DataProgressEventHandler Progress;

        string DefaultSchemaName { get; }

        List<string> GetSchemata();
        List<InformationSchema.Columns> GetColumns();
        List<InformationSchema.Routines> GetRoutines();
        List<InformationSchema.Parameters> GetParameters();
        List<InformationSchema.KeyColumnUsage> GetKeyColumns();
        List<InformationSchema.ConstraintColumnUsage> GetConstraintColumns();
        List<InformationSchema.TableConstraints> GetTableConstraints();
        List<InformationSchema.ReferentialConstraints> GetReferentialConstraints();

        string MakeIdentifier(params string[] parts);
        bool DescribesIdentity(InformationSchema.Columns column);
        bool ColumnChanged(Mapping.MappedPropertyAttribute final, Mapping.MappedPropertyAttribute initial);
        bool RoutineChanged(Mapping.MappedMethodAttribute final, Mapping.MappedMethodAttribute initial);
        bool KeyChanged(Mapping.PrimaryKey final, Mapping.PrimaryKey initial);
        void AnalyzeQuery(string routineText, Mapping.MappedMethodAttribute routine);

        Mapping.MappedMethodAttribute GetCommandDescription(System.Reflection.MethodInfo method);

        Type GetSystemType(string sqlType);

        string MakeCreateSchemaScript(string schemaName);
        string MakeDropSchemaScript(string schemaName);

        string MakeCreateTableScript(Mapping.MappedClassAttribute table);
        string MakeDropTableScript(Mapping.MappedClassAttribute table);

        string MakeCreateColumnScript(Mapping.MappedPropertyAttribute column);
        string MakeDropColumnScript(Mapping.MappedPropertyAttribute column);
        string MakeAlterColumnScript(Mapping.MappedPropertyAttribute final, Mapping.MappedPropertyAttribute initial);

        string MakeDropRoutineScript(Mapping.MappedMethodAttribute routine);
        string MakeCreateRoutineScript(Mapping.MappedMethodAttribute routine);
        string MakeAlterRoutineScript(Mapping.MappedMethodAttribute routine);

        string MakeDropRelationshipScript(Mapping.Relationship relation);
        string MakeCreateRelationshipScript(Mapping.Relationship relation);

        string MakeDropPrimaryKeyScript(Mapping.PrimaryKey key);
        string MakeCreatePrimaryKeyScript(Mapping.PrimaryKey key);

        bool RelationshipChanged(Mapping.Relationship finalRelation, Mapping.Relationship initialRelation);
    }
}
