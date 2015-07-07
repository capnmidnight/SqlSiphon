using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon
{
    public class RelationshipExistsException : Exception
    {
        public RelationshipExistsException(string relationshipName)
            : base(string.Format("The foreign key `{0}` already exists.", relationshipName))
        {}
    }
}
