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
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public class FKAttribute : DatabaseObjectAttribute
    {
        public Type Target { get; private set; }

        public string Prefix { get; private set; }

        private string columnName;

        public FKAttribute(Type target) : this(target, null) { }

        public FKAttribute(Type target, string columnName)
        {
            this.Target = target;
            this.columnName = columnName;
        }

        internal static List<Relationship> GetRelationships(Type t)
        {
            List<Relationship> fks = new List<Relationship>();
            var tableDef = DatabaseObjectAttribute.GetAttribute<TableAttribute>(t);
            var fkOrganizer = new Dictionary<Type, Dictionary<string, List<string>>>();
            var properties = t.GetProperties();
            foreach (var property in properties)
            {
                var columnDef = DatabaseObjectAttribute.GetAttribute<ColumnAttribute>(property) ?? new ColumnAttribute();
                columnDef.InferProperties(property);

                var includeFKAttrs = DatabaseObjectAttribute.GetAttributes<FKAttribute>(property);

                foreach (var attr in includeFKAttrs)
                {
                    if (!fkOrganizer.ContainsKey(attr.Target))
                    {
                        fkOrganizer.Add(attr.Target, new Dictionary<string, List<string>>());
                    }

                    var target = fkOrganizer[attr.Target];

                    InferPrefix(columnDef, attr);
                    var pre = attr.Prefix ?? "";

                    if (!target.ContainsKey(pre))
                    {
                        target.Add(pre, new List<string>());
                    }

                    target[pre].Add(attr.columnName);
                }
            }

            foreach (var targetType in fkOrganizer.Keys)
            {
                foreach (var prefix in fkOrganizer[targetType].Keys)
                {
                    var columns = fkOrganizer[targetType][prefix];
                    var r = new Relationship(prefix, t, targetType, columns.ToArray());
                    r.Schema = tableDef.Schema;
                    fks.Add(r);
                }
            }

            return fks;
        }

        private static void InferPrefix(ColumnAttribute columnDef, FKAttribute attr)
        {
            if (attr.Prefix == null)
            {
                var targetProperties = attr.Target
                    .GetProperties()
                    .OrderBy(p => p.Name.Length)
                    .ToList();
                foreach (var targetProperty in targetProperties)
                {
                    var targetColumnDef = DatabaseObjectAttribute.GetAttribute<ColumnAttribute>(targetProperty);
                    targetColumnDef.InferProperties(targetProperty);
                    if (columnDef.Name.EndsWith(targetColumnDef.Name))
                    {
                        if (columnDef.Name.Length > targetColumnDef.Name.Length)
                        {
                            attr.Prefix = columnDef.Name.Substring(0, columnDef.Name.Length - targetColumnDef.Name.Length);
                        }
                        break;
                    }
                }
            }
        }
    }
}
