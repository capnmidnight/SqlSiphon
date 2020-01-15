using System;

namespace SqlSiphon
{
    public class InconsistentRelationshipDefinitionException : Exception
    {
        public InconsistentRelationshipDefinitionException(string relationshipName)
            : base(string.Format("The foreign key `{0}` has some columns that say it should auto generate an index, but others that do not.", relationshipName))
        { }
    }
}
