﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon
{
    public interface IDatabaseStateWriter
    {
        void AlterDatabase(ScriptStatus script);
        void MarkScriptAsRan(ScriptStatus script);
        bool RunCommandLine(string executablePath, string configurationPath, string server, string database, string adminUser, string adminPass, string query);
    }
}