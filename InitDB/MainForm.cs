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
        public const string DEFAULT_SESSION_NAME = "<none>";
        private const string SESSIONS_FILENAME = "sessions.dat";
        private const string OPTIONS_FILENAME = "options.dat";
        private const string SQLCMD_PATH_KEY = "SQLCMDPATH";
        private const string PSQL_PATH_KEY = "PGSQLPATH";
        private const string OBJECT_FILTER_KEY = "OBJECTFILTER";
        private const string HORIZONTAL_LINE = "================================================================================";
        private const string DEFAULT_SQLCMD_PATH = @"C:\Program Files\Microsoft SQL Server\110\Tools\Binn\sqlcmd.exe";
        private const string DEFAULT_PSQL_PATH = @"C:\Program Files\PostgreSQL\9.3\bin\psql.exe";
        private const string DEFAULT_OBJECT_FILTER = @"^(vw_)?aspnet_";

        private static Type[] CONNECTION_TYPES = new Type[]
        {
            typeof(SqlSiphon.SqlServer.SqlServerDataConnectorFactory),
            typeof(SqlSiphon.Postgres.PostgresDataConnectorFactory),
        };

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
        private OptionsDialog optionsDialog = new OptionsDialog();

        public MainForm()
        {
            InitializeComponent();
            this.databaseTypeList.DataSource = MainForm.CONNECTION_TYPES;
            this.pendingScriptsGV.AutoGenerateColumns = false;
            this.initialScriptsGV.AutoGenerateColumns = false;
            this.finalScriptsGV.AutoGenerateColumns = false;
            foreach (ScriptType value in Enum.GetValues(typeof(ScriptType)))
            {
                if (value != ScriptType.None)
                {
                    this.filterTypesCBL.Items.Add(value);
                }
            }
            this.Icon = Properties.Resources.InitDBLogo;
            this.statusRTB.Text = string.Empty;
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            this.Text += " v" + version.ToString(4);
            LoadSessions();
            LoadOptions();
            this.optionsDialog.BrowsePSQLPathClick += optionsDialog_BrowsePSQLPathClick;
            this.optionsDialog.BrowseSQLCMDPathClick += optionsDialog_BrowseSQLCMDPathClick;
        }

        public DataConnector MakeDatabaseConnection()
        {
            var constructorParams = new[] { 
                typeof(IDataConnectorFactory),
                typeof(string), 
                typeof(string), 
                typeof(string), 
                typeof(string) 
            };
            if (File.Exists(this.assemblyTB.Text))
            {
                var ss = typeof(IDataConnector);
                var assembly = System.Reflection.Assembly.LoadFrom(this.assemblyTB.Text);
                var candidateTypes = assembly.GetTypes()
                    .Where(t => t.GetInterfaces().Contains(ss)
                                && !t.IsAbstract
                                && !t.IsInterface).ToArray();
                ConstructorInfo constructor = null;
                object[] constructorArgs = null;
                foreach (var type in candidateTypes)
                {
                    SyncUI(() =>
                    {
                        var factoryType = (Type)this.databaseTypeList.SelectedValue;
                        var factoryConstructor = factoryType.GetConstructor(System.Type.EmptyTypes);
                        var factory = (IDataConnectorFactory)factoryConstructor.Invoke(System.Type.EmptyTypes);
                        constructorArgs = new object[] { factory, this.serverTB.Text, this.databaseTB.Text, this.adminUserTB.Text, this.adminPassTB.Text };
                        constructor = type.GetConstructor(constructorParams);
                    });

                    if (constructor != null && constructorArgs != null)
                    {
                        this.CurrentDataAccessLayerType = type;
                        return ((DataConnector)constructor.Invoke(constructorArgs));
                    }
                }
            }
            throw new Exception(string.Format("Couldn't find any types with a constructor that takes the {0} expected parameter types ({1}).", constructorParams.Length, string.Join(", ", constructorParams.Select(p => p.FullName))));
        }

        private void SyncUI(Action act)
        {
            if (this.InvokeRequired)
                this.Invoke(act);
            else act();
        }

        private void Dump(string txt, bool isError, bool modal)
        {
            this.SyncUI(() =>
            {
                if (!txt.EndsWith("..."))
                {
                    txt += Environment.NewLine;
                }

                txt = txt.Replace("\r\n", "\n");

                statusRTB.AppendText(txt);
                statusRTB.SelectionStart = statusRTB.TextLength - txt.Length;
                statusRTB.SelectionLength = txt.Length;
                statusRTB.SelectionColor = isError ? System.Drawing.Color.Salmon : System.Drawing.Color.Aquamarine;
                statusRTB.SelectionLength = 0;
                statusRTB.SelectionStart = statusRTB.TextLength;
                statusRTB.ScrollToCaret();
                toolStripStatusLabel1.Text = txt.Substring(0, Math.Min(txt.Length, 100)).Trim();

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
                this.Dump(txt, true, modal);
                this.SyncUI(() =>
                {
                    this.tabControl1.SelectedTab = this.tabStatus;
                    this.runToolStripMenuItem.Enabled = true;
                });
            }
        }

        private void ToOutput(string txt, bool modal = false)
        {
            this.Dump(txt, false, modal);
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

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.runToolStripMenuItem.Enabled = false;
            Application.DoEvents();
            if (PathsAreCorrect())
            {
                tabControl1.SelectedTab = tabStatus;
                Task.Run(new Action(SetupDB));
            }
        }

        private void SetupDB()
        {
            WithErrorCapture(() =>
            {
                bool succeeded = false;
                using (var db = this.MakeDatabaseConnection())
                {
                    var ss = db.GetSqlSiphon();
                    var delta = CreateDelta(ss);
                    DisplayDelta(delta);
                    var t = db.GetType();
                    this.ToOutput(string.Format("Syncing {0}.{1}", t.Namespace, t.Name));

                    succeeded = RunScripts(delta.Scripts, ss);
                    try
                    {
                        if (delta.PostExecute != null)
                        {
                            foreach (var post in delta.PostExecute)
                            {
                                post(db);
                            }
                        }
                    }
                    catch (Exception exp)
                    {
                        succeeded = false;
                        this.ToError(exp);
                    }

                    if (succeeded)
                    {
                        this.ToOutput("All done", true);
                    }
                    else
                    {
                        this.ToError("There was an error. Rerun in debug mode and step through the program in the debugger.", true);
                    }

                    delta = CreateDelta(ss);
                    DisplayDelta(delta);
                    this.SyncUI(() => runToolStripMenuItem.Enabled = true);
                }
                return succeeded;
            });
        }

        private bool PathsAreCorrect()
        {
            var sqlcmdGood = File.Exists(this.optionsDialog.SQLCMDPath);
            var psqlGood = File.Exists(this.optionsDialog.PSQLPath);
            var assemblyGood = File.Exists(assemblyTB.Text);
            if (!psqlGood)
                this.ToError("Can't find PSQL");
            if (!sqlcmdGood)
                this.ToError("Can't find SQLCMD");
            if (!assemblyGood)
                this.ToError("Can't find Assembly");
            return psqlGood && sqlcmdGood && assemblyGood;
        }

        private bool RunScripts(IEnumerable<ScriptStatus> scripts, ISqlSiphon db)
        {
            var succeeded = true;
            foreach (var script in scripts)
            {
                if (script.Run)
                {
                    succeeded = WithErrorCapture(() => RunScript(script, false, db)) && succeeded;
                }
            };
            return succeeded;
        }

        private bool RunScript(ScriptStatus script, bool selectTab, ISqlSiphon db)
        {
            bool succeeded = true;
            if (selectTab)
            {
                this.tabControl1.SelectedTab = this.tabStatus;
            }

            this.ToOutput(HORIZONTAL_LINE);
            this.ToOutput(string.Format("{0} {1}...", script.ScriptType, script.Name));

            if (script.ScriptType == ScriptType.CreateCatalogue
                || script.ScriptType == ScriptType.CreateDatabaseLogin
                || script.ScriptType == ScriptType.InstallExtension)
            {

                var db_OnStandardError = new IOEventHandler((sender, args) =>
                {
                    succeeded = false;
                    this.ToError(args.Text);
                });

                db.OnStandardError += db_OnStandardError;
                db.OnStandardOutput += db_OnStandardOutput;
                succeeded = db.RunCommandLine(
                    GetExecutable(db),
                    System.Windows.Forms.Application.UserAppDataPath,
                    this.serverTB.Text,
                    script.ScriptType == ScriptType.InstallExtension ? this.databaseTB.Text : null,
                    this.adminUserTB.Text,
                    this.adminPassTB.Text,
                    script.Script);
                db.OnStandardOutput -= db_OnStandardOutput;
                db.OnStandardError -= db_OnStandardError;

                if (succeeded && script.ScriptType == ScriptType.CreateDatabaseLogin)
                {
                    // wait a beat for the database to catch up.
                    System.Threading.Thread.Sleep(5000);
                }
            }
            else
            {
                db.AlterDatabase(script);
            }

            this.ToOutput(succeeded ? "succeeded!" : "failed.");

            return succeeded;
        }

        void db_OnStandardOutput(object sender, IOEventArgs args)
        {
            this.ToOutput(args.Text);
        }

        private Type CurrentDataAccessLayerType;

        private DatabaseDelta CreateDelta(ISqlSiphon db)
        {
            var r = ObjectFilter;
            var initial = db.GetInitialState(this.databaseTB.Text, r);
            var final = db.GetFinalState(this.CurrentDataAccessLayerType, this.sqlUserTB.Text, this.sqlPassTB.Text);
            return final.Diff(initial, db, db);
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

        private void Analyze()
        {
            if (PathsAreCorrect())
            {
                Task.Run(() =>
                {
                    try
                    {
                        this.ToOutput("Synchronizing schema.");
                        using (var db = this.MakeDatabaseConnection().GetSqlSiphon())
                        {
                            DisplayDelta(CreateDelta(db));
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
                var list = new BindingList<ScriptStatus>(delta.Scripts);
                this.pendingScriptsGV.DataSource = list;
                this.initialScriptsGV.DataSource = delta.Initial;
                this.finalScriptsGV.DataSource = delta.Final;
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
                this.databaseTypeList.SelectedIndex,
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
            this.Analyze();
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
            if (sessionName != null
                && this.sessions.ContainsKey(sessionName)
                && MessageBox.Show(
                    string.Format("Are you sure you want to delete session \"{0}\"?", sessionName),
                    "Confirm delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
            {
                this.sessions.Remove(sessionName);
                this.names.Remove(sessionName);
                this.SaveSessions();
            }
        }

        private void savedSessionList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var sessionName = savedSessionList.SelectedItem as string;
            if (sessionName != null)
            {
                sessionToolStripMenuItem.Enabled = savedSessionList.SelectedIndex > 0;
                if (this.CurrentSession != null)
                {
                    this.serverTB.Text = this.CurrentSession.Server;
                    this.databaseTB.Text = this.CurrentSession.DBName;
                    this.adminUserTB.Text = this.CurrentSession.AdminName;
                    this.adminPassTB.Text = this.CurrentSession.AdminPassword;
                    this.sqlUserTB.Text = this.CurrentSession.LoginName;
                    this.sqlPassTB.Text = this.CurrentSession.LoginPassword;
                    this.assemblyTB.Text = this.CurrentSession.AssemblyFile;
                    this.objFilterTB.Text = this.CurrentSession.ObjectFilter ?? this.optionsDialog.DefaultObjectFilterRegexText;
                    this.databaseTypeList.SelectedIndex = this.CurrentSession.DatabaseTypeIndex;
                    var st = this.CurrentSession.ScriptTypes ?? new ScriptType[] { };
                    for (int i = 0; i < this.filterTypesCBL.Items.Count; ++i)
                    {
                        this.filterTypesCBL.SetItemChecked(i, st.Contains((ScriptType)this.filterTypesCBL.Items[i]));
                    }
                    this.statusRTB.Text = "";
                    tabControl1.SelectedTab = this.tabScripts;
                    this.pendingScriptsGV.DataSource = null;
                    if (sessionName != DEFAULT_SESSION_NAME)
                    {
                        this.Analyze();
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
            var gv = sender as DataGridView;
            if (gv != null)
            {
                gv.Enabled = false;
                WithErrorCapture(() =>
                {
                    var row = gv.Rows[e.RowIndex];
                    var scriptObject = (ScriptStatus)row.DataBoundItem;
                    var selectedCell = row.Cells[e.ColumnIndex];
                    var stringValue = selectedCell.Value as string;
                    bool succeeded = true;
                    Application.DoEvents();
                    if (stringValue == scriptObject.Script)
                    {
                        var sv = new ScriptView();
                        if (gv == pendingScriptsGV)
                        {
                            sv.Prompt(scriptObject.Script, s => selectedCell.Value = s);
                        }
                        else
                        {
                            sv.Prompt(scriptObject.Script);
                        }
                        // forms get disposed when they get closed, no need to dispose them here.
                    }
                    else if (gv == pendingScriptsGV && stringValue == "run")
                    {
                        using (var db = this.MakeDatabaseConnection())
                        {
                            succeeded = RunScript(scriptObject, true, db.GetSqlSiphon());
                        }
                        gv.Rows.RemoveAt(e.RowIndex);
                    }
                    else if (gv == pendingScriptsGV && stringValue == "skip")
                    {
                        using (var db = this.MakeDatabaseConnection())
                        {
                            db.GetSqlSiphon().MarkScriptAsRan(scriptObject);
                        }
                        gv.Rows.RemoveAt(e.RowIndex);
                    }
                    return succeeded;
                }, (exp) =>
                {
                    this.tabControl1.SelectedTab = this.tabStatus;
                    return string.Format("{0}: {1}", exp.GetType().Name, exp.Message);
                });
                gv.Enabled = true;
            }
        }

        private void optionsBtn_Click(object sender, EventArgs e)
        {
            if (this.optionsDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.options[SQLCMD_PATH_KEY] = this.optionsDialog.SQLCMDPath;
                this.options[PSQL_PATH_KEY] = this.optionsDialog.PSQLPath;
                this.options[OBJECT_FILTER_KEY] = this.optionsDialog.DefaultObjectFilterRegexText;
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
                e.SuppressKeyPress = true;
                var script = this.generalScriptTB.Text;
                if (this.generalScriptTB.SelectionLength > 0)
                {
                    script = script.Substring(this.generalScriptTB.SelectionStart, this.generalScriptTB.SelectionLength);
                }
                var scriptObj = new ScriptStatus(ScriptType.InstallExtension, "none", script);

                using (var db = this.MakeDatabaseConnection().GetSqlSiphon())
                {
                    this.RunScript(scriptObj, true, db);
                }
            }
        }

        private string GetExecutable(IDataConnector connector)
        {
            if (connector.DatabaseType == SqlSiphon.Postgres.PostgresDataAccessLayer.DATABASE_TYPE_NAME)
            {
                return this.optionsDialog.PSQLPath;
            }
            else if (connector.DatabaseType == SqlSiphon.SqlServer.SqlServerDataAccessLayer.DATABASE_TYPE_NAME)
            {
                return this.optionsDialog.SQLCMDPath;
            }
            return null;
        }

        private bool WithErrorCapture(Func<bool> act, Func<Exception, string> errorHandler = null)
        {
            if (errorHandler == null)
            {
                errorHandler = (exp) => exp.Message;
            }
            try
            {
                return act();
            }
            catch (Exception exp)
            {
                this.ToOutput("failed");
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
            MessageBox.Show(string.Format(
@"               InitDB (v{0})
           for SqlSiphon (v{1})

by Sean T. McBeth (v1) (sean@seanmcbeth.com)",
                Assembly.GetExecutingAssembly().GetName().Version,
                typeof(ISqlSiphon).Assembly.GetName().Version));
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void analyzeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Analyze();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (var db = this.MakeDatabaseConnection().GetSqlSiphon())
                {
                    var delta = this.CreateDelta(db);
                    var sb = new System.Text.StringBuilder();
                    foreach (var script in delta.Scripts)
                    {
                        sb.AppendLine(script.Script);
                        sb.AppendLine("GO");
                    }
                    File.WriteAllText(this.saveFileDialog1.FileName, sb.ToString());
                }
            }
        }

        private void generateCodeBTN_Click(object sender, EventArgs e)
        {
            using (var db = this.MakeDatabaseConnection().GetSqlSiphon())
            {
                var initial = db.GetInitialState(this.databaseTB.Text, ObjectFilter);
                initial.WriteCodeFiles(@"D:\\Sean\\Desktop", "TestProject.Data", "TestConnector");
            }
        }
    }
}
