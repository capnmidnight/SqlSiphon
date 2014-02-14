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
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlSiphon.Mapping
{
    /// <summary>
    /// An attribute to use for tagging methods as being mapped to a stored procedure call.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class MappedMethodAttribute : MappedTypeAttribute
    {
        /// <summary>
        /// The number of milliseconds before ADO.NET gives up waiting on the command.
        /// Defaults to -1, which specifies the default timeout for the database
        /// driver used by ADO.NET for the connection.
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// The way that the command should be used. Either as a StoredProcedure
        /// or as a Text query. Defaults to StoredProcedure.
        /// </summary>
        public CommandType CommandType { get; set; }

        /// <summary>
        /// The script of the command. If this method should be mapped as a stored
        /// procedure, then it is used as part of a CREATE PROCEDURE script that
        /// is ran during a synchronization process. If the method is specified
        /// as a text query, then the query is ran directly.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// When set to true, instructs SqlSiphon to automatically wrap every
        /// query with a transaction. Defaults to false.
        /// </summary>
        public bool EnableTransaction { get; set; }

        /// <summary>
        /// A description of all the parameters that are part of the method.
        /// </summary>
        public List<MappedParameterAttribute> Parameters { get; private set; }

        private MethodInfo originalMethod;

        /// <summary>
        /// Default constructor to set default values;
        /// </summary>
        public MappedMethodAttribute()
        {
            this.Parameters = new List<MappedParameterAttribute>();
            this.Timeout = -1;
            this.CommandType = CommandType.StoredProcedure;
            this.EnableTransaction = false;
        }

        /// <summary>
        /// Finds the MappedParameterAttribute for the giving ParameterInfo,
        /// or makes one up if it can't find one. All parameters in a mapped
        /// method will eventually have a MappedParameterAttribute represent it.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private MappedParameterAttribute ToMappedParameter(ParameterInfo parameter)
        {
            var attr = GetAttribute<MappedParameterAttribute>(parameter)
                ?? new MappedParameterAttribute();
            attr.InferProperties(parameter);
            return attr;
        }

        /// <summary>
        /// A virtual method to analyze an method and figure out the
        /// parameters and default settings for it. The attribute can't
        /// find the thing its attached to on its own, so this can't 
        /// be done in a constructor, we have to do it for it.
        /// </summary>
        /// <param name="obj">The method to InferProperties</param>
        public override void InferProperties(MethodInfo obj)
        {
            base.InferProperties(obj);
            this.originalMethod = obj;
            this.Parameters.AddRange(
                obj.GetParameters()
                .Select(this.ToMappedParameter));
        }
    }
}
