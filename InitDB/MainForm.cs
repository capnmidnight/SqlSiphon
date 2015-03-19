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
        private static string HORIZONTAL_LINE = "================================================================================";
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
        private OptionsDialog optionsDialog = new OptionsDialog();

        public MainForm()
        {
            InitializeComponent();
            this.pendingScriptsGV.AutoGenerateColumns = false;
            this.Icon = Properties.Resources.InitDBLogo;
            this.txtStdOut.Text = string.Empty;
            this.txtStdErr.Text = string.Empty;
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            this.Text += " v" + version.ToString(4);
            LoadSessions();
            LoadOptions();
            this.optionsDialog.BrowsePSQLPathClick += optionsDialog_BrowsePSQLPathClick;
            this.optionsDialog.BrowseSQLCMDPathClick += optionsDialog_BrowseSQLCMDPathClick;
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

        private string CoalesceOption(string key, string defaultValue)
        {
            if (!this.options.ContainsKey(key))
            {
                this.options.Add(key, defaultValue);
            }
            return this.options[key];
        }

        private void DisplayOptions()
        {
            this.optionsDialog.SQLCMDPath = this.CoalesceOption(SQLCMD_PATH_KEY, DEFAULT_SQLCMD_PATH);
            this.optionsDialog.PSQLPath = this.CoalesceOption(PSQL_PATH_KEY, DEFAULT_PSQL_PATH);
            this.optionsDialog.DefaultObjectFilterRegexText = this.CoalesceOption(OBJECT_FILTER_KEY, DEFAULT_OBJECT_FILTER);
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
            var sqlcmdGood = this.IsPostgres || File.Exists(this.optionsDialog.SQLCMDPath);
            var psqlGood = !this.IsPostgres || File.Exists(this.optionsDialog.PSQLPath);
            var assemblyGood = File.Exists(assemblyTB.Text);
            if (!psqlGood)
                this.ToError("Can't find PSQL");
            if (!sqlcmdGood)
                this.ToError("Can't find SQLCMD");
            if (!assemblyGood)
                this.ToError("Can't find Assembly");
            return psqlGood && sqlcmdGood && assemblyGood;
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
                        this.optionsDialog.PSQLPath,
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
                    this.optionsDialog.SQLCMDPath,
                    "-S " + serverTB.Text,
                    string.IsNullOrWhiteSpace(adminUserTB.Text) ? null : "-U " + adminUserTB.Text,
                    string.IsNullOrWhiteSpace(adminPassTB.Text) ? null : "-P " + adminPassTB.Text,
                    (database != null) ? "-d " + database : null,
                    string.Format(" -{0} \"{1}\"", isFile ? "i" : "Q", qry));
            }
            return success;
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

                if (succeeded)
                {
                    using (var db = connector())
                    {
                        db.Progress += db_Progress;
                        var delta = db.Analyze(ObjectFilter);
                        DisplayDelta(delta);
                        var t = db.GetType();
                        this.ToOutput(string.Format("Syncing {0}.{1}", t.Namespace, t.Name));
                        if (succeeded)
                        {
                            succeeded = RunScripts(delta.Scripts, db);
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

        private bool RunScripts(IEnumerable<ScriptStatus> scripts, ISqlSiphon db)
        {
            var success = true;
            foreach (var script in scripts)
            {
                if (script.Run)
                {
                    try
                    {
                        this.ToOutput(string.Format("{0} {1}.", script.Script, script.Name));
                        db.AlterDatabase(script);
                    }
                    catch (Exception exp)
                    {
                        this.ToError(exp);
                        success = false;
                    }
                }
            }
            return success;
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
                var list = new BindingList<ScriptStatus>(delta.Scripts.OrderBy(s => s.ScriptType).ToList());
                this.pendingScriptsGV.DataSource = list;
                this.analyzeButton.Enabled = true;
            });
        }

        private string BrowseFrom(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && (File.Exists(path) || Directory.Exists(path)))
            {
                var file = new FileInfo(path);
                openFileDialog1.InitialDirectory = file.DirectoryName;
                openFileDialog1.FileName = file.Name;
            }
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return openFileDialog1.FileName;
            }
            return null;
        }

        private void browseAssemblyBtn_Click(object sender, EventArgs e)
        {
            assemblyTB.Text = BrowseFrom(assemblyTB.Text) ?? assemblyTB.Text;
        }

        void optionsDialog_BrowseSQLCMDPathClick(object sender, EventArgs e)
        {
            this.optionsDialog.SQLCMDPath = BrowseFrom(this.optionsDialog.SQLCMDPath) ?? this.optionsDialog.SQLCMDPath;
        }

        void optionsDialog_BrowsePSQLPathClick(object sender, EventArgs e)
        {
            this.optionsDialog.PSQLPath = BrowseFrom(this.optionsDialog.PSQLPath) ?? this.optionsDialog.PSQLPath;
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
                    this.objFilterTB.Text = this.CurrentSession.ObjectFilter ?? this.optionsDialog.DefaultObjectFilterRegexText;
                    this.createDatabaseChk.Checked = this.CurrentSession.CreateDatabase;
                    this.createLoginChk.Checked = this.CurrentSession.CreateLogin;
                    this.createTablesChk.Checked = this.CurrentSession.CreateSchemaObjects;
                    this.initializeDataChk.Checked = this.CurrentSession.InitializeData;
                    this.syncProceduresChk.Checked = this.CurrentSession.SyncStoredProcedures;
                    this.createFKsChk.Checked = this.CurrentSession.CreateFKs;
                    this.createIndicesChk.Checked = this.CurrentSession.CreateIndices;
                    this.installExtensionsChk.Checked = this.CurrentSession.InstallExtensions;
                    this.txtStdOut.Text = "";
                    this.txtStdErr.Text = "";
                }

                saveSessionButton.Enabled
                    = deleteSessionButton.Enabled
                    = (sessionName != DEFAULT_SESSION_NAME);
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
                            var col = pendingScriptScriptColumn.Index;
                            var row = gv.Rows[e.RowIndex];
                            var scriptCell = row.Cells[col];
                            var script = (string)scriptCell.Value;
                            gv.Enabled = false;
                            Application.DoEvents();
                            if (e.ColumnIndex == pendingScriptNameColumn.Index)
                            {
                                scriptCell.Value = viewScript.Prompt(script);
                            }
                            else if (e.ColumnIndex == pendingScriptRunButtonColumn.Index)
                            {
                                RunScript((ScriptStatus)row.DataBoundItem);
                                gv.Rows.RemoveAt(e.RowIndex);
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

        private void RunScript(ScriptStatus script)
        {
            this.tabControl1.SelectedTab = this.tabStdOut;
            this.ToOutput(script.Script);
            var connector = this.MakeDatabaseConnector();
            using (var db = connector())
                db.AlterDatabase(script);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.optionsDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.options[SQLCMD_PATH_KEY] = this.optionsDialog.SQLCMDPath;
                File.WriteAllLines(OPTIONS_FILENAME,
                    this.options.Select(kv => string.Join("=", kv.Key, kv.Value)).ToArray());
            }
            else
            {
                this.DisplayOptions();
            }
        }
    }
}
