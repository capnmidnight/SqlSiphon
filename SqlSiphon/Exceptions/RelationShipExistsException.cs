using System;

namespace SqlSiphon
{
    public class RelationshipExistsException : Exception
    {
        public RelationshipExistsException(string relationshipName)
            : base($"The foreign key `{relationshipName}` already exists.")
        { }
    }
}
