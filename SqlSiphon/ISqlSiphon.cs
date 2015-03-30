﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon
{
    public interface ISqlSiphon : IDataConnector, 
        IDatabaseStateReader, 
        IDatabaseStateWriter,
        IAssemblyStateReader, 
        IDatabaseScriptGenerator
    {
    }
}
