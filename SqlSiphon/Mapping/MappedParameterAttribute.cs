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
using System.Reflection;

namespace SqlSiphon.Mapping
{
    /// <summary>
    /// An attribute to tag to parameters of a method to provide optional information
    /// about how the method parameter maps to a stored procedure parameter.
    /// 
    /// Only one attribute of a given type may be applied to
    /// any type of thing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class MappedParameterAttribute : MappedTypeAttribute
    {
        private bool directionNotSet = true;
        private ParameterDirection paramDirection;

        /// <summary>
        /// Gets or sets the stored procedure parameter's "direction", i.e.
        /// whether or not it's an input parameter, output parameter, or both.
        /// </summary>
        public ParameterDirection Direction
        {
            get
            {
                return this.paramDirection;
            }
            set
            {
                directionNotSet = false;
                this.paramDirection = value;
            }
        }

        public MappedParameterAttribute() {}
        
        /// <summary>
        /// A virtual method to analyze an object and figure out the
        /// default settings for it. The attribute can't find the thing
        /// its attached to on its own, so this can't be done in a
        /// constructor, we have to do it for it.
        /// </summary>
        /// <param name="obj">The object to InferProperties</param>
        internal override void InferProperties(ParameterInfo parameter)
        {
            base.InferProperties(parameter);

            // If the parameter direction was not set explicitly,
            // then infer it from the method parameter's direction.
            if (this.directionNotSet)
            {
                Direction = ParameterDirection.Input;
                if (parameter.IsIn && parameter.IsOut)
                    Direction = ParameterDirection.InputOutput;
                else if (parameter.IsOut)
                    Direction = ParameterDirection.Output;
                else if (parameter.IsRetval)
                    Direction = ParameterDirection.ReturnValue;
            }

            // Infer optionalness of the stored procedure's parameter
            // from whether or not the method's parameter is optional, 
            // but only if the IsOptional property of the attribute 
            // was not set explicitly.
            if (this.optionalNotSet)
                this.IsOptional = parameter.IsOptional;

            // Infer the default value for the stored procedure's 
            // parameter from the method parameter's default value,
            // but only if the DefaultValue property of the attribute
            // was not set to a specific value.
            if (this.DefaultValue == null && parameter.DefaultValue != DBNull.Value)
                this.DefaultValue = parameter.DefaultValue;
        }
    }
}
