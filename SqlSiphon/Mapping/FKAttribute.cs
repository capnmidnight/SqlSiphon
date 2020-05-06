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
        public void InferProperties(ColumnAttribute columnDef)
        {
            if (FromColumnName == null)
            {
                FromColumnName = columnDef.Name;
            }

            var targetTableDef = DatabaseObjectAttribute.GetTable(Target) ?? new TableAttribute(Target);

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
