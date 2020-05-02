using System;

namespace SqlSiphon
{
    public class IndexExistsException : Exception
    {
        public IndexExistsException(string indexName, string attemptedTableName, string currentTableName)
            : base($"Cannot add index `{indexName}` to table `{attemptedTableName}`. It already exists on table `{currentTableName}`.")
        {
        }
    }
}
