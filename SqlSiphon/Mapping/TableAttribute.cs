/*
https://www.github.com/capnmidnight/SqlSiphon
Copyright (c) 2009 - 2014 Sean T. McBeth
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this 
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or 
  other materials provided with the distribution.

* Neither the name of McBeth Software Systems nor the names of its contributors
  may be used to endorse or promote products derived from this software without 
  specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE 
OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED 
OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        public Dictionary<string, Index> Indexes { get; private set; }

        public TableAttribute()
        {
            this.Properties = new List<ColumnAttribute>();
            this.EnumValues = new Dictionary<int, string>();
            this.Indexes = new Dictionary<string, Index>();
        }

        public TableAttribute(
            InformationSchema.Columns[] columns,
            IDatabaseStateReader dal)
            : this(columns, null, null, null, null, dal)
        {
        }

        public TableAttribute(
            InformationSchema.Columns[] columns,
            InformationSchema.TableConstraints[] constraints,
            InformationSchema.KeyColumnUsage[] keyColumns,
            InformationSchema.ConstraintColumnUsage[] constraintColumns,
            InformationSchema.IndexColumnUsage[] indexedColumns,
            IDatabaseStateReader dal)
            : this()
        {
            var testColumn = columns.First();
            this.Schema = testColumn.table_schema;
            this.Name = testColumn.table_name;
            this.Include = true;
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
                this.Properties.Add(new ColumnAttribute(this, column, isIncludedInPK, dal));
            }

            if (indexedColumns != null)
            {
                foreach (var idxCol in indexedColumns)
                {
                    if (!this.Indexes.ContainsKey(idxCol.index_name))
                    {
                        this.Indexes.Add(idxCol.index_name, new Index(this, idxCol.index_name));
                    }
                    this.Indexes[idxCol.index_name].Columns.Add(idxCol.column_name);
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
            var attr = GetAttribute<RoutineAttribute>(method);
            if (attr == null || !attr.Include)
                return null;
            attr.InferProperties(method);
            return attr;
        }

        /// <summary>
        /// A virtual method to analyze an object and figure out the
        /// default settings for it. The attribute can't find the thing
        /// its attached to on its own, so this can't be done in a
        /// constructor, we have to do it for it.
        /// </summary>
        /// <param name="obj">The object to InferProperties</param>
        /// 
        public override void InferProperties(Type obj)
        {
            base.InferProperties(obj);
            if (obj.IsEnum)
            {
                this.Properties.Add(new ColumnAttribute
                {
                    Table = this,
                    IncludeInPrimaryKey = true,
                    Name = "Value",
                    SqlType = "int"
                });

                this.Properties.Add(new ColumnAttribute
                {
                    Table = this,
                    Name = "Description",
                    SqlType = "nvarchar(max)"
                });

                var names = obj.GetEnumNames();
                foreach (var name in names)
                    EnumValues.Add((int)Enum.Parse(obj, name), name);
            }
            else
            {
                var props = obj.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                for (var i = 0; i < props.Length; ++i)
                {
                    var columnDescription = GetAttribute<ColumnAttribute>(props[i]) ?? new ColumnAttribute();
                    columnDescription.InferProperties(this, props[i]);
                    if (columnDescription.Include)
                    {
                        this.Properties.Add(columnDescription);
                    }

                    var indexInclusions = GetAttributes<IncludeInIndexAttribute>(props[i]);
                    foreach (var idxInc in indexInclusions)
                    {
                        if (!this.Indexes.ContainsKey(idxInc.Name))
                        {
                            this.Indexes.Add(idxInc.Name, new Index(this, idxInc.Name));
                        }
                        this.Indexes[idxInc.Name].Columns.Add(columnDescription.Name);
                    }
                }
            }
        }

        public override string ToString()
        {
            return "TABLE " + base.ToString();
        }
    }
}
