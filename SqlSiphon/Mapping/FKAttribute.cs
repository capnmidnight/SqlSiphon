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
using SqlSiphon.Model;

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
            this.Target = target;
            this.AutoCreateIndex = true;
        }

        private void InferProperties(ColumnAttribute columnDef)
        {
            if (this.FromColumnName == null)
            {
                this.FromColumnName = columnDef.Name;
            }

            var targetTableDef = DatabaseObjectAttribute.GetAttribute<TableAttribute>(this.Target) ?? new TableAttribute();
            targetTableDef.InferProperties(this.Target);

            foreach (var targetColumnDef in targetTableDef.Properties)
            {
                if (columnDef.Name.ToLowerInvariant().EndsWith(targetColumnDef.Name.ToLowerInvariant()))
                {
                    if (this.ToColumnName == null)
                    {
                        this.ToColumnName = targetColumnDef.Name;
                    }

                    if (this.Prefix == null)
                    {
                        this.Prefix = columnDef.Name.Substring(0, columnDef.Name.Length - targetColumnDef.Name.Length);
                    }
                    break;
                }
            }

            if (this.Prefix == null)
            {
                this.Prefix = string.Empty;
            }
        }

        internal static List<Relationship> GetRelationships(Type t)
        {
            List<Relationship> fks = new List<Relationship>();
            var tableDef = DatabaseObjectAttribute.GetAttribute<TableAttribute>(t);
            if (tableDef != null)
            {
                tableDef.InferProperties(t);
                var fkOrganizer = new Dictionary<Type, Dictionary<string, List<string>>>();
                var autoCreateIndex = new Dictionary<Type, Dictionary<string, bool>>();
                foreach (var columnDef in tableDef.Properties)
                {
                    var fkDefs = columnDef.GetOtherAttributes<FKAttribute>();

                    foreach (var fkDef in fkDefs)
                    {
                        fkDef.InferProperties(columnDef);

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
                            throw new Exceptions.InconsistentRelationshipDefinitionException(fkDef.FromColumnName);
                        }

                        fkOrganizer[fkDef.Target][fkDef.Prefix].Add(fkDef.FromColumnName);
                    }
                }

                foreach (var targetType in fkOrganizer.Keys)
                {
                    foreach (var prefix in fkOrganizer[targetType].Keys)
                    {
                        var columns = fkOrganizer[targetType][prefix];
                        var r = new Relationship(prefix, t, targetType, autoCreateIndex[targetType][prefix], columns.ToArray());
                        r.Schema = tableDef.Schema;
                        fks.Add(r);
                    }
                }
            }

            return fks;
        }
    }
}
