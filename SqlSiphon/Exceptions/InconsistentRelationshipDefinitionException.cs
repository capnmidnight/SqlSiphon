using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Exceptions
{
    public class InconsistentRelationshipDefinitionException : Exception
    {
        public InconsistentRelationshipDefinitionException(string relationshipName)
            : base(string.Format("The foreign key `{0}` has some columns that say it should auto generate an index, but others that do not.", relationshipName))
        {}
    }
}
