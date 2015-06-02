using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SqlSiphon.Mapping;

namespace SqlSiphon
{
    public interface IDatabaseStateReader : IDatabaseObjectHandler
    {
        DatabaseState GetInitialState(string catalogueName, Regex filterObjects);

        bool DescribesIdentity(InformationSchema.Columns column);

        void AnalyzeQuery(string routineText, RoutineAttribute routine);

        int DefaultTypePrecision(string typeName, int testPrecision);
        Type GetSystemType(string sqlType);

        List<string> GetSchemata();
        List<string> GetScriptStatus();
        List<string> GetDatabaseLogins();
        List<InformationSchema.Columns> GetColumns();
        List<InformationSchema.IndexColumnUsage> GetIndexColumns();
        List<InformationSchema.Routines> GetRoutines();
        List<InformationSchema.Parameters> GetParameters();
        List<InformationSchema.KeyColumnUsage> GetKeyColumns();
        List<InformationSchema.ConstraintColumnUsage> GetConstraintColumns();
        List<InformationSchema.TableConstraints> GetTableConstraints();
        List<InformationSchema.ReferentialConstraints> GetReferentialConstraints();
    }
}
