using SqlSiphon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace InitDB
{
    public partial class MainForm : Form
    {
        public static string DEFAULT_SESSION_NAME = "<none>";
        private static string POSTGRES = "PostgreSQL";
        private static string SQL_SERVER = "Microsoft SQL Server";
        private static string SESSIONS_FILENAME = "sessions.dat";
        private static string OPTIONS_FILENAME = "options.dat";
        private static string SQLCMD_PATH_KEY = "SQLCMDPATH";
        private static string PSQL_PATH_KEY = "PGSQLPATH";
        private static string OBJECT_FILTER_KEY = "OBJECTFILTER";
        private static string REG_ASPNET_PATH_KEY = "REGASPNETPATH";
        private static string HORIZONTAL_LINE = "================================================================================";
        private static string DEFAULT_REG_ASPNET_PATH = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\aspnet_regsql.exe";
        private static string DEFAULT_SQLCMD_PATH = @"C:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd.exe";
        private static string DEFAULT_PSQL_PATH = @"C:\Program Files\PostgreSQL\9.3\bin\psql.exe";
        private static string DEFAULT_OBJECT_FILTER = @"^(vw_)?aspnet_";

        static string UnrollStackTrace(Exception e)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendFormat(@"<stacktrace timestamp=""{0}"">
", DateTime.Now);
            var exp = e;
            for (int i = 0; exp != null; ++i)
            {
                sb.AppendFormat(
@"<exception depth=""{0}"" type=""{1}"">
    <source>{2}</source>
    <message>{3}</message>
    <stackTrace>{4}</stackTrace>
</exception>
", i, exp.GetType().FullName, exp.Source, exp.Message, exp.StackTrace);
                exp = exp.InnerException;
            }
            sb.AppendLine("</stacktrace>");
            return sb.ToString();
        }

        private Dictionary<string, Session> sessions;
        private Dictionary<string, string> options;
        private BindingList<string> names;
        private ScriptView viewScript = new ScriptView();

        public MainForm()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.InitDBLogo;
            this.browseAssemblyBtn.Tag = this.assemblyTB;
            this.browseSqlCmdButton.Tag = this.sqlcmdTB;
            this.browseRegSqlButton.Tag = this.regsqlTB;
            this.browsePsqlButton.Tag = this.psqlTB;
            this.txtStdOut.Text = string.Empty;
            this.txtStdErr.Text = string.Empty;
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            this.Text += " v" + version.ToString(4);
            LoadSessions();
            LoadOptions();
        }

        public Func<ISqlSiphon> MakeDatabaseConnector()
        {
            Func<ISqlSiphon> connector = null;
            string dbType = "UNKNOWN";
            if (File.Exists(this.assemblyTB.Text))
            {
                var ss = typeof(ISqlSiphon);
                var assembly = System.Reflection.Assembly.LoadFrom(this.assemblyTB.Text);
                var candidateTypes = assembly.GetTypes()
                    .Where(t => t.GetInterfaces().Contains(ss)
                                && !t.IsAbstract
                                && !t.IsInterface).ToArray();
                var pg = typeof(SqlSiphon.Postgres.NpgsqlDataAccessLayer);
                var ms = typeof(SqlSiphon.SqlServer.SqlServerDataAccessLayer);
                foreach (var type in candidateTypes)
                {
                    if (type.IsSubclassOf(pg) || type.IsSubclassOf(ms))
                    {
                        var vt = type.IsSubclassOf(pg) ? pg : ms;
                        dbType = vt == pg ? POSTGRES : SQL_SERVER;
                        this.IsPostgres = dbType == POSTGRES;
                        var constructorParams = new[] { typeof(string) };
                        var constructorArgs = new object[] { this.MakeConnectionString() };
                        var constructor = type.GetConstructor(constructorParams);
                        if (constructor != null)
                        {
                            connector = () => (ISqlSiphon)constructor.Invoke(constructorArgs);
                            break;
                        }
                    }
                }
            }

            this.Invoke(new Action(() =>
            {
                this.installExtensionsChk.Visible = this.IsPostgres;
                this.dbType.Text = dbType;
            }));
            return connector;
        }

        private string MakeConnectionString()
        {
            var cred = new[] { 
                new[]{this.adminUserTB.Text, this.adminPassTB.Text}, 
                new[]{this.sqlUserTB.Text, this.sqlPassTB.Text}}
                .Where(s => !string.IsNullOrEmpty(s[0]) && !string.IsNullOrEmpty(s[1]))
                .FirstOrDefault();
            string connStr = null;
            if (this.IsPostgres)
            {
                var builder = new Npgsql.NpgsqlConnectionStringBuilder
                {
                    Database = this.databaseTB.Text.ToLower()
                };

                if (cred != null)
                {
                    builder.UserName = cred[0];
                    builder.Add("Password", cred[1]);
                }

                var i = this.serverTB.Text.IndexOf(":");
                if (i > -1)
                {
                    builder.Host = this.serverTB.Text.Substring(0, i);
                    int port = 0;
                    if (int.TryParse(this.serverTB.Text.Substring(i + 1), out port))
                    {
                        builder.Port = port;
                    }
                }
                else
                {
                    builder.Host = this.serverTB.Text;
                }

                connStr = builder.ConnectionString;
            }
            else
            {
                var builder = new System.Data.SqlClient.SqlConnectionStringBuilder
                {
                    DataSource = this.serverTB.Text,
                    InitialCatalog = this.databaseTB.Text
                };
                builder.IntegratedSecurity = cred == null;
                if (cred != null)
                {
                    builder.UserID = cred[0];
                    builder.Password = cred[1];
                }
                connStr = builder.ConnectionString;
            }
            return connStr;
        }

        private void SyncUI(Action act)
        {
            if (this.InvokeRequired)
                this.Invoke(act);
            else act();
        }

        private void Dump(TextBox box, string txt, bool modal)
        {
            this.SyncUI(() =>
            {
                box.AppendText(txt + Environment.NewLine);
                if (modal)
                {
                    MessageBox.Show(txt);
                }
            });
        }

        private void ToError(Exception exp, bool modal = false)
        {
            if (exp != null)
            {
                this.ToError(UnrollStackTrace(exp), modal);
            }
        }

        private void ToError(string txt, bool modal = false)
        {
            if (txt != null)
            {
                this.Dump(this.txtStdErr, txt, modal);
                this.SyncUI(() =>
                {
                    this.tabControl1.SelectedTab = this.tabStdErr;
                    this.analyzeButton.Enabled = true;
                    this.runButton.Enabled = true;
                });
            }
        }

        private void ToOutput(string txt, bool modal = false)
        {
            this.Dump(this.txtStdOut, txt, modal);
        }

        private void LoadOptions()
        {
            if (File.Exists(OPTIONS_FILENAME))
            {
                this.options = File.ReadAllLines(OPTIONS_FILENAME)
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0)
                    .Select(l => l.Split('='))
                    .Where(p => p.Length == 2)
                    .ToDictionary(p => p[0], p => p[1]);
            }
            else
            {
                this.options = new Dictionary<string, string>();
            }

            this.DisplayOptions();
        }

        private void CoalesceOption(string key, string value, TextBox output)
        {
            if (!this.options.ContainsKey(key))
            {
                this.cancelOptionsButton.Enabled = true;
                this.options.Add(key, value);
            }
            output.Text = this.options[key];
        }

        private bool lockOptions = false;
        private void DisplayOptions()
        {
            lockOptions = true;
            this.CoalesceOption(SQLCMD_PATH_KEY, DEFAULT_SQLCMD_PATH, this.sqlcmdTB);
            this.CoalesceOption(PSQL_PATH_KEY, DEFAULT_PSQL_PATH, this.psqlTB);
            this.CoalesceOption(REG_ASPNET_PATH_KEY, DEFAULT_REG_ASPNET_PATH, this.regsqlTB);
            this.CoalesceOption(OBJECT_FILTER_KEY, DEFAULT_OBJECT_FILTER, this.defaultObjFilterTB);
            lockOptions = false;
            this.EnableSaveOption();
        }

        private void LoadSessions()
        {
            if (File.Exists(SESSIONS_FILENAME))
                this.sessions = File.ReadAllLines(SESSIONS_FILENAME)
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0)
                    .Select(l =>
                    {
                        try
                        {
                            return new Session(l);
                        }
                        catch
                        {
                            return null;
                        }
                    })
                    .Where(l => l != null)
                    .ToDictionary(s => s.Name);
            else
            {
                this.sessions = new Dictionary<string, Session>();
            }
            if (!this.sessions.ContainsKey(DEFAULT_SESSION_NAME))
                this.sessions.Add(DEFAULT_SESSION_NAME, new Session());
            this.names = new BindingList<string>(this.sessions.Keys.OrderBy(k => k).ToList());
            this.savedSessionList.DataSource = this.names;
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            this.runButton.Enabled = false;
            Application.DoEvents();
            if (PathsAreCorrect())
                Task.Run(new Action(SetupDB));
        }

        private bool IsPostgres;

        private bool PathsAreCorrect()
        {
            var sqlcmdGood = this.IsPostgres || File.Exists(sqlcmdTB.Text);
            var psqlGood = !this.IsPostgres || File.Exists(psqlTB.Text);
            var regsqlGood = File.Exists(regsqlTB.Text);
            var assemblyGood = File.Exists(assemblyTB.Text);
            if (!psqlGood)
                this.ToError("Can't find PSQL");
            if (!sqlcmdGood)
                this.ToError("Can't find SQLCMD");
            if (!regsqlGood)
                this.ToError("Can't find ASPNET_REGSQL");
            if (!assemblyGood)
                this.ToError("Can't find Assembly");
            return psqlGood && sqlcmdGood && regsqlGood && assemblyGood;
        }

        private bool RunProcess(string name, params string[] args)
        {
            var shortName = new FileInfo(name).Name;
            var succeeded = true;
            this.ToOutput(HORIZONTAL_LINE);
            this.ToOutput(string.Format(":> {0} {1}\r\n", shortName, string.Join(" ", args)));
            var procInfo = new ProcessStartInfo
            {
                FileName = name,
                Arguments = string.Join(" ", args.Where(s => s != null)),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                ErrorDialog = true,
            };
            using (var proc = new Process())
            {
                proc.StartInfo = procInfo;
                var err = new DataReceivedEventHandler(delegate(object sender, DataReceivedEventArgs e)
                {
                    if (e.Data != null)
                    {
                        this.ToError(e.Data);
                        succeeded = false;
                    }
                });
                proc.ErrorDataReceived += err;
                proc.OutputDataReceived += proc_OutputDataReceived;
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
                proc.OutputDataReceived -= proc_OutputDataReceived;
                proc.ErrorDataReceived -= err;
            }
            this.ToOutput(string.Format("finished {0}", shortName));
            this.ToOutput(HORIZONTAL_LINE);
            return succeeded;
        }

        private void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.ToOutput(e.Data);
        }

        private bool RunQueryWithPSQL(string qry, string database, bool isFile)
        {
            bool success = false;
            if (isFile && !File.Exists(qry))
            {
                this.ToError(string.Format("Tried running script from file \"{0}\", but it doesn't exist!", qry));
            }
            else if (string.IsNullOrWhiteSpace(adminUserTB.Text) || string.IsNullOrWhiteSpace(adminPassTB.Text))
            {
                this.ToError("PSQL does not support Windows Authentication. Please provide a username and password for the server process.");
            }
            else
            {
                string server = serverTB.Text;
                string port = null;
                var i = server.IndexOf(":");
                if (i > -1)
                {
                    port = server.Substring(i + 1);
                    server = server.Substring(0, i);
                }

                // we can't send a password to the server directly, so we have twiddle the user's
                // pgpass.conf file. See this page for more information on the pgpass.conf file:
                //     http://www.postgresql.org/docs/current/static/libpq-pgpass.html
                var confPath = System.Windows.Forms.Application.UserAppDataPath;
                i = confPath.IndexOf("InitDB");
                confPath = Path.Combine(confPath.Substring(0, i), "postgresql", "pgpass.conf");
                bool lineAdded = false;
                string[] originalConf = null;
                if (File.Exists(confPath))
                {
                    originalConf = File.ReadAllLines(confPath);
                }
                var lineToAdd = string.Format(
                    "{0}:{1}:{2}:{3}:{4}",
                    server,
                    port ?? "*",
                    database ?? "*",
                    adminUserTB.Text,
                    adminPassTB.Text);

                if (originalConf == null)
                {
                    lineAdded = true;
                    File.WriteAllText(confPath, lineToAdd);
                }
                else
                {
                    var conf = originalConf.ToList();
                    i = conf.IndexOf(lineToAdd);
                    if (i == -1)
                    {
                        conf.Add(lineToAdd);
                        lineAdded = true;
                        File.WriteAllLines(confPath, conf);
                    }
                }

                try
                {
                    success = RunProcess(
                        psqlTB.Text,
                        "-h " + server,
                        string.IsNullOrWhiteSpace(port) ? null : "-p " + port,
                        "-U " + adminUserTB.Text,
                        string.Format(" -{0} \"{1}\"", isFile ? "f" : "c", qry),
                        (database != null) ? "-d " + database : null);
                }
                catch (Exception exp)
                {
                    this.ToError(exp);
                    success = false;
                }
                finally
                {
                    // put everything back the way it was
                    if (lineAdded)
                    {
                        if (originalConf == null)
                        {
                            File.Delete(confPath);
                        }
                        else
                        {
                            File.WriteAllLines(confPath, originalConf);
                        }
                    }
                }
            }
            return success;
        }

        private bool RunQueryWithSQLCMD(string qry, string database, bool isFile)
        {
            bool success = false;
            if (isFile && !File.Exists(qry))
            {
                this.ToError(string.Format("Tried running script from file \"{0}\", but it doesn't exist!", qry));
            }
            else
            {
                success = RunProcess(
                    sqlcmdTB.Text,
                    "-S " + serverTB.Text,
                    string.IsNullOrWhiteSpace(adminUserTB.Text) ? null : "-U " + adminUserTB.Text,
                    string.IsNullOrWhiteSpace(adminPassTB.Text) ? null : "-P " + adminPassTB.Text,
                    (database != null) ? "-d " + database : null,
                    string.Format(" -{0} \"{1}\"", isFile ? "i" : "Q", qry));
            }
            return success;
        }

        private bool RunASPNET_REGSQL()
        {
            return RunProcess(
                regsqlTB.Text,
                "-S " + serverTB.Text,
                adminUserTB.Text != string.Empty ? "-U " + adminUserTB.Text : null,
                adminPassTB.Text != string.Empty ? "-P " + adminPassTB.Text : null,
                adminUserTB.Text == string.Empty ? "-E " : null,
                "-A mr",
                "-d " + databaseTB.Text);
        }

        private void SetupDB()
        {
            var connector = this.MakeDatabaseConnector();
            Func<string, string, bool, bool> runQuery;
            if (this.IsPostgres)
            {
                runQuery = RunQueryWithPSQL;
            }
            else
            {
                runQuery = RunQueryWithSQLCMD;
            }

            try
            {
                bool succeeded = true;
                var dbName = databaseTB.Text;
                if (this.IsPostgres)
                {
                    dbName = dbName.ToLower();
                }

                if (createDatabaseChk.Checked)
                {
                    succeeded = runQuery(string.Format("CREATE DATABASE {0};", dbName), null, false);
                }
                if (this.IsPostgres && succeeded && installExtensionsChk.Checked)
                {
                    succeeded &= runQuery("CREATE EXTENSION \\\"uuid-ossp\\\";", dbName, false)
                        && runQuery("CREATE EXTENSION \\\"postgis\\\";", dbName, false);
                }
                if (succeeded && createLoginChk.Checked)
                {
                    if (this.IsPostgres)
                    {
                        succeeded = runQuery(string.Format("CREATE USER {0} WITH PASSWORD '{1}'", sqlUserTB.Text, sqlPassTB.Text), null, false);
                    }
                    else
                    {
                        succeeded = runQuery(string.Format("CREATE LOGIN {0} WITH PASSWORD = '{1}', DEFAULT_DATABASE={2};", sqlUserTB.Text, sqlPassTB.Text, dbName), null, false)
                            && runQuery(string.Format("CREATE USER {0} FOR LOGIN {0};", sqlUserTB.Text), dbName, false)
                            && runQuery(string.Format("ALTER USER {0} WITH DEFAULT_SCHEMA=dbo;", sqlUserTB.Text), dbName, false)
                            && runQuery(string.Format("ALTER ROLE db_owner ADD MEMBER {0};", sqlUserTB.Text), dbName, false);
                    }
                }

                if (succeeded && !this.IsPostgres && regSqlChk.Checked)
                {
                    succeeded = RunASPNET_REGSQL();
                }

                if (succeeded)
                {
                    using (var db = connector())
                    {
                        db.Progress += db_Progress;
                        var delta = db.Analyze(ObjectFilter);
                        DisplayDelta(delta);
                        var t = db.GetType();
                        this.ToOutput(string.Format("Syncing {0}.{1}", t.Namespace, t.Name));
                        if (succeeded && createTablesChk.Checked)
                        {
                            succeeded &= RunScripts("Creating table:", delta.CreateTableScripts, db);
                            succeeded &= RunScripts("Creating columns:", delta.CreateColumnScripts, db);
                        }

                        if (succeeded && createFKsChk.Checked)
                        {
                            succeeded &= RunScripts("Creating foreign keys:", delta.CreateRelationshipScripts.Reverse(), db);
                        }

                        if (succeeded && createIndicesChk.Checked)
                        {
                            succeeded &= RunScripts("Creating indexes:", delta.CreateIndexScripts, db);
                        }

                        if (succeeded && syncProceduresChk.Checked)
                        {
                            succeeded &= RunScripts("Creating stored procedures:", delta.CreateRoutineScripts, db);
                            succeeded &= RunScripts("Altering stored procedures:", delta.AlteredRoutineScripts, db);
                        }

                        if (succeeded && initializeDataChk.Checked)
                        {
                            succeeded &= RunScripts("Syncing data:", delta.OtherScripts, db);
                        }
                        delta = db.Analyze(ObjectFilter);
                        DisplayDelta(delta);
                    }
                }

                if (succeeded)
                {
                    this.ToOutput("All done", true);
                }
                else
                {
                    this.ToError("There was an error. Rerun in debug mode and step through the program in the debugger.", true);
                }
            }
            catch (Exception exp)
            {
                this.ToError(exp, true);
            }
            finally
            {
                this.SyncUI(() => runButton.Enabled = true);
            }
        }

        private bool RunScripts(string feature, IEnumerable<KeyValuePair<string, string>> scripts, ISqlSiphon db)
        {
            foreach (var script in scripts)
            {
                try
                {
                    this.ToOutput(string.Format("{0} {1}.", feature, script.Key));
                    db.AlterDatabase(script.Value);
                }
                catch (Exception exp)
                {
                    this.ToError(exp);
                }
            }
            return true;
        }

        private void db_Progress(object sender, DataProgressEventArgs e)
        {
            this.ToOutput(string.Format("{0} of {1}: {2}", e.CurrentRow + 1, e.RowCount, e.Message));
        }

        private Regex ObjectFilter { get { return new Regex(this.objFilterTB.Text, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled); } }

        private void analyzeButton_Click(object sender, EventArgs e)
        {
            var connector = this.MakeDatabaseConnector();
            analyzeButton.Enabled = false;
            if (PathsAreCorrect())
            {
                Task.Run(() =>
                {
                    try
                    {
                        this.ToOutput("Synchronizing schema.");
                        using (var db = connector())
                        {
                            DisplayDelta(db.Analyze(ObjectFilter));
                        }
                        this.ToOutput("Schema synched.");
                    }
                    catch (Exception exp)
                    {
                        this.ToError(exp);
                    }
                });
            }
        }

        private void DisplayDelta(DatabaseDelta delta)
        {
            this.SyncUI(() =>
            {
                FillGV(this.createTablesGV, delta.CreateTableScripts);
                FillGV(this.dropTablesGV, delta.DropTableScripts);
                FillGV(this.unalteredTablesGV, delta.UnalteredTableScripts);
                FillGV(this.createColumnsGV, delta.CreateColumnScripts);
                FillGV(this.dropColumnsGV, delta.DropColumnScripts);
                FillGV(this.alteredColumnsGV, delta.AlteredColumnScripts);
                FillGV(this.unalteredColumnsGV, delta.UnalteredColumnScripts);
                FillGV(this.createRelationshipsGV, delta.CreateRelationshipScripts.Reverse());
                FillGV(this.dropRelationshipsGV, delta.DropRelationshipScripts);
                FillGV(this.unalteredRelationshipsGV, delta.UnalteredRelationshipScripts);
                FillGV(this.createRoutinesGV, delta.CreateRoutineScripts);
                FillGV(this.dropRoutinesGV, delta.DropRoutineScripts);
                FillGV(this.alteredRoutinesGV, delta.AlteredRoutineScripts);
                FillGV(this.unalteredRoutinesGV, delta.UnalteredRoutineScripts);
                FillGV(this.createIndicesGV, delta.CreateIndexScripts);
                FillGV(this.dropIndicesGV, delta.DropIndexScripts);
                FillGV(this.unalteredIndicesGV, delta.UnalteredIndexScripts);
                FillGV(this.othersGV, delta.OtherScripts);
                this.analyzeButton.Enabled = true;
            });
        }

        private static void FillGV(DataGridView gv, IEnumerable<KeyValuePair<string, string>> collect)
        {
            gv.Rows.Clear();
            foreach (var entry in collect)
                gv.Rows.Add(entry.Key, entry.Value);
        }

        private void BrowseFile(TextBox path)
        {
            try
            {
                var file = new FileInfo(path.Text);
                openFileDialog1.InitialDirectory = file.DirectoryName;
                openFileDialog1.FileName = file.Name;
            }
            catch
            {
                // empty string can make the FileInfo constructor error,
                // but I don't care, it's just a convenience and the user
                // will recover it on their own.
            }
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path.Text = openFileDialog1.FileName;
            }

        }

        private void browse_Click(object sender, EventArgs e)
        {
            BrowseFile((TextBox)((Button)sender).Tag);
        }

        private void saveSessionButton_Click(object sender, EventArgs e)
        {
            var sesh = new Session(
                this.savedSessionList.Text,
                this.serverTB.Text,
                this.databaseTB.Text,
                this.adminUserTB.Text,
                this.adminPassTB.Text,
                this.sqlUserTB.Text,
                this.sqlPassTB.Text,
                this.assemblyTB.Text,
                this.objFilterTB.Text,
                this.createDatabaseChk.Checked,
                this.createLoginChk.Checked,
                this.regSqlChk.Checked,
                this.createTablesChk.Checked,
                this.initializeDataChk.Checked,
                this.syncProceduresChk.Checked,
                this.createFKsChk.Checked,
                this.createIndicesChk.Checked,
                this.installExtensionsChk.Checked);

            if (this.sessions.ContainsKey(sesh.Name))
                this.sessions[sesh.Name] = sesh;
            else
            {
                this.sessions.Add(sesh.Name, sesh);
                this.names.Add(sesh.Name);
                this.savedSessionList.SelectedItem = sesh.Name;
            }

            this.SaveSessions();
        }

        private Session CurrentSession
        {
            get
            {
                Session value = null;
                this.SyncUI(() =>
                {
                    if (this.sessions.ContainsKey(this.savedSessionList.Text))
                        value = this.sessions[this.savedSessionList.Text];
                });
                return value;
            }
        }

        private void SaveSessions()
        {
            File.WriteAllLines(SESSIONS_FILENAME, this.sessions.Values.Select(v => v.ToString()).ToArray());
        }

        private void deleteSessionButton_Click(object sender, EventArgs e)
        {
            var sessionName = savedSessionList.SelectedItem as string;
            if (sessionName != null)
            {
                if (this.sessions.ContainsKey(sessionName))
                {
                    this.sessions.Remove(sessionName);
                    this.names.Remove(sessionName);
                    this.SaveSessions();
                }
            }
        }

        private void savedSessionList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var sessionName = savedSessionList.SelectedItem as string;
            if (sessionName != null)
            {
                if (this.CurrentSession != null)
                {
                    this.dbType.Text = "UNKNOWN";
                    this.serverTB.Text = this.CurrentSession.Server;
                    this.databaseTB.Text = this.CurrentSession.DBName;
                    this.adminUserTB.Text = this.CurrentSession.AdminName;
                    this.adminPassTB.Text = this.CurrentSession.AdminPassword;
                    this.sqlUserTB.Text = this.CurrentSession.LoginName;
                    this.sqlPassTB.Text = this.CurrentSession.LoginPassword;
                    this.assemblyTB.Text = this.CurrentSession.AssemblyFile;
                    this.objFilterTB.Text = this.CurrentSession.ObjectFilter ?? this.defaultObjFilterTB.Text;
                    this.createDatabaseChk.Checked = this.CurrentSession.CreateDatabase;
                    this.createLoginChk.Checked = this.CurrentSession.CreateLogin;
                    this.regSqlChk.Checked = this.CurrentSession.RegisterASPNETMembership;
                    this.createTablesChk.Checked = this.CurrentSession.CreateSchemaObjects;
                    this.initializeDataChk.Checked = this.CurrentSession.InitializeData;
                    this.syncProceduresChk.Checked = this.CurrentSession.SyncStoredProcedures;
                    this.createFKsChk.Checked = this.CurrentSession.CreateFKs;
                    this.createIndicesChk.Checked = this.CurrentSession.CreateIndices;
                    this.installExtensionsChk.Checked = this.CurrentSession.InstallExtensions;

                    try
                    {
                        this.txtStdOut.Text = "";
                        this.txtStdErr.Text = "";
                    }
                    catch { }
                }

                saveSessionButton.Enabled
                    = deleteSessionButton.Enabled
                    = (sessionName != DEFAULT_SESSION_NAME);

                this.createTablesGV.Rows.Clear();
                this.dropTablesGV.Rows.Clear();
                this.unalteredTablesGV.Rows.Clear();
                this.createColumnsGV.Rows.Clear();
                this.dropColumnsGV.Rows.Clear();
                this.alteredColumnsGV.Rows.Clear();
                this.unalteredColumnsGV.Rows.Clear();
                this.createRelationshipsGV.Rows.Clear();
                this.dropRelationshipsGV.Rows.Clear();
                this.unalteredRelationshipsGV.Rows.Clear();
                this.createRoutinesGV.Rows.Clear();
                this.dropRoutinesGV.Rows.Clear();
                this.alteredRoutinesGV.Rows.Clear();
                this.unalteredRoutinesGV.Rows.Clear();
                this.createIndicesGV.Rows.Clear();
                this.dropIndicesGV.Rows.Clear();
                this.unalteredIndicesGV.Rows.Clear();
                this.othersGV.Rows.Clear();
            }
        }

        private void savedSessionList_TextUpdate(object sender, EventArgs e)
        {
            var sessionName = savedSessionList.Text.Trim();
            if (sessionName.Length == 0)
                sessionName = DEFAULT_SESSION_NAME;
            saveSessionButton.Enabled
                = deleteSessionButton.Enabled
                = (sessionName != DEFAULT_SESSION_NAME);

            if (sessionName == DEFAULT_SESSION_NAME)
                savedSessionList.SelectedItem = DEFAULT_SESSION_NAME;
        }

        private void enableSaveCancelButtons(object sender, EventArgs e)
        {
            if (!lockOptions)
            {
                EnableSaveCancelOptions();
            }
        }

        private void EnableSaveCancelOptions()
        {
            cancelOptionsButton.Enabled =
                this.options[REG_ASPNET_PATH_KEY] != this.regsqlTB.Text
                || this.options[SQLCMD_PATH_KEY] != this.sqlcmdTB.Text
                || this.options[PSQL_PATH_KEY] != this.psqlTB.Text
                || this.options[OBJECT_FILTER_KEY] != this.defaultObjFilterTB.Text;
            EnableSaveOption();
        }

        private void EnableSaveOption()
        {
            saveOptionsButton.Enabled
                = File.Exists(sqlcmdTB.Text)
                    && File.Exists(regsqlTB.Text)
                    && File.Exists(psqlTB.Text)
                    && cancelOptionsButton.Enabled;
        }

        private void saveOptionsButton_Click(object sender, EventArgs e)
        {
            this.options[SQLCMD_PATH_KEY] = this.sqlcmdTB.Text;
            this.options[REG_ASPNET_PATH_KEY] = this.regsqlTB.Text;
            File.WriteAllLines(OPTIONS_FILENAME,
                this.options.Select(kv => string.Join("=", kv.Key, kv.Value)).ToArray());
            EnableSaveCancelOptions();
        }

        private void cancelOptionsButton_Click(object sender, EventArgs e)
        {
            this.DisplayOptions();
        }

        private void scriptGV_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var gv = sender as DataGridView;
            if (gv != null)
            {
                Task.Run(() =>
                {
                    this.SyncUI(() =>
                    {
                        try
                        {
                            var script = (string)gv.Rows[e.RowIndex].Cells[1].Value;
                            gv.Enabled = false;
                            Application.DoEvents();
                            switch (e.ColumnIndex)
                            {
                                case 0:
                                    gv.Rows[e.RowIndex].Cells[1].Value = viewScript.Prompt(script);
                                    break;
                                case 2:
                                    RunScript(script);
                                    gv.Rows.RemoveAt(e.RowIndex);
                                    break;
                                case 3:
                                    SkipScript(script);
                                    gv.Rows.RemoveAt(e.RowIndex);
                                    break;
                            }
                            this.ToOutput("Success");
                        }
                        catch (Exception exp)
                        {
                            this.tabControl1.SelectedTab = this.tabStdErr;
                            this.ToError(string.Format("{0}: {1}", exp.GetType().Name, exp.Message));
                        }
                        finally
                        {
                            gv.Enabled = true;
                        }
                    });
                });
            }
        }

        private void SkipScript(string script)
        {
            var connector = this.MakeDatabaseConnector();
            using (var db = connector())
                db.MarkScriptAsRan(script);
        }

        private void RunScript(string script)
        {
            this.tabControl1.SelectedTab = this.tabStdOut;
            this.ToOutput(script);
            var connector = this.MakeDatabaseConnector();
            using (var db = connector())
                db.AlterDatabase(script);
        }
    }
}
