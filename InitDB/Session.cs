﻿using System.Linq;
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
        public bool CreateDatabase { get { return Get<bool>(); } set { Set(value); } }
        public bool CreateLogin { get { return Get<bool>(); } set { Set(value); } }
        public bool CreateSchemaObjects { get { return Get<bool>(); } set { Set(value); } }
        public bool InitializeData { get { return Get<bool>(); } set { Set(value); } }
        public bool SyncStoredProcedures { get { return Get<bool>(); } set { Set(value); } }
        public bool CreateFKs { get { return Get<bool>(); } set { Set(value); } }
        public bool CreateIndices { get { return Get<bool>(); } set { Set(value); } }
        public bool InstallExtensions { get { return Get<bool>(); } set { Set(value); } }

        public static char PAIR_SEPARATOR = ';';
        public static char KEY_VALUE_SEPARATOR = ':';


        public Session()
            : this(InitDB.MainForm.DEFAULT_SESSION_NAME, "localhost\\SQLEXPRESS", "", "", "", "", "", "", "", false, false, false, false, true, false, false, false)
        {
        }

        public Session(string name, string server, string dbname, string adminName, string adminPassword,
            string loginName, string loginPassword, string assemblyFile, string objectFilter,
            bool createDatabase, bool createLogin, bool createSchemaObj, bool initData, bool syncProcs,
            bool createFKs, bool createIndices, bool installExtensions)
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
            this.CreateDatabase = createDatabase;
            this.CreateLogin = createLogin;
            this.CreateSchemaObjects = createSchemaObj;
            this.InitializeData = initData;
            this.SyncStoredProcedures = syncProcs;
            this.CreateFKs = createFKs;
            this.CreateIndices = createIndices;
            this.InstallExtensions = installExtensions;
        }

        public Session(string line)
        {
            this.values = line
                .Split(PAIR_SEPARATOR)
                .Select(pair => pair.Split(KEY_VALUE_SEPARATOR))
                .ToDictionary(pair => pair.FirstOrDefault(), pair =>
                {
                    var v = pair.LastOrDefault() ?? "";
                    var o = v.Equals("True") || v.Equals("False")
                        ? (object)v.Equals("True")
                        : (object)v;
                    return o;
                });
        }

        public override string ToString()
        {
            return
                string.Join(PAIR_SEPARATOR.ToString(),
                    this.values.Select(pair =>
                        string.Join(KEY_VALUE_SEPARATOR.ToString(), pair.Key, pair.Value)));
        }
    }
}
