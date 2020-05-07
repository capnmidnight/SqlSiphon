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
    /// An attribute to use for tagging classes as being mapped to views.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class,
        Inherited = false,
        AllowMultiple = false)]
    public class ViewAttribute : DatabaseObjectAttribute
    {
        public List<ColumnAttribute> Properties { get; private set; }

        public string Query { get; private set; }

        public ViewAttribute(string query)
        {
            Properties = new List<ColumnAttribute>();
            Query = query;
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

        public ViewAttribute(
            string query,
            InformationSchema.Columns[] columns,
            ISqlSiphon dal)
            : this(query)
        {
            var testColumn = columns.First();
            Schema = testColumn.table_schema;
            Name = testColumn.table_name;
            Include = true;

            foreach (var column in columns)
            {
                Properties.Add(new ColumnAttribute(this, column, dal));
            }
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
        protected override void InferProperties(Type obj)
        {
            base.InferProperties(obj);
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

            foreach (var prop in props)
            {
                var columnDescription = DatabaseObjectAttribute.GetAttribute(prop) ?? new ColumnAttribute(prop);
                if (columnDescription.Include)
                {
                    columnDescription.IsOptional = true;
                    columnDescription.View = this;
                    Properties.Add(columnDescription);
                }
            }

            if (Properties.All(f => !f.Include))
            {
                throw new ViewHasNoColumnsException(this);
            }
        }

        public override string ToString()
        {
            return $"View: {Schema}.{Name}";
        }
    }
}
