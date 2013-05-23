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

namespace SqlSiphon
{
    public class UnifiedSetter
    {
        private MemberInfo member;
        public UnifiedSetter(MemberInfo member)
        {
            this.member = member;
        }

        private Type TypeToSet
        {
            get
            {
                return member.MemberType == MemberTypes.Field
                    ? ((FieldInfo)member).FieldType
                    : ((PropertyInfo)member).PropertyType;
            }
        }


        public void SetValue(object obj, object value)
        {
            try
            {
                if (value != DBNull.Value)
                {
                    if (TypeToSet.IsEnum)
                    {
                        if (value is string)
                            value = Enum.Parse(TypeToSet, (string)value);
                        else if (!TypeToSet.IsEnumDefined(value))
                            throw new ArgumentException(string.Format("\"{0}\" is not a valid value for the enumeration", value));
                    }

                    if (member.MemberType == MemberTypes.Field)
                        ((FieldInfo)member).SetValue(obj, value);
                    else
                        ((PropertyInfo)member).SetValue(obj, value, null);
                }
            }
            catch (Exception exp)
            {
                string message = string.Format("Could not set member {0}:{1}({2}) with value {3}({4}). Reason: {5}",
                    member.DeclaringType.FullName,
                    member.Name,
                    TypeToSet.Name,
                    value,
                    value != null ? value.GetType().Name : "N/A",
                    exp.Message);
                throw new Exception(message, exp);
            }
        }
        public string Name
        {
            get { return member.Name; }
        }
    }
}
