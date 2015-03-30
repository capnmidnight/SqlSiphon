using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon
{
    public interface IDatabaseStateWriter
    {
        void AlterDatabase(ScriptStatus script);
        void MarkScriptAsRan(ScriptStatus script);
    }
}
