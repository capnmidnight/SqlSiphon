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

namespace SqlSiphon
{
    public static class TypeInfo
    {
        public static readonly Dictionary<Type, int> typeSizes = new Dictionary<Type, int>()
        {
            [typeof(bool)] = sizeof(bool),
            [typeof(bool?)] = sizeof(bool),
            [typeof(char)] = sizeof(char),
            [typeof(char?)] = sizeof(char),
            [typeof(sbyte)] = sizeof(sbyte),
            [typeof(sbyte?)] = sizeof(sbyte),
            [typeof(byte)] = sizeof(byte),
            [typeof(byte?)] = sizeof(byte),
            [typeof(short)] = sizeof(short),
            [typeof(short?)] = sizeof(short),
            [typeof(ushort)] = sizeof(ushort),
            [typeof(ushort?)] = sizeof(ushort),
            [typeof(int)] = sizeof(int),
            [typeof(int?)] = sizeof(int),
            [typeof(uint)] = sizeof(uint),
            [typeof(uint?)] = sizeof(uint),
            [typeof(long)] = sizeof(long),
            [typeof(long?)] = sizeof(long),
            [typeof(ulong)] = sizeof(ulong),
            [typeof(ulong?)] = sizeof(ulong),
            [typeof(decimal)] = sizeof(decimal),
            [typeof(decimal?)] = sizeof(decimal),
            [typeof(float)] = sizeof(float),
            [typeof(float?)] = sizeof(float),
            [typeof(double)] = sizeof(double),
            [typeof(double?)] = sizeof(double)
        };
    }
}