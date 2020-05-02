using System;

namespace SqlSiphon
{
    public class InconsistentRelationshipDefinitionException : Exception
    {
        public InconsistentRelationshipDefinitionException(string relationshipName)
            : base($"The foreign key `{relationshipName}` has some columns that say it should auto generate an index, but others that do not.")
        { }
    }
}
