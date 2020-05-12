using System;

namespace SqlSiphon.Mapping
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class FKAttribute : DatabaseObjectAttribute
    {
        public Type Target { get; private set; }

        public string Prefix { get; set; }

        public bool AutoCreateIndex { get; set; }

        public string FromColumnName { get; set; }

        public string ToColumnName { get; set; }

        public FKAttribute(Type target)
        {
            Target = target;
            AutoCreateIndex = true;
        }

        /// <summary>
        /// 
        /// 
        /// This method is not called from the DatabaseObjectAttribute.GetAttribute(s)
        /// methods because those methods aren't overloaded for different types
        /// of ICustomAttributeProvider types, but InferProperties is.
        /// </summary>
        /// <param name="columnDef"></param>
        public void InferProperties(ISqlSiphon dal, ColumnAttribute columnDef)
        {
            if (FromColumnName == null)
            {
                FromColumnName = columnDef.Name;
            }

            var targetTableDef = DatabaseObjectAttribute.GetTable(dal, Target) ?? new TableAttribute(dal, Target);

            foreach (var targetColumnDef in targetTableDef.Properties)
            {
                if (columnDef.Name.EndsWith(targetColumnDef.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (ToColumnName == null)
                    {
                        ToColumnName = targetColumnDef.Name;
                    }

                    if (Prefix == null)
                    {
                        Prefix = columnDef.Name.Substring(0, columnDef.Name.Length - targetColumnDef.Name.Length);
                    }
                    break;
                }
            }

            if (Prefix == null)
            {
                Prefix = string.Empty;
            }
        }
    }
}
