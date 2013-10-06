/*
https://www.github.com/capnmidnight/SqlSiphon
Copyright (c) 2009, 2010, 2011, 2012, 2013 Sean T. McBeth
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
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class MappedParameterAttribute : MappedTypeAttribute
    {
        private bool directionNotSet = true;
        private ParameterDirection paramDirection;
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

        internal override void Study(ParameterInfo parameter)
        {
            base.Study(parameter);

            if (this.Name == null)
                this.Name = parameter.Name;

            if (this.DefaultValue == null && parameter.DefaultValue != DBNull.Value)
                this.DefaultValue = parameter.DefaultValue;

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

            if (this.optionalNotSet)
                this.IsOptional = parameter.IsOptional;
        }
    }
}
