using System;

namespace SqlSiphon
{
    public class IndexExistsException : Exception
    {
        public IndexExistsException(string indexName, string attemptedTableName, string currentTableName)
            : base(string.Format("Cannot add index `{0}` to table `{1}`. It already exists on table `{2}`.", indexName, attemptedTableName, currentTableName))
        {
        }
    }
}
