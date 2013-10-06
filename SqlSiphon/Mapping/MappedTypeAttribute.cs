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
using System.Reflection;

namespace SqlSiphon.Mapping
{
    [AttributeUsage(
        AttributeTargets.Parameter
        | AttributeTargets.Property
        | AttributeTargets.Method
        | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class MappedTypeAttribute : MappedSchemaObjectAttribute
    {
        public Type SystemType { get; protected set; }

        public bool IsCollection
        {
            get
            {
                return this.SystemType
                    .FindInterfaces(new TypeFilter((t, o) =>
                        t == typeof(System.Collections.IEnumerable)), null)
                    .Length > 0;
            }
        }

        public string SqlType { get; set; }

        public bool IsSizeSet { get; private set; }
        private int typeSize;
        public int Size
        {
            get { return this.typeSize; }
            set
            {
                this.IsSizeSet = true;
                this.typeSize = value;
            }
        }

        public bool IsPrecisionSet { get; private set; }
        private int typePrecision;
        public int Precision
        {
            get { return this.typePrecision; }
            set
            {
                this.IsPrecisionSet = true;
                this.typePrecision = value;
            }
        }

        public object DefaultValue { get; set; }

        protected bool optionalNotSet = true;
        private bool isOptionalField = false;
        public bool IsOptional
        {
            get { return isOptionalField; }
            set
            {
                optionalNotSet = false;
                isOptionalField = value;
            }
        }

        public MappedTypeAttribute() { }

        internal override void Study(System.Reflection.ParameterInfo parameter)
        {
            base.Study(parameter);
            if (this.SystemType == null)
                this.SystemType = parameter.ParameterType;
        }

        public string ToString(string openSize = "(", string closeSize = ")", string before = "", string after = "")
        {
            string output = "";
            var format = this.IsSizeSet
                ? this.IsPrecisionSet
                    ? "{0} {1} {2}{3}, {4}{5} {6}"
                    : "{0} {1} {2}{3}{5} {6}"
                : "{0} {1} {6}";

            output = string.Format(
                "{0} {1} {2}{3}, {4}{5} {6}",
                before,
                this.SqlType,
                openSize,
                this.Size,
                this.Precision,
                closeSize,
                after);

            return output.Trim();
        }
    }
}
