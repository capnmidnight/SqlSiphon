using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSiphon.Mapping;

namespace SqlSiphon
{
    public enum ScriptType
    {
        None,
        CreateCatalogue,
        CreateDatabaseLogin,
        CreateSchema,
        InstallExtension,
        CreateTable,
        CreateColumn,
        TranslateData,
        DropRoutine,
        DropRelationship,
        DropPrimaryKey,
        AlterColumn,
        DropColumn,
        DropTable,
        DropSchema,
        CreatePrimaryKey,
        CreateRelationship,
        CreateRoutine,
        DropIndex,
        CreateIndex,
        InitializeData
    }

    [Table]
    public class ScriptStatus : BoundObject
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
        public ScriptStatus(ScriptType type, string name, string script)
        {
            this.ScriptType = type;
            this.Name = name;
            this.Script = script;
        }
    }
}
