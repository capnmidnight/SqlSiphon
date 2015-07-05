using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon
{
    public interface IDatabaseScriptGenerator : IDatabaseObjectHandler
    {
        string MakeRoutineIdentifier(RoutineAttribute routine);

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
