using System;

using SqlSiphon.Mapping;

namespace SqlSiphon
{
    [Table]
    public class ScriptStatus :
        BoundObject,
        IComparable<ScriptStatus>
    {
        [AutoPK]
        public int ScriptID { get; set; }

        public string Name { get; set; }

        [Column(DefaultValue = "0")]
        public ScriptType ScriptType { get; set; }

        [Column(DefaultValue = "getdate()")]
        public DateTime RanOn { get; set; }

        public string Script { get { return Get<string>(); } set { Set(value); } }

        [Exclude]
        public bool Run { get { return Get<bool>(); } set { Set(value); } }

        [Exclude]
        public string Reason { get; private set; }

        /// <summary>
        /// Default constructor enables automatic construction.
        /// </summary>
        public ScriptStatus() { }

        /// <summary>
        /// Creates a script with some basic metadata that can be displayed in InitDB
        /// </summary>
        /// <param name="type">Scripts can be ordered by type to ensure correct execution order</param>
        /// <param name="name">Usually the fully-qualified name of the object being modified, plus the script type, used as
        /// user-friendly description of the script.</param>
        /// <param name="script">The thing that does the work.</param>
        public ScriptStatus(ScriptType type, string name, string script, string reason)
        {
            ScriptType = type;
            Name = name;
            Script = script;
            Reason = reason;
        }

        public int CompareTo(ScriptStatus obj)
        {
            var result = ScriptType.CompareTo(obj.ScriptType);
            if (result == 0)
            {
                result = string.Compare(Name, obj.Name, StringComparison.InvariantCultureIgnoreCase);
            }
            return result;
        }
    }
}
