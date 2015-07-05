using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon
{
    public interface IAssemblyStateReader
    {
        DatabaseState GetFinalState(Type dalType, string userName, string password);

        string ColumnChanged(ColumnAttribute final, ColumnAttribute initial);
        string RoutineChanged(RoutineAttribute final, RoutineAttribute initial);
        string KeyChanged(PrimaryKey final, PrimaryKey initial);
        string RelationshipChanged(Relationship finalRelation, Relationship initialRelation);
        string IndexChanged(Index finalIndex, Index initialIndex);
    }
}
