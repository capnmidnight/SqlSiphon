using System.Linq;
using System.IO;
using SqlSiphon;
using System.Data;
using System.Data.Common;

namespace InitDB
{
    class Session : SqlSiphon.BoundObject
    {
        public string Name { get { return Get<string>(); } set { Set(value); } }
        public string Server { get { return Get<string>(); } set { Set(value); } }
        public string DBName { get { return Get<string>(); } set { Set(value); } }
        public string AdminName { get { return Get<string>(); } set { Set(value); } }
        public string AdminPassword { get { return Get<string>(); } set { Set(value); } }
        public string LoginName { get { return Get<string>(); } set { Set(value); } }
        public string LoginPassword { get { return Get<string>(); } set { Set(value); } }
        public string AssemblyFile { get { return Get<string>(); } set { Set(value); } }
        public string ObjectFilter { get { return Get<string>(); } set { Set(value); } }
        public int DatabaseTypeIndex
        {
            get
            {
                var str = Get<string>();
                int index = 0;
                int.TryParse(str, out index);
                return index;
            }
            set { Set(value.ToString()); }
        }
        public ScriptType[] ScriptTypes { get { return Get<ScriptType[]>(); } set { Set(value); } }

        public static char PAIR_SEPARATOR = ';';
        public static char KEY_VALUE_SEPARATOR = ':';
        public static char ARRAY_VALUE_SEPARATOR = ',';

        public Session()
            : this(InitDB.MainForm.DEFAULT_SESSION_NAME, "localhost\\SQLEXPRESS", "", "", "", "", "", "", "", 0, new ScriptType[] { })
        {
        }

        public Session(string name, string server, string dbname, string adminName, string adminPassword,
            string loginName, string loginPassword, string assemblyFile, string objectFilter, int databaseTypeIndex,
            ScriptType[] filters)
        {
            this.Name = name;
            this.Server = server;
            this.DBName = dbname;
            this.AdminName = adminName;
            this.AdminPassword = adminPassword;
            this.LoginName = loginName;
            this.LoginPassword = loginPassword;
            this.AssemblyFile = assemblyFile;
            this.ObjectFilter = objectFilter;
            this.DatabaseTypeIndex = databaseTypeIndex;
            this.ScriptTypes = filters;
        }

        public Session(string line)
        {
            this.values = line
                .Split(PAIR_SEPARATOR)
                .Select(pair => pair.Split(KEY_VALUE_SEPARATOR))
                .ToDictionary(pair => pair.FirstOrDefault(), pair =>
                {
                    var k = pair.FirstOrDefault() ?? "";
                    var v = pair.LastOrDefault() ?? "";
                    object o = null;
                    if (v.Equals("True") || v.Equals("False"))
                    {
                        o = v.Equals("True");
                    }
                    else if (k == "ScriptTypes")
                    {
                        o = v.Split(ARRAY_VALUE_SEPARATOR).Select(p => (ScriptType)System.Enum.Parse(typeof(ScriptType), p)).ToArray();
                    }
                    else
                    {
                        o = v;
                    }
                    return o;
                });
        }

        private string Serialize(object obj)
        {
            if (obj.GetType().IsArray)
            {
                return string.Join(ARRAY_VALUE_SEPARATOR.ToString(), ((System.Collections.IEnumerable)obj).Cast<object>().Select(v => v.ToString()));
            }
            else
            {
                return obj.ToString();
            }
        }

        public override string ToString()
        {
            return
                string.Join(PAIR_SEPARATOR.ToString(),
                    this.values.Select(pair =>
                        string.Join(KEY_VALUE_SEPARATOR.ToString(), pair.Key, Serialize(pair.Value))));
        }
    }
}
