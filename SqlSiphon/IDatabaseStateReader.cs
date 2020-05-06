using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using SqlSiphon.Mapping;

namespace SqlSiphon
{
    public interface IDatabaseStateReader : IDatabaseObjectHandler
    {
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
        List<InformationSchema.Columns> GetColumns();
        List<InformationSchema.Columns> GetUDTTColumns();
        List<InformationSchema.IndexColumnUsage> GetIndexColumns();
        List<InformationSchema.Routines> GetRoutines();
        List<InformationSchema.Parameters> GetParameters();
        List<InformationSchema.KeyColumnUsage> GetKeyColumns();
        List<InformationSchema.ConstraintColumnUsage> GetConstraintColumns();
        List<InformationSchema.TableConstraints> GetTableConstraints();
        List<InformationSchema.ReferentialConstraints> GetReferentialConstraints();
    }
}
