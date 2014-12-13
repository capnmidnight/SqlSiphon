using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SqlSiphon.Mapping;

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
        bool ColumnChanged(MappedPropertyAttribute final, MappedPropertyAttribute initial);
        bool RoutineChanged(MappedMethodAttribute final, MappedMethodAttribute initial);
        bool KeyChanged(PrimaryKey final, PrimaryKey initial);
        void AnalyzeQuery(string routineText, MappedMethodAttribute routine);

        MappedMethodAttribute GetCommandDescription(System.Reflection.MethodInfo method);

        Type GetSystemType(string sqlType);

        string MakeCreateSchemaScript(string schemaName);
        string MakeDropSchemaScript(string schemaName);

        string MakeCreateTableScript(MappedClassAttribute table);
        string MakeDropTableScript(MappedClassAttribute table);

        string MakeCreateColumnScript(MappedPropertyAttribute column);
        string MakeDropColumnScript(MappedPropertyAttribute column);
        string MakeAlterColumnScript(MappedPropertyAttribute final, MappedPropertyAttribute initial);

        string MakeDropRoutineScript(MappedMethodAttribute routine);
        string MakeCreateRoutineScript(MappedMethodAttribute routine);
        string MakeAlterRoutineScript(MappedMethodAttribute final, MappedMethodAttribute initial);

        string MakeDropRelationshipScript(Relationship relation);
        string MakeCreateRelationshipScript(Relationship relation);

        string MakeDropPrimaryKeyScript(PrimaryKey key);
        string MakeCreatePrimaryKeyScript(PrimaryKey key);

        bool RelationshipChanged(Relationship finalRelation, Relationship initialRelation);
    }
}
