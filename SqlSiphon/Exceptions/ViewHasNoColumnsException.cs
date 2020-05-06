using System;

namespace SqlSiphon
{
    public class ViewHasNoColumnsException : Exception
    {
        public ViewHasNoColumnsException(Mapping.ViewAttribute view)
            : base($"The table `{view.Schema}`.`{view.Name}` defined by type `{view.SystemType.FullName}` has no properties that could be mapped to columns.")
        {
        }
    }
}
