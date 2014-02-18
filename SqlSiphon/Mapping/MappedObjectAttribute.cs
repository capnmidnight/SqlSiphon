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
using System.Reflection;
using System.Linq;

namespace SqlSiphon.Mapping
{
    /// <summary>
    /// A base class for types that are mapped to the database.
    /// This attribute can be applied to classes, class properties,
    /// methods, method parameters, and enumerations. 
    /// 
    /// Only one attribute of a given type may be applied to
    /// any type of thing.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class
        | AttributeTargets.Method
        | AttributeTargets.Parameter
        | AttributeTargets.Property,
        Inherited = false,
        AllowMultiple = false)]
    public abstract class MappedObjectAttribute : Attribute
    {
        /// <summary>
        /// A property to override the default interpretation
        /// of the type's name. Usually, objects in the database
        /// are named after the type of thing that we are
        /// mapping, but in some cases we will want to override
        /// this behavior.
        /// </summary>
        public string Name { get; set; }
 
        /// <summary>
        /// A property to turn on or off the inclusion of the
        /// type as an object in the database. Defaults to
        /// "true", but can be overridden to keep from trying
        /// to process the thing. This can be useful for
        /// mapped classes that have extra attributes that we
        /// don't need in the database.
        /// </summary>
        public bool Include { get; set; }

        /// <summary>
        /// As this class is abstract, it can't be instantiated.
        /// </summary>
        public MappedObjectAttribute()
        {
            this.Include = true;
        }

        /// <summary>
        /// Retrieve an attribute of a certain type from an object.
        /// </summary>
        /// <typeparam name="T">The type of attribute to find</typeparam>
        /// <param name="obj">The object on which to find the attribute</param>
        /// <returns>The attribute instance, or null if no such
        /// attribute exists</returns>
        public static T GetAttribute<T>(ICustomAttributeProvider obj)
            where T : MappedObjectAttribute
        {
            var attr = obj
                .GetCustomAttributes(typeof(T), false)
                .Cast<T>()
                .FirstOrDefault();
            return attr;
        }

        /// <summary>
        /// Usually, objects in the database are named after the 
        /// type of thing that we are mapping, but in some cases 
        /// we will want to override this behavior. 
        /// </summary>
        /// <param name="name"></param>
        private void SetName(string name)
        {
            if (this.Name == null)
                this.Name = name;
        }

        /// <summary>
        /// A virtual method to analyze an object and figure out the
        /// default settings for it. The attribute can't find the thing
        /// its attached to on its own, so this can't be done in a
        /// constructor, we have to do it for it.
        /// </summary>
        /// <param name="obj">The object to InferProperties</param>
        internal virtual void InferProperties(ParameterInfo obj)
        {
            this.SetName(obj.Name);
        }

        /// <summary>
        /// A virtual method to analyze an object and figure out the
        /// default settings for it. The attribute can't find the thing
        /// its attached to on its own, so this can't be done in a
        /// constructor, we have to do it for it.
        /// </summary>
        /// <param name="obj">The object to InferProperties</param>
        internal virtual void InferProperties(MemberInfo obj)
        {
            this.SetName(obj.Name);
        }

        /// <summary>
        /// A virtual method to analyze an object and figure out the
        /// default settings for it. The attribute can't find the thing
        /// its attached to on its own, so this can't be done in a
        /// constructor, we have to do it for it.
        /// </summary>
        /// <param name="obj">The object to InferProperties</param>
        internal virtual void InferProperties(Type obj)
        {
            this.SetName(obj.Name);
        }
    }
}
