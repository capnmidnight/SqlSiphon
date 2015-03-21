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
        DatabaseState GetFinalState(string userName, string password);
        void AlterDatabase(ScriptStatus script);
        void MarkScriptAsRan(ScriptStatus script);

        event DataProgressEventHandler Progress;

        string DefaultSchemaName { get; }

        List<string> GetSchemata();
        List<string> GetDatabaseLogins();
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
        bool RoutineChanged(RoutineAttribute final, RoutineAttribute initial);
        bool KeyChanged(PrimaryKey final, PrimaryKey initial);

        bool RelationshipChanged(Relationship finalRelation, Relationship initialRelation);
        bool IndexChanged(Index finalIndex, Index initialIndex);

        void AnalyzeQuery(string routineText, RoutineAttribute routine);

        RoutineAttribute GetCommandDescription(System.Reflection.MethodInfo method);

        Type GetSystemType(string sqlType);

        string MakeCreateDatabaseLoginScript(string userName, string password, string database);
        string MakeCreateCatalogueScript(string catalogueName);

        string MakeCreateSchemaScript(string schemaName);
        string MakeDropSchemaScript(string schemaName);

        string MakeCreateTableScript(TableAttribute table);
        string MakeDropTableScript(TableAttribute table);

        string MakeCreateColumnScript(ColumnAttribute column);
        string MakeDropColumnScript(ColumnAttribute column);
        string MakeAlterColumnScript(ColumnAttribute final, ColumnAttribute initial);

        string MakeDropRoutineScript(RoutineAttribute routine);
        string MakeCreateRoutineScript(RoutineAttribute routine);
        
        string MakeDropRelationshipScript(Relationship relation);
        string MakeCreateRelationshipScript(Relationship relation);

        string MakeDropPrimaryKeyScript(PrimaryKey key);
        string MakeCreatePrimaryKeyScript(PrimaryKey key);

        string MakeDropIndexScript(Index key);
        string MakeCreateIndexScript(Index key);
    }
}
