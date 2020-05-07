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