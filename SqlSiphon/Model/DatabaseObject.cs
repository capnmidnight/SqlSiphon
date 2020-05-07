using System;
using System.Reflection;

namespace SqlSiphon.Model
{
    /// <summary>
    /// A base class for types that are mapped to the database.
    /// </summary>
    public abstract class DatabaseObject
    {
        protected ICustomAttributeProvider SourceObject { get; set; }

        /// <summary>
        /// Get or set a schema name for objects in the database. Defaults to
        /// null, which causes the data access system to use whatever is
        /// defined as the default value for the database vendor.
        /// </summary>
        public virtual string Schema { get; set; }

        /// <summary>
        /// A property to override the default interpretation
        /// of the type's name. Usually, objects in the database
        /// are named after the type of thing that we are
        /// mapping, but in some cases we will want to override
        /// this behavior.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Usually, objects in the database are named after the 
        /// type of thing that we are mapping, but in some cases 
        /// we will want to override this behavior. 
        /// </summary>
        /// <param name="name"></param>
        private void SetName(string name)
        {
            if (Name == null)
            {
                Name = name;
            }
        }

        /// <summary>
        /// A virtual method to analyze an object and figure out the
        /// default settings for it. The attribute can't find the thing
        /// its attached to on its own, so this can't be done in a
        /// constructor, we have to do it for it.
        /// </summary>
        /// <param name="obj">The object to InferProperties</param>
        protected virtual void InferProperties(MethodInfo obj)
        {
            SourceObject = obj;
            SetName(obj.Name);
        }

        /// <summary>
        /// A virtual method to analyze an object and figure out the
        /// default settings for it. The attribute can't find the thing
        /// its attached to on its own, so this can't be done in a
        /// constructor, we have to do it for it.
        /// </summary>
        /// <param name="obj">The object to InferProperties</param>
        protected virtual void InferProperties(PropertyInfo obj)
        {
            SourceObject = obj;
            SetName(obj.Name);
        }

        /// <summary>
        /// A virtual method to analyze an object and figure out the
        /// default settings for it. The attribute can't find the thing
        /// its attached to on its own, so this can't be done in a
        /// constructor, we have to do it for it.
        /// </summary>
        /// <param name="obj">The object to InferProperties</param>
        protected virtual void InferProperties(ParameterInfo obj)
        {
            SourceObject = obj;
            SetName(obj.Name);
        }

        /// <summary>
        /// A virtual method to analyze an object and figure out the
        /// default settings for it. The attribute can't find the thing
        /// its attached to on its own, so this can't be done in a
        /// constructor, we have to do it for it.
        /// </summary>
        /// <param name="obj">The object to InferProperties</param>
        protected virtual void InferProperties(Type obj)
        {
            SourceObject = obj;
            SetName(obj.Name);
        }

        public override string ToString()
        {
            return $"[{Schema}].[{Name}]";
        }
    }
}
