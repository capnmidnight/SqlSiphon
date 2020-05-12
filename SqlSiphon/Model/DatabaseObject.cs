namespace SqlSiphon.Model
{
    public abstract class DatabaseObject
    {
        /// <summary>
        /// Get a schema name for objects in the database. Defaults to
        /// null, which causes the data access system to use whatever is
        /// defined as the default value for the database vendor.
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// The name of the object as it should exist in the database.
        /// </summary>
        public string Name { get; }

        protected DatabaseObject(string schema, string name)
        {
            Schema = schema;
            Name = name;
        }
    }
}
