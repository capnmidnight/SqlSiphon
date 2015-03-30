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

        bool ColumnChanged(ColumnAttribute final, ColumnAttribute initial);
        bool RoutineChanged(RoutineAttribute final, RoutineAttribute initial);
        bool KeyChanged(PrimaryKey final, PrimaryKey initial);

        bool RelationshipChanged(Relationship finalRelation, Relationship initialRelation);
        bool IndexChanged(Index finalIndex, Index initialIndex);
    }
}
