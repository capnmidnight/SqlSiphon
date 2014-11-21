﻿using SqlSiphon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InitDB
{
    public partial class MainForm : Form
    {
        public static string DEFAULT_SESSION_NAME = "<none>";
        private static string POSTGRES = "PostgreSQL";
        private static string SESSIONS_FILENAME = "sessions.dat";
        private static string OPTIONS_FILENAME = "options.dat";
        private static string SQLCMD_PATH_KEY = "SQLCMDPATH";
        private static string PSQL_PATH_KEY = "PGSQLPATH";
        private static string REG_ASPNET_PATH_KEY = "REGASPNETPATH";
        private static string HORIZONTAL_LINE = "================================================================================";
        private static string DEFAULT_REG_ASPNET_PATH = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\aspnet_regsql.exe";
        private static string DEFAULT_SQLCMD_PATH = @"C:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd.exe";
        private static string DEFAULT_PSQL_PATH = @"C:\Program Files\PostgreSQL\9.3\bin\psql.exe";

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
       
        public ISqlSiphon MakeDatabaseConnection()
        {
            if (File.Exists(this.assemblyTB.Text))
            {
                var ss = typeof(ISqlSiphon);
                var vt = this.IsPostgres()
                    ? typeof(SqlSiphon.Postgres.NpgsqlDataAccessLayer)
                    : typeof(SqlSiphon.SqlServer.SqlServerDataAccessLayer);
                var constructorParams = new[] { typeof(string) };
                var constructorArgs = new object[] { this.MakeConnectionString() };
                var assembly = System.Reflection.Assembly.LoadFrom(this.assemblyTB.Text);
                var constructor = assembly.GetTypes()
                    .Where(t => t.GetInterfaces().Contains(ss)
                                && t.IsSubclassOf(vt)
                                && !t.IsAbstract
                                && !t.IsInterface)
                    .Select(t => t.GetConstructor(constructorParams))
                    .FirstOrDefault();
                if (constructor != null)
                {
                    return (ISqlSiphon)constructor.Invoke(constructorArgs);
                }
            }
            return null;
        }

        private string MakeConnectionString()
        {
            var cred = new[] { 
                new[]{this.adminUserTB.Text, this.adminPassTB.Text}, 
                new[]{this.sqlUserTB.Text, this.sqlPassTB.Text}}
                .Where(s => !string.IsNullOrEmpty(s[0]) && !string.IsNullOrEmpty(s[1]))
                .FirstOrDefault();
            string connStr = null;
            if (this.IsPostgres())
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
                this.options = File.ReadAllLines(OPTIONS_FILENAME)
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0)
                    .Select(l => l.Split('='))
                    .Where(p => p.Length == 2)
                    .ToDictionary(p => p[0], p => p[1]);
            else
                this.options = new Dictionary<string, string>();

            if (!this.options.ContainsKey(SQLCMD_PATH_KEY))
                this.options.Add(SQLCMD_PATH_KEY, DEFAULT_SQLCMD_PATH);

            if (!this.options.ContainsKey(PSQL_PATH_KEY))
                this.options.Add(PSQL_PATH_KEY, DEFAULT_PSQL_PATH);

            if (!this.options.ContainsKey(REG_ASPNET_PATH_KEY))
                this.options.Add(REG_ASPNET_PATH_KEY, DEFAULT_REG_ASPNET_PATH);

            DisplayOptions();
        }

        private void DisplayOptions()
        {
            this.sqlcmdTB.Text = this.options[SQLCMD_PATH_KEY];
            this.regsqlTB.Text = this.options[REG_ASPNET_PATH_KEY];
            this.psqlTB.Text = this.options[PSQL_PATH_KEY];
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

        private bool IsPostgres()
        {
            if (this.InvokeRequired)
            {
                return (bool)this.Invoke(new Func<bool>(this.IsPostgres));
            }
            else
            {
                return ((string)dbTypeList.SelectedItem) == POSTGRES;
            }
        }

        private bool PathsAreCorrect()
        {
            var sqlcmdGood = this.IsPostgres() || File.Exists(sqlcmdTB.Text);
            var psqlGood = !this.IsPostgres() || File.Exists(psqlTB.Text);
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
                proc.ErrorDataReceived += proc_ErrorDataReceived;
                proc.ErrorDataReceived += new DataReceivedEventHandler(delegate(object sender, DataReceivedEventArgs e)
                {
                    succeeded = false;
                });
                proc.OutputDataReceived += proc_OutputDataReceived;
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
            }
            this.ToOutput(string.Format("finished {0}", shortName));
            this.ToOutput(HORIZONTAL_LINE);
            return succeeded;
        }

        private void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.ToOutput(e.Data);
        }

        private void proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.ToError(e.Data);
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
            Func<string, string, bool, bool> runQuery;
            if (this.IsPostgres())
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
                if (this.IsPostgres())
                {
                    dbName = dbName.ToLower();
                }

                if (chkCreateDatabase.Checked)
                {
                    succeeded = runQuery(string.Format("CREATE DATABASE {0};", dbName), null, false);
                    if (succeeded && this.IsPostgres())
                    {
                        succeeded = runQuery("CREATE EXTENSION \\\"uuid-ossp\\\";", dbName, false);
                    }
                }
                if (succeeded && chkCreateLogin.Checked)
                {
                    if (this.IsPostgres())
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

                if (succeeded && !this.IsPostgres() && chkRegSql.Checked)
                {
                    succeeded = RunASPNET_REGSQL();
                }

                if (succeeded)
                {
                    using (var db = this.MakeDatabaseConnection())
                    {
                        db.Progress += db_Progress;
                        var delta = db.Analyze();
                        DisplayDelta(delta);
                        var t = db.GetType();
                        this.ToOutput(string.Format("Syncing {0}.{1}", t.Namespace, t.Name));
                        if (succeeded && chkCreateTables.Checked)
                        {
                            succeeded &= RunScripts("Creating table:", delta.CreateTablesScripts, db);
                            succeeded &= RunScripts("Creating columns:", delta.CreateColumnsScripts, db);
                        }

                        if (succeeded && chkCreateFKs.Checked)
                        {
                            succeeded &= RunScripts("Creating foreign keys:", delta.CreateRelationshipsScripts, db);
                        }

                        if (succeeded && chkCreateIndices.Checked)
                        {
                            succeeded &= RunScripts("Creating indexes:", delta.CreateIndexScripts, db);
                        }

                        if (succeeded && chkSyncProcedures.Checked)
                        {
                            succeeded &= RunScripts("Creating stored procedures:", delta.CreateRoutinesScripts, db);
                            succeeded &= RunScripts("Creating stored procedures:", delta.AlteredRoutinesScripts, db);
                        }

                        if (succeeded && chkInitializeData.Checked)
                        {
                            succeeded &= RunScripts("Syncing data:", delta.OtherScripts, db);
                        }
                        delta = db.Analyze();
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


        private bool RunScripts(string feature, Dictionary<string, string> scripts, ISqlSiphon db)
        {
            foreach (var scriptName in scripts.Keys)
            {
                try
                {
                    this.ToOutput(string.Format("{0} {1}.", feature, scriptName));
                    db.AlterDatabase(scripts[scriptName]);
                }
                catch (Exception exp)
                {
                    this.ToError(exp);
                    return false;
                }
            }
            return true;
        }

        private void db_Progress(object sender, DataProgressEventArgs e)
        {
            this.ToOutput(string.Format("{0} of {1}: {2}", e.CurrentRow + 1, e.RowCount, e.Message));
        }

        private void analyzeButton_Click(object sender, EventArgs e)
        {
            analyzeButton.Enabled = false;
            if (PathsAreCorrect())
            {
                Task.Run(() =>
                {
                    try
                    {
                        this.ToOutput("Synchronizing schema.");
                        using (var db = this.MakeDatabaseConnection())
                        {
                            DisplayDelta(db.Analyze());
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
                FillGV(this.createTablesGV, delta.CreateTablesScripts);
                FillGV(this.dropTablesGV, delta.DropTablesScripts);
                FillGV(this.unalteredTablesGV, delta.UnalteredTablesScripts);
                FillGV(this.createColumnsGV, delta.CreateColumnsScripts);
                FillGV(this.dropColumnsGV, delta.DropColumnsScripts);
                FillGV(this.alteredColumnsGV, delta.AlteredColumnsScripts);
                FillGV(this.unalteredColumnsGV, delta.UnalteredColumnsScripts);
                FillGV(this.createRelationshipsGV, delta.CreateRelationshipsScripts);
                FillGV(this.dropRelationshipsGV, delta.DropRelationshipsScripts);
                FillGV(this.unalteredRelationshipsGV, delta.UnalteredRelationshipsScripts);
                FillGV(this.createRoutinesGV, delta.CreateRoutinesScripts);
                FillGV(this.dropRoutinesGV, delta.DropRoutinesScripts);
                FillGV(this.alteredRoutinesGV, delta.AlteredRoutinesScripts);
                FillGV(this.unalteredRoutinesGV, delta.UnalteredRoutinesScripts);
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
                (string)this.dbTypeList.SelectedItem,
                this.serverTB.Text,
                this.databaseTB.Text,
                this.adminUserTB.Text,
                this.adminPassTB.Text,
                this.sqlUserTB.Text,
                this.sqlPassTB.Text,
                this.assemblyTB.Text,
                this.chkCreateDatabase.Checked,
                this.chkCreateLogin.Checked,
                this.chkRegSql.Checked,
                this.chkCreateTables.Checked,
                this.chkInitializeData.Checked,
                this.chkSyncProcedures.Checked,
                this.chkCreateFKs.Checked,
                this.chkCreateIndices.Checked);

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
                    this.serverTB.Text = this.CurrentSession.Server;
                    this.dbTypeList.SelectedItem = this.CurrentSession.DatabaseType;
                    this.databaseTB.Text = this.CurrentSession.DBName;
                    this.adminUserTB.Text = this.CurrentSession.AdminName;
                    this.adminPassTB.Text = this.CurrentSession.AdminPassword;
                    this.sqlUserTB.Text = this.CurrentSession.LoginName;
                    this.sqlPassTB.Text = this.CurrentSession.LoginPassword;
                    this.assemblyTB.Text = this.CurrentSession.AssemblyFile;
                    this.chkCreateDatabase.Checked = this.CurrentSession.CreateDatabase;
                    this.chkCreateLogin.Checked = this.CurrentSession.CreateLogin;
                    this.chkRegSql.Checked = this.CurrentSession.RegisterASPNETMembership;
                    this.chkCreateTables.Checked = this.CurrentSession.CreateSchemaObjects;
                    this.chkInitializeData.Checked = this.CurrentSession.InitializeData;
                    this.chkSyncProcedures.Checked = this.CurrentSession.SyncStoredProcedures;
                    this.chkCreateFKs.Checked = this.CurrentSession.CreateFKs;
                    this.chkCreateIndices.Checked = this.CurrentSession.CreateIndices;
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
            EnableSaveCancelOptions();
        }

        private void EnableSaveCancelOptions()
        {
            cancelOptionsButton.Enabled =
                this.options[REG_ASPNET_PATH_KEY] != this.regsqlTB.Text
                || this.options[SQLCMD_PATH_KEY] != this.sqlcmdTB.Text
                || this.options[PSQL_PATH_KEY] != this.psqlTB.Text;
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
            using (var db = this.MakeDatabaseConnection())
                db.MarkScriptAsRan(script);
        }

        private void RunScript(string script)
        {
            this.tabControl1.SelectedTab = this.tabStdOut;
            this.ToOutput(script);
            using (var db = this.MakeDatabaseConnection())
                db.AlterDatabase(script);
        }
    }
}