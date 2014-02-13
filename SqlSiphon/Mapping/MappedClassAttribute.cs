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
        Inherited = true,
        AllowMultiple = false)]
    public class MappedClassAttribute : MappedSchemaObjectAttribute
    {
        public List<MappedMethodAttribute> Methods { get; private set; }
        public List<MappedPropertyAttribute> Properties { get; private set; }

        public MappedClassAttribute()
        {
            this.Methods = new List<MappedMethodAttribute>();
            this.Properties = new List<MappedPropertyAttribute>();
        }

        /// <summary>
        /// For a reflected method, determine the mapping parameters.
        /// Methods do not get mapped by default, so if the method
        /// doesn't have a MappedMethodAttribute, then none will be
        /// returned.
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        private MappedMethodAttribute GetMethodDescriptions(MethodInfo method)
        {
            var attr = GetAttribute<MappedMethodAttribute>(method);
            if (attr == null || !attr.Include)
                return null;
            attr.InferProperties(method);
            return attr;
        }

        /// <summary>
        /// For a reflected property, determine the mapping parameters.
        /// Properties get mapped by default, so if the property does
        /// not have a MappedPropertyAttribute, then one is generated
        /// for it and some settings are inferred.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        private MappedPropertyAttribute GetPropertyDescriptions(PropertyInfo prop)
        {
            var attr = GetAttribute<MappedPropertyAttribute>(prop)
                ?? new MappedPropertyAttribute();
            attr.InferProperties(prop);
            return attr;
        }

        private static BindingFlags PATTERN = BindingFlags.Public | BindingFlags.Instance;
        /// <summary>
        /// A virtual method to analyze an object and figure out the
        /// default settings for it. The attribute can't find the thing
        /// its attached to on its own, so this can't be done in a
        /// constructor, we have to do it for it.
        /// </summary>
        /// <param name="obj">The object to InferProperties</param>
        /// 
        internal override void InferProperties(Type obj)
        {
            base.InferProperties(obj);

            this.Methods.AddRange(
                obj.GetMethods(PATTERN)
                .Select(this.GetMethodDescriptions)
                .Where(m => m != null));

            this.Properties.AddRange(
                obj.GetProperties(PATTERN)
                .Select(this.GetPropertyDescriptions)
                .Where(p => p.Include));
        }
    }
}
