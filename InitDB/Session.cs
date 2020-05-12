using System;
using System.Globalization;
using System.Linq;

using SqlSiphon;

namespace InitDB
{
    internal class Session : BoundObject
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
                return int.Parse(Get<string>(), NumberStyles.Integer, CultureInfo.InvariantCulture);
            }
            set
            {
                Set(value.ToString(CultureInfo.InvariantCulture));
            }
        }
        public ScriptType[] ScriptTypes { get { return Get<ScriptType[]>(); } set { Set(value); } }

        public static char PAIR_SEPARATOR = ';';
        public static char KEY_VALUE_SEPARATOR = ':';
        public static char ARRAY_VALUE_SEPARATOR = ',';

        public Session()
            : this(
                  MainForm.DEFAULT_SESSION_NAME,
                  "localhost",
                  "", "", "", "", "", "", "",
                  0, Array.Empty<ScriptType>())
        {
        }

        public Session(string name, string server, string dbname, string adminName, string adminPassword,
            string loginName, string loginPassword, string assemblyFile, string objectFilter, int databaseTypeIndex,
            ScriptType[] filters)
        {
            Name = name;
            Server = server;
            DBName = dbname;
            AdminName = adminName;
            AdminPassword = adminPassword;
            LoginName = loginName;
            LoginPassword = loginPassword;
            AssemblyFile = assemblyFile;
            ObjectFilter = objectFilter;
            DatabaseTypeIndex = databaseTypeIndex;
            ScriptTypes = filters;
        }

        public Session(string line)
        {
            foreach (var pair in line.Split(PAIR_SEPARATOR))
            {
                var parts = pair.Split(KEY_VALUE_SEPARATOR);
                var k = parts.FirstOrDefault() ?? "";
                var v = parts.LastOrDefault() ?? "";
                var isTrue = v.Equals("True", StringComparison.InvariantCultureIgnoreCase);
                var isFalse = v.Equals("False", StringComparison.InvariantCultureIgnoreCase);
                object o = null;
                if (isTrue || isFalse)
                {
                    o = isTrue;
                }
                else if (k == "ScriptTypes")
                {
                    o = v.Split(ARRAY_VALUE_SEPARATOR)
                        .Where(p => p.Length > 0)
                        .Select(p => (ScriptType)Enum.Parse(typeof(ScriptType), p))
                        .ToArray();
                }
                else
                {
                    o = v;
                }

                Values.Add(k, o);
            }
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
                    Values.Select(pair =>
                        string.Join(KEY_VALUE_SEPARATOR.ToString(), pair.Key, Serialize(pair.Value))));
        }
    }
}
