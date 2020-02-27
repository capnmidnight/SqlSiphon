using System;

namespace InitDB
{
    public delegate void BrowseCommandPathEventHandler(object sender, BrowseCommandPathEventArgs args);

    public class BrowseCommandPathEventArgs : EventArgs
    {
        public string TypeName { get; }

        public string Path { get; }

        public BrowseCommandPathEventArgs(string typeName, string path)
        {
            TypeName = typeName;
            Path = path;
        }
    }
}
