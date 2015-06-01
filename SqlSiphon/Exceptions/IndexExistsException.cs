using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Exceptions
{
    public class IndexExistsException : Exception
    {
        public IndexExistsException(string indexName, string attemptedTableName, string currentTableName)
            : base(string.Format("Cannot add index `{0}` to table `{1}`. It already exists on table `{2}`.", indexName, attemptedTableName, currentTableName))
        {
        }
    }
}
