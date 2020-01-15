using System;

namespace SqlSiphon
{
    public class RelationshipExistsException : Exception
    {
        public RelationshipExistsException(string relationshipName)
            : base(string.Format("The foreign key `{0}` already exists.", relationshipName))
        { }
    }
}
