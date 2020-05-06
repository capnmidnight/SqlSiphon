using System;
using SqlSiphon.Mapping;
using SqlSiphon.Model;

namespace SqlSiphon
{
    public interface IDatabaseScriptGenerator : IDatabaseObjectHandler
    {
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
    }
}
