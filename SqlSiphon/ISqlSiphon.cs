using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using SqlSiphon.Mapping;
using SqlSiphon.Model;

namespace SqlSiphon
{
    public interface ISqlSiphon : IDataConnector
    {
        string DefaultSchemaName { get; }
        string MakeIdentifier(params string[] parts);
        DatabaseState GetFinalState(Type dalType, string userName, string password, string database);

        string ColumnChanged(ColumnAttribute final, ColumnAttribute initial);
        string RoutineChanged(RoutineAttribute final, RoutineAttribute initial);
        string KeyChanged(PrimaryKey final, PrimaryKey initial);
        string RelationshipChanged(Relationship finalRelation, Relationship initialRelation);
        string IndexChanged(TableIndex finalIndex, TableIndex initialIndex);
        DatabaseState GetInitialState(string catalogueName, Regex filterObjects);

        bool DescribesIdentity(InformationSchema.Columns column);

        void AnalyzeQuery(string routineText, RoutineAttribute routine);

        int GetDefaultTypePrecision(string typeName, int testPrecision);

        bool HasDefaultTypeSize(Type type);
        int GetDefaultTypeSize(Type type);
        Type GetSystemType(string sqlType);
        string NormalizeSqlType(string sqlType);

        List<string> GetSchemata();
        List<string> GetScriptStatus();
        List<string> GetDatabaseLogins();
        List<InformationSchema.Columns> GetTableColumns();
        List<InformationSchema.Columns> GetUDTTColumns();
        List<InformationSchema.Views> GetViews();
        List<InformationSchema.Columns> GetViewColumns();
        List<InformationSchema.IndexColumnUsage> GetIndexColumns();
        List<InformationSchema.Routines> GetRoutines();
        List<InformationSchema.Parameters> GetParameters();
        List<InformationSchema.KeyColumnUsage> GetKeyColumns();
        List<InformationSchema.ConstraintColumnUsage> GetConstraintColumns();
        List<InformationSchema.TableConstraints> GetTableConstraints();
        List<InformationSchema.ReferentialConstraints> GetReferentialConstraints();
        bool SupportsScriptType(ScriptType type);

        bool IsUDTT(Type systemType);

        string MakeInsertScript(TableAttribute table, object value);

        string MakeRoutineIdentifier(RoutineAttribute routine, bool withParamNames);

        string MakeCreateDatabaseLoginScript(string userName, string password, string database);
        string MakeCreateCatalogueScript(string catalogueName);

        string MakeCreateSchemaScript(string schemaName);
        string MakeDropSchemaScript(string schemaName);

        string MakeCreateTableScript(TableAttribute table);
        string MakeDropTableScript(TableAttribute table);

        TableAttribute MakeUDTTTableAttribute(Type type);
        string MakeCreateUDTTScript(TableAttribute info);
        string MakeDropUDTTScript(TableAttribute info);

        string MakeCreateViewScript(ViewAttribute info);
        string MakeDropViewScript(ViewAttribute info);

        string MakeCreateColumnScript(ColumnAttribute column);
        string MakeDropColumnScript(ColumnAttribute column);
        string MakeAlterColumnScript(ColumnAttribute final, ColumnAttribute initial);

        string MakeDropRoutineScript(RoutineAttribute routine);
        string MakeRoutineBody(RoutineAttribute routine);
        string MakeCreateRoutineScript(RoutineAttribute routine, bool buildBody = true);

        string MakeDropRelationshipScript(Relationship relation);
        string MakeCreateRelationshipScript(Relationship relation);

        string MakeDropPrimaryKeyScript(PrimaryKey key);
        string MakeCreatePrimaryKeyScript(PrimaryKey key);

        string MakeDropIndexScript(TableIndex key);
        string MakeCreateIndexScript(TableIndex key);
        void AlterDatabase(ScriptStatus script);
        void MarkScriptAsRan(ScriptStatus script);
        bool RunCommandLine(string executablePath, string configurationPath, string server, string database, string adminUser, string adminPass, string query);

        event IOEventHandler OnStandardOutput;
        event IOEventHandler OnStandardError;
    }
}
