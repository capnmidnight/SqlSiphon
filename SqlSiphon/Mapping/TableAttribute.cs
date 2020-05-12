using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SqlSiphon.Model;

namespace SqlSiphon.Mapping
{
    /// <summary>
    /// An attribute to use for tagging classes as being mapped to tables.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class
        | AttributeTargets.Enum,
        Inherited = false,
        AllowMultiple = false)]
    public class TableAttribute : DatabaseObjectAttribute
    {
        public PrimaryKey PrimaryKey { get; set; }
        public List<ColumnAttribute> Properties { get; private set; }
        public Dictionary<int, string> EnumValues { get; private set; }
        public Dictionary<string, TableIndex> Indexes { get; private set; }

        public TableAttribute()
        {
            Properties = new List<ColumnAttribute>();
            EnumValues = new Dictionary<int, string>();
            Indexes = new Dictionary<string, TableIndex>();
        }

        public TableAttribute(ISqlSiphon dal, Type t)
            : this()
        {
            InferProperties(dal, t);
        }

        public TableAttribute(
            InformationSchema.Column[] columns,
            ISqlSiphon dal)
            : this(columns, null, null, null, null, dal)
        {
        }

        public override Type SystemType
        {
            get
            {
                return base.SystemType ?? (Type)SourceObject;
            }
            protected set
            {
                base.SystemType = value;
            }
        }

        public TableAttribute(
            InformationSchema.Column[] columns,
            InformationSchema.TableConstraint[] constraints,
            InformationSchema.KeyColumnUsage[] keyColumns,
            InformationSchema.ConstraintColumnUsage[] constraintColumns,
            InformationSchema.IndexColumnUsage[] indexedColumns,
            ISqlSiphon dal)
            : this()
        {
            var testColumn = columns.First();
            Schema = testColumn.table_schema;
            Name = testColumn.table_name;
            Include = true;
            var columnConstraints = new Dictionary<string, List<string>>();
            if (keyColumns != null)
            {
                foreach (var c in keyColumns)
                {
                    var columnKey = dal.MakeIdentifier(c.column_name);
                    if (!columnConstraints.ContainsKey(columnKey))
                    {
                        columnConstraints.Add(columnKey, new List<string>());
                    }
                    columnConstraints[columnKey].Add(dal.MakeIdentifier(c.constraint_schema, c.constraint_name));
                }
            }
            else if (constraintColumns != null)
            {
                foreach (var c in constraintColumns)
                {
                    var columnKey = dal.MakeIdentifier(c.column_name);
                    if (!columnConstraints.ContainsKey(columnKey))
                    {
                        columnConstraints.Add(columnKey, new List<string>());
                    }
                    columnConstraints[columnKey].Add(dal.MakeIdentifier(c.constraint_schema, c.constraint_name));
                }
            }

            Dictionary<string, string> constraintTypes = null;

            if (constraints != null)
            {
                constraintTypes = constraints.ToDictionary(c => dal.MakeIdentifier(c.constraint_schema, c.constraint_name), c => c.constraint_type);
            }
            foreach (var column in columns)
            {
                var key = dal.MakeIdentifier(column.column_name);
                var isIncludedInPK = columnConstraints.ContainsKey(key)
                    && columnConstraints[key].Any(constraintName => constraintTypes != null && constraintTypes.ContainsKey(constraintName)
                        && constraintTypes[constraintName] == "PRIMARY KEY");
                Properties.Add(new ColumnAttribute(this, column, isIncludedInPK, dal));
            }

            if (indexedColumns != null)
            {
                foreach (var idxCol in indexedColumns)
                {
                    if (!Indexes.ContainsKey(idxCol.index_name))
                    {
                        Indexes.Add(idxCol.index_name, new TableIndex(this, idxCol.index_schema, idxCol.index_name));
                    }
                    Indexes[idxCol.index_name].Columns.Add(idxCol.column_name);
                }
            }
        }

        /// <summary>
        /// For a reflected method, determine the mapping parameters.
        /// Methods do not get mapped by default, so if the method
        /// doesn't have a RoutineAttribute, then none will be
        /// returned.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private RoutineAttribute GetMethodDescriptions(MethodInfo method)
        {
            var attr = RoutineAttribute.GetCommandDescription(method);
            if (attr == null || !attr.Include)
            {
                return null;
            }

            return attr;
        }

        /// <summary>
        /// A virtual method to analyze an object and figure out the
        /// default settings for it. The attribute can't find the thing
        /// its attached to on its own, so this can't be done in a
        /// constructor, we have to do it for it.
        /// 
        /// This method is not called from the DatabaseObjectAttribute.GetAttribute(s)
        /// methods because those methods aren't overloaded for different types
        /// of ICustomAttributeProvider types, but InferProperties is.
        /// </summary>
        /// <param name="obj">The object to InferProperties</param>
        /// 
        protected override void InferProperties(ISqlSiphon dal, Type obj)
        {
            base.InferProperties(dal, obj);
            if (obj.IsEnum)
            {
                var valueColumn = new EnumerationTableColumn(obj, this, "Value", typeof(int))
                {
                    IncludeInPrimaryKey = true
                };

                var descriptionColumn = new EnumerationTableColumn(obj, this, "Description", typeof(string));

                Properties.Add(valueColumn);
                Properties.Add(descriptionColumn);

                PrimaryKey = new PrimaryKey(dal, this);

                var names = obj.GetEnumNames();
                foreach (var name in names)
                {
                    EnumValues.Add((int)Enum.Parse(obj, name), name);
                }
            }
            else
            {
                var props = obj.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .OrderByDescending(p =>
                    {
                        var depth = 0;
                        var top = obj;
                        while (top != null && p.DeclaringType != top)
                        {
                            depth++;
                            top = top.BaseType;
                        }
                        return depth;
                    }).ToArray();
                var hasPK = false;
                foreach (var prop in props)
                {
                    var columnDescription = DatabaseObjectAttribute.GetColumn(prop) ?? new ColumnAttribute(prop);
                    if (columnDescription.Include)
                    {
                        columnDescription.Table = this;
                        Properties.Add(columnDescription);
                        if (columnDescription.IncludeInPrimaryKey)
                        {
                            hasPK = true;
                        }

                        var indexInclusions = columnDescription.GetOtherAttributes<IndexAttribute>();
                        foreach (var idxInc in indexInclusions)
                        {
                            if (!Indexes.ContainsKey(idxInc.Name))
                            {
                                Indexes.Add(idxInc.Name, new TableIndex(this, idxInc.Schema ?? Schema, idxInc.Name));
                            }
                            Indexes[idxInc.Name].Columns.Add(columnDescription.Name);
                        }
                    }
                }

                if (hasPK)
                {
                    PrimaryKey = new PrimaryKey(dal, this);
                }

                if (Properties.All(f => !f.Include))
                {
                    throw new TableHasNoColumnsException(this);
                }
            }
        }

        public List<Relationship> GetRelationships(ISqlSiphon dal)
        {
            var fks = new List<Relationship>();
            var fkOrganizer = new Dictionary<Type, Dictionary<string, List<string>>>();
            var autoCreateIndex = new Dictionary<Type, Dictionary<string, bool>>();
            foreach (var columnDef in Properties)
            {
                var fkDefs = columnDef.GetOtherAttributes<FKAttribute>();

                foreach (var fkDef in fkDefs)
                {
                    fkDef.InferProperties(dal, columnDef);

                    if (!fkOrganizer.ContainsKey(fkDef.Target))
                    {
                        fkOrganizer.Add(fkDef.Target, new Dictionary<string, List<string>>());
                        autoCreateIndex.Add(fkDef.Target, new Dictionary<string, bool>());
                    }

                    if (!fkOrganizer[fkDef.Target].ContainsKey(fkDef.Prefix))
                    {
                        fkOrganizer[fkDef.Target].Add(fkDef.Prefix, new List<string>());
                        autoCreateIndex[fkDef.Target].Add(fkDef.Prefix, fkDef.AutoCreateIndex);
                    }
                    else if (fkDef.AutoCreateIndex != autoCreateIndex[fkDef.Target][fkDef.Prefix])
                    {
                        throw new InconsistentRelationshipDefinitionException(fkDef.FromColumnName);
                    }

                    fkOrganizer[fkDef.Target][fkDef.Prefix].Add(fkDef.FromColumnName);
                }
            }

            foreach (var targetType in fkOrganizer.Keys)
            {
                foreach (var prefix in fkOrganizer[targetType].Keys)
                {
                    var columns = fkOrganizer[targetType][prefix];
                    var r = new Relationship(prefix, this, targetType, autoCreateIndex[targetType][prefix], columns.ToArray())
                    {
                        Schema = Schema
                    };
                    fks.Add(r);
                }
            }

            return fks;
        }

        public override string ToString()
        {
            return $"Table: {Schema}.{Name}";
        }
    }
}
