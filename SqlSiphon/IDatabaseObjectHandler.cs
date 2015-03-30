using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSiphon
{
    public interface IDatabaseObjectHandler
    {
        string DefaultSchemaName { get; }
        string MakeIdentifier(params string[] parts);
    }
}
