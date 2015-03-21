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
    <stackTrace>{4}</stackTrace>ception>
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
            foreach (ScriptType value in Enum.GetValues(typeof(ScriptType)))
            {
                if (value != ScriptType.None)
                {
                    this.filterTypesCBL.Items.Add(value);
                }
            }
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
                this.createExtensionsBtn.Visible = this.IsPostgres;
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
            {
                tabControl1.SelectedTab = tabStdOut;
                Task.Run(new Action(SetupDB));
            }
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

        private void RunProcess(Action<bool> complete, string name, params string[] args)
        {
            var succeeded = true;
            var shortName = new FileInfo(name).Name;
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
            var proc = new Process();
            proc.StartInfo = procInfo;
            var err = new DataReceivedEventHandler(delegate(object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                {
                    this.ToError(e.Data);
                    succeeded = false;
                }
            });
            proc.EnableRaisingEvents = true;
            proc.ErrorDataReceived += err;
            proc.OutputDataReceived += proc_OutputDataReceived;
            var exit = new EventHandler(delegate(object sender, EventArgs e)
            {
                this.SyncUI(() =>
                {
                    this.ToOutput(string.Format("finished {0}", shortName));
                    this.ToOutput(HORIZONTAL_LINE);
                });
                proc.OutputDataReceived -= proc_OutputDataReceived;
                proc.ErrorDataReceived -= err;
                proc.Dispose();
                complete(succeeded);
            });
            proc.Exited += exit;
            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
        }

        private void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.ToOutput(e.Data);
        }

        private void RunQueryWithPSQL(string qry, string database, bool isFile, Action<bool> complete)
        {
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

                RunProcess((b) =>
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
                    complete(b);
                }, this.optionsDialog.PSQLPath, "-h " + server, string.IsNullOrWhiteSpace(port) ? null : "-p " + port, "-U " + adminUserTB.Text, string.Format(" -{0} \"{1}\"", isFile ? "f" : "c", qry), (database != null) ? "-d " + database : null);
            }
        }

        private void RunQueryWithSQLCMD(string qry, string database, bool isFile, Action<bool> complete)
        {
            if (isFile && !File.Exists(qry))
            {
                this.ToError(string.Format("Tried running script from file \"{0}\", but it doesn't exist!", qry));
            }
            else
            {
                RunProcess(complete, this.optionsDialog.SQLCMDPath, "-S " + serverTB.Text, string.IsNullOrWhiteSpace(adminUserTB.Text) ? null : "-U " + adminUserTB.Text, string.IsNullOrWhiteSpace(adminPassTB.Text) ? null : "-P " + adminPassTB.Text, (database != null) ? "-d " + database : null, string.Format(" -{0} \"{1}\"", isFile ? "i" : "Q", qry));
            }
        }

        private void RunQuery(string qry, string database, bool isFile, Action<bool> complete)
        {
            if (this.IsPostgres)
            {
                RunQueryWithPSQL(qry, database, isFile, complete);
            }
            else
            {
                RunQueryWithSQLCMD(qry, database, isFile, complete);
            }
        }


        private void createExtensionsBtn_Click(object sender, EventArgs e)
        {
            RunQuery("CREATE EXTENSION \\\"uuid-ossp\\\";", DatabaseName, false, (succeeded) =>
            {
                if (succeeded)
                {
                    RunQuery("CREATE EXTENSION \\\"postgis\\\";", DatabaseName, false, StepsComplete);
                }
                else
                {
                    StepsComplete(false);
                }
            });
        }

        private void createUserBtn_Click(object sender, EventArgs e)
        {
            if (this.IsPostgres)
            {
                RunQuery(string.Format("CREATE USER {0} WITH PASSWORD '{1}'", sqlUserTB.Text, sqlPassTB.Text), null, false, StepsComplete);
            }
            else
            {
                RunQuery(string.Format("CREATE LOGIN {0} WITH PASSWORD = '{1}', DEFAULT_DATABASE={2};", sqlUserTB.Text, sqlPassTB.Text, DatabaseName), null, false, SetupDB4_2);
            }
        }

        private void SetupDB4_2(bool succeeded)
        {
            if (succeeded)
            {
                RunQuery(string.Format("CREATE USER {0} FOR LOGIN {0};", sqlUserTB.Text), DatabaseName, false, SetupDB4_3);
            }
            else
            {
                StepsComplete(false);
            }
        }

        private void SetupDB4_3(bool succeeded)
        {
            if (succeeded)
            {
                RunQuery(string.Format("ALTER USER {0} WITH DEFAULT_SCHEMA=dbo;", sqlUserTB.Text), DatabaseName, false, SetupDB4_4);
            }
            else
            {
                StepsComplete(false);
            }
        }

        private void SetupDB4_4(bool succeeded)
        {
            if (succeeded)
            {
                RunQuery(string.Format("ALTER ROLE db_owner ADD MEMBER {0};", sqlUserTB.Text), DatabaseName, false, StepsComplete);
            }
            else
            {
                StepsComplete(false);
            }
        }

        private void StepsComplete(bool succeeded)
        {
            if (succeeded)
            {
                this.ToOutput("Succeeded.");
            }
            else
            {
                this.ToError("Failed.");
            }
        }

        private void SetupDB()
        {
            var connector = MakeDatabaseConnector();
            var db = connector();
            db.Progress += db_Progress;
            var delta = db.Analyze(this.databaseTB.Text, ObjectFilter);
            DisplayDelta(delta);
            var t = db.GetType();
            this.ToOutput(string.Format("Syncing {0}.{1}", t.Namespace, t.Name));

            RunScripts(delta.Scripts, db, (succeed) =>
            {
                if (succeed)
                {
                    this.ToOutput("All done", true);
                }
                else
                {
                    this.ToError("There was an error. Rerun in debug mode and step through the program in the debugger.", true);
                }

                delta = db.Analyze(this.databaseTB.Text, ObjectFilter);
                DisplayDelta(delta);
                this.SyncUI(() => runButton.Enabled = true);
                db.Dispose();
            });
        }

        private string DatabaseName
        {
            get
            {
                var dbName = databaseTB.Text;
                if (this.IsPostgres)
                {
                    dbName = dbName.ToLower();
                }
                return dbName;
            }
        }

        private void RunScripts(IEnumerable<ScriptStatus> scripts, ISqlSiphon db, Action<bool> complete)
        {
            bool defered = false;
            try
            {
                foreach (var script in scripts)
                {
                    if (script.Run)
                    {
                        this.ToOutput(string.Format("{0} {1}.", script.Script, script.Name));
                        if (script.ScriptType == ScriptType.CreateCatalogue)
                        {
                            defered = true;
                            this.RunQuery(script.Script, null, false, complete);
                        }
                        else
                        {
                            db.AlterDatabase(script);
                        }
                    }
                }
                if (!defered)
                {
                    complete(true);
                }
            }
            catch (Exception exp)
            {
                this.ToError(exp);
                complete(false);
            }
        }

        private void db_Progress(object sender, DataProgressEventArgs e)
        {
            this.ToOutput(string.Format("{0} of {1}: {2}", e.CurrentRow + 1, e.RowCount, e.Message));
        }

        private Regex ObjectFilter
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.objFilterTB.Text))
                {
                    return null;
                }
                else
                {
                    return new Regex(this.objFilterTB.Text, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }
            }
        }

        private void Analzye()
        {
            var connector = this.MakeDatabaseConnector();
            if (PathsAreCorrect())
            {
                Task.Run(() =>
                {
                    try
                    {
                        this.ToOutput("Synchronizing schema.");
                        using (var db = connector())
                        {
                            DisplayDelta(db.Analyze(this.databaseTB.Text, ObjectFilter));
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
                FilterScripts();
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

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
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
                this.filterTypesCBL.CheckedItems.Cast<ScriptType>().ToArray());

            if (this.sessions.ContainsKey(sesh.Name))
            {
                this.sessions[sesh.Name] = sesh;
            }
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
            var lines = this.sessions.Values.Select(v => v.ToString()).ToArray();
            File.WriteAllLines(SESSIONS_FILENAME, lines);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
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
                    this.createExtensionsBtn.Visible = false;
                    var st = this.CurrentSession.ScriptTypes ?? new ScriptType[] { };
                    for (int i = 0; i < this.filterTypesCBL.Items.Count; ++i)
                    {
                        this.filterTypesCBL.SetItemChecked(i, st.Contains((ScriptType)this.filterTypesCBL.Items[i]));
                    }
                    this.txtStdOut.Text = "";
                    this.txtStdErr.Text = "";
                    if (sessionName != DEFAULT_SESSION_NAME)
                    {
                        this.Analzye();
                    }
                }

                saveToolStripMenuItem.Enabled
                    = deleteToolStripMenuItem.Enabled
                    = (sessionName != DEFAULT_SESSION_NAME);
            }
        }

        private void savedSessionList_TextUpdate(object sender, EventArgs e)
        {
            var sessionName = savedSessionList.Text.Trim();
            if (sessionName.Length == 0)
                sessionName = DEFAULT_SESSION_NAME;
            saveToolStripMenuItem.Enabled
                = deleteToolStripMenuItem.Enabled
                = (sessionName != DEFAULT_SESSION_NAME);

            if (sessionName == DEFAULT_SESSION_NAME)
                savedSessionList.SelectedItem = DEFAULT_SESSION_NAME;
        }

        private void scriptGV_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            Task.Run(() =>
            {
                this.SyncUI(() =>
                {
                    pendingScriptsGV.Enabled = false;
                    WithErrorCapture(() =>
                    {
                        var col = pendingScriptScriptColumn.Index;
                        var row = pendingScriptsGV.Rows[e.RowIndex];
                        var scriptCell = row.Cells[col];
                        var script = (string)scriptCell.Value;
                        Application.DoEvents();
                        if (e.ColumnIndex == pendingScriptNameColumn.Index)
                        {
                            scriptCell.Value = viewScript.Prompt(script);
                        }
                        else if (e.ColumnIndex == pendingScriptRunButtonColumn.Index)
                        {
                            RunScript((ScriptStatus)row.DataBoundItem);
                            pendingScriptsGV.Rows.RemoveAt(e.RowIndex);
                        }
                    }, (exp) =>
                    {
                        this.tabControl1.SelectedTab = this.tabStdErr;
                        return string.Format("{0}: {1}", exp.GetType().Name, exp.Message);
                    });
                    pendingScriptsGV.Enabled = true;
                });
            });
        }

        private void RunScript(ScriptStatus script)
        {
            this.tabControl1.SelectedTab = this.tabStdOut;
            this.ToOutput(script.Script);
            if (script.ScriptType == ScriptType.CreateCatalogue)
            {
                this.RunQuery(script.Script, null, false, (s) => ToOutput(s ? "Success" : "Failure"));
            }
            else
            {
                var connector = this.MakeDatabaseConnector();
                using (var db = connector())
                {
                    db.AlterDatabase(script);
                }
            }
        }

        private void optionsBtn_Click(object sender, EventArgs e)
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


        private void FilterScripts(ItemCheckEventArgs e = null)
        {
            var types = this.filterTypesCBL.CheckedItems.Cast<ScriptType>().ToList();
            if (e != null)
            {
                if (e.NewValue == CheckState.Checked)
                {
                    types.Add((ScriptType)this.filterTypesCBL.Items[e.Index]);
                }
                else
                {
                    types.Remove((ScriptType)this.filterTypesCBL.Items[e.Index]);
                }
            }
            foreach (DataGridViewRow row in pendingScriptsGV.Rows)
            {
                var script = (ScriptStatus)row.DataBoundItem;
                script.Run = types.Contains(script.ScriptType);
            }
        }

        private void generalScriptTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                var script = this.generalScriptTB.Text;
                if (this.generalScriptTB.SelectionLength > 0)
                {
                    script = script.Substring(this.generalScriptTB.SelectionStart, this.generalScriptTB.SelectionLength);
                }
                WithErrorCapture(() => this.RunQuery(script, DatabaseName, false, (succeeded) =>
                {
                    if (succeeded)
                    {
                        this.ToOutput("Success");
                    }
                    else
                    {
                        this.ToError("Something failed");
                    }
                }));
                e.SuppressKeyPress = true;
            }
        }

        private bool WithErrorCapture(Action act, Func<Exception, string> errorHandler = null)
        {
            if (errorHandler == null)
            {
                errorHandler = (exp) => exp.Message;
            }
            try
            {
                act();
                this.ToOutput("Success!");
                return true;
            }
            catch (Exception exp)
            {
                this.ToError(errorHandler(exp));
                return false;
            }
        }

        private void filterTypesCBL_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            FilterScripts(e);
        }

        private void selectAllFiltersCB_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < this.filterTypesCBL.Items.Count; ++i)
            {
                this.filterTypesCBL.SetItemChecked(i, this.selectAllFiltersCB.Checked);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("InitDB for SqlSiphon by Sean T. McBeth (sean@seanmcbeth.com)");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
