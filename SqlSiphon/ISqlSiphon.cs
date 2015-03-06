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
        List<InformationSchema.IndexColumnUsage> GetIndexColumns();
        List<InformationSchema.Routines> GetRoutines();
        List<InformationSchema.Parameters> GetParameters();
        List<InformationSchema.KeyColumnUsage> GetKeyColumns();
        List<InformationSchema.ConstraintColumnUsage> GetConstraintColumns();
        List<InformationSchema.TableConstraints> GetTableConstraints();
        List<InformationSchema.ReferentialConstraints> GetReferentialConstraints();

        string MakeIdentifier(params string[] parts);
        bool DescribesIdentity(InformationSchema.Columns column);
        bool ColumnChanged(ColumnAttribute final, ColumnAttribute initial);
        bool RoutineChanged(SavedRoutineAttribute final, SavedRoutineAttribute initial);
        bool KeyChanged(PrimaryKey final, PrimaryKey initial);
        void AnalyzeQuery(string routineText, SavedRoutineAttribute routine);

        SavedRoutineAttribute GetCommandDescription(System.Reflection.MethodInfo method);

        Type GetSystemType(string sqlType);

        string MakeCreateSchemaScript(string schemaName);
        string MakeDropSchemaScript(string schemaName);

        string MakeCreateTableScript(TableAttribute table);
        string MakeDropTableScript(TableAttribute table);

        string MakeCreateColumnScript(ColumnAttribute column);
        string MakeDropColumnScript(ColumnAttribute column);
        string MakeAlterColumnScript(ColumnAttribute final, ColumnAttribute initial);

        string MakeDropRoutineScript(SavedRoutineAttribute routine);
        string MakeCreateRoutineScript(SavedRoutineAttribute routine);
        string MakeAlterRoutineScript(SavedRoutineAttribute final, SavedRoutineAttribute initial);

        string MakeDropRelationshipScript(Relationship relation);
        string MakeCreateRelationshipScript(Relationship relation);

        string MakeDropPrimaryKeyScript(PrimaryKey key);
        string MakeCreatePrimaryKeyScript(PrimaryKey key);

        string MakeDropIndexScript(Index key);
        string MakeCreateIndexScript(Index key);

        bool RelationshipChanged(Relationship finalRelation, Relationship initialRelation);
        bool IndexChanged(Index finalIndex, Index initialIndex);
    }
}
