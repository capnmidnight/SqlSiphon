using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using SqlSiphon;

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

        private static readonly Type[] CONNECTION_TYPES = new Type[]
        {
            typeof(SqlSiphon.SqlServer.SqlServerDataConnectorFactory),
            typeof(SqlSiphon.Postgres.PostgresDataConnectorFactory)
        };

        private static string UnrollStackTrace(Exception e)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendFormat(@"<stacktrace timestamp=""{0}"">
", DateTime.Now);
            var exp = e;
            for (var i = 0; exp != null; ++i)
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
        private readonly OptionsDialog optionsDialog = new OptionsDialog();

        public MainForm()
        {
            InitializeComponent();
            databaseTypeList.DataSource = MainForm.CONNECTION_TYPES
                .Select(DataConnector.GetDatabaseTypeName)
                .ToArray();
            pendingScriptsGV.AutoGenerateColumns = false;
            initialScriptsGV.AutoGenerateColumns = false;
            finalScriptsGV.AutoGenerateColumns = false;
            foreach (ScriptType value in Enum.GetValues(typeof(ScriptType)))
            {
                if (value != ScriptType.None)
                {
                    filterTypesCBL.Items.Add(value);
                }
            }
            Icon = Properties.Resources.InitDBLogo;
            statusRTB.Text = string.Empty;
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            Text += " v" + version.ToString(4);
            LoadSessions();
            LoadOptions();
            optionsDialog.BrowsePSQLPathClick += optionsDialog_BrowsePSQLPathClick;
            optionsDialog.BrowseSQLCMDPathClick += optionsDialog_BrowseSQLCMDPathClick;
        }

        public DataConnector MakeDatabaseConnection()
        {
            var constructorParams = new[] {
                typeof(IDataConnector)
            };
            if (File.Exists(assemblyTB.Text))
            {
                var ss = typeof(IDataConnector);
                var assembly = System.Reflection.Assembly.LoadFrom(assemblyTB.Text);
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
                        var factoryType = CONNECTION_TYPES[databaseTypeList.SelectedIndex];
                        var factoryConstructor = factoryType.GetConstructor(System.Type.EmptyTypes);
                        var factory = (IDataConnectorFactory)factoryConstructor.Invoke(System.Type.EmptyTypes);
                        var connector = factory.MakeConnector(serverTB.Text, databaseTB.Text, adminUserTB.Text, adminPassTB.Text);
                        constructorArgs = new object[] { connector };
                        constructor = type.GetConstructor(constructorParams);
                    });

                    if (constructor != null && constructorArgs != null)
                    {
                        CurrentDataAccessLayerType = type;
                        return ((DataConnector)constructor.Invoke(constructorArgs));
                    }
                }
            }
            throw new Exception(string.Format("Couldn't find any types with a constructor that takes the {0} expected parameter types ({1}).", constructorParams.Length, string.Join(", ", constructorParams.Select(p => p.FullName))));
        }

        private void SyncUI(Action act)
        {
            if (InvokeRequired)
            {
                Invoke(act);
            }
            else
            {
                act();
            }
        }

        private void Dump(string txt, bool isError, bool modal)
        {
            SyncUI(() =>
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
                ToError(UnrollStackTrace(exp), modal);
            }
        }

        private void ToError(string txt, bool modal = false)
        {
            if (txt != null)
            {
                Dump(txt, true, modal);
                SyncUI(() =>
                {
                    tabControl1.SelectedTab = tabStatus;
                    runToolStripMenuItem.Enabled = true;
                });
            }
        }

        private void ToOutput(string txt, bool modal = false)
        {
            Dump(txt, false, modal);
        }

        private void LoadOptions()
        {
            if (File.Exists(OPTIONS_FILENAME))
            {
                options = File.ReadAllLines(OPTIONS_FILENAME)
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0)
                    .Select(l => l.Split('='))
                    .Where(p => p.Length == 2)
                    .ToDictionary(p => p[0], p => p[1]);
            }
            else
            {
                options = new Dictionary<string, string>();
            }

            DisplayOptions();
        }

        private string CoalesceOption(string key, string defaultValue)
        {
            if (!options.ContainsKey(key))
            {
                options.Add(key, defaultValue);
            }
            return options[key];
        }

        private void DisplayOptions()
        {
            optionsDialog.SQLCMDPath = CoalesceOption(SQLCMD_PATH_KEY, DEFAULT_SQLCMD_PATH);
            optionsDialog.PSQLPath = CoalesceOption(PSQL_PATH_KEY, DEFAULT_PSQL_PATH);
            optionsDialog.DefaultObjectFilterRegexText = CoalesceOption(OBJECT_FILTER_KEY, DEFAULT_OBJECT_FILTER);
        }

        private void LoadSessions()
        {
            if (File.Exists(SESSIONS_FILENAME))
            {
                sessions = File.ReadAllLines(SESSIONS_FILENAME)
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
            }
            else
            {
                sessions = new Dictionary<string, Session>();
            }
            if (!sessions.ContainsKey(DEFAULT_SESSION_NAME))
            {
                sessions.Add(DEFAULT_SESSION_NAME, new Session());
            }

            names = new BindingList<string>(sessions.Keys.OrderBy(k => k).ToList());
            savedSessionList.DataSource = names;
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            runToolStripMenuItem.Enabled = false;
            Application.DoEvents();
            if (PathsAreCorrect())
            {
                tabControl1.SelectedTab = tabStatus;
                Task.Run(new Action(SetupDB));
            }
        }

        private void SetupDB()
        {
            try
            {
                WithErrorCapture(() =>
                {
                    var succeeded = false;
                    using (var db = MakeDatabaseConnection())
                    {
                        var ss = db.GetSqlSiphon();
                        var delta = CreateDelta(ss);
                        DisplayDelta(delta);
                        var t = db.GetType();
                        ToOutput(string.Format("Syncing {0}.{1}", t.Namespace, t.Name));

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
                            ToError(exp);
                        }

                        if (succeeded)
                        {
                            ToOutput("All done", true);
                        }
                        else
                        {
                            ToError("There was an error. Rerun in debug mode and step through the program in the debugger.", true);
                        }

                        delta = CreateDelta(ss);
                        DisplayDelta(delta);
                        SyncUI(() => runToolStripMenuItem.Enabled = true);
                    }
                    return succeeded;
                });
            }
            catch (ConnectionFailedException exp)
            {
                ToOutput("failed");
                ToError(exp.Message);
            }
        }

        private bool PathsAreCorrect()
        {
            var sqlcmdGood = File.Exists(optionsDialog.SQLCMDPath);
            var psqlGood = File.Exists(optionsDialog.PSQLPath);
            var assemblyGood = File.Exists(assemblyTB.Text);
            if (!psqlGood)
            {
                ToError("Can't find PSQL");
            }

            if (!sqlcmdGood)
            {
                ToError("Can't find SQLCMD");
            }

            if (!assemblyGood)
            {
                ToError("Can't find Assembly");
            }

            return psqlGood && sqlcmdGood && assemblyGood;
        }

        private bool RunScripts(IEnumerable<ScriptStatus> scripts, ISqlSiphon db)
        {
            var succeeded = true;
            try
            {
                foreach (var script in scripts)
                {
                    if (script.Run)
                    {
                        succeeded = WithErrorCapture(() => RunScript(script, false, db)) && succeeded;
                    }
                }
            }
            catch (ConnectionFailedException exp)
            {
                ToOutput("failed");
                ToError(exp.Message);
                succeeded = false;
            }
            return succeeded;
        }

        private bool RunScript(ScriptStatus script, bool selectTab, ISqlSiphon db)
        {
            var succeeded = true;
            if (selectTab)
            {
                tabControl1.SelectedTab = tabStatus;
            }

            ToOutput(HORIZONTAL_LINE);
            ToOutput(string.Format("{0} {1}...", script.ScriptType, script.Name));

            if (script.ScriptType == ScriptType.CreateCatalogue
                || script.ScriptType == ScriptType.CreateDatabaseLogin
                || script.ScriptType == ScriptType.InstallExtension)
            {

                var db_OnStandardError = new IOEventHandler((sender, args) =>
                {
                    succeeded = false;
                    ToError(args.Text);
                });

                db.OnStandardError += db_OnStandardError;
                db.OnStandardOutput += db_OnStandardOutput;
                succeeded = db.RunCommandLine(
                    GetExecutable(db),
                    System.Windows.Forms.Application.UserAppDataPath,
                    serverTB.Text,
                    script.ScriptType == ScriptType.InstallExtension ? databaseTB.Text : null,
                    adminUserTB.Text,
                    adminPassTB.Text,
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

            ToOutput(succeeded ? "succeeded!" : "failed.");

            return succeeded;
        }

        private void db_OnStandardOutput(object sender, IOEventArgs args)
        {
            ToOutput(args.Text);
        }

        private Type CurrentDataAccessLayerType;

        private DatabaseDelta CreateDelta(ISqlSiphon db)
        {
            var r = ObjectFilter;
            var initial = db.GetInitialState(databaseTB.Text, r);
            var final = db.GetFinalState(CurrentDataAccessLayerType, sqlUserTB.Text, sqlPassTB.Text);
            return final.Diff(initial, db, db);
        }

        private Regex ObjectFilter
        {
            get
            {
                if (string.IsNullOrWhiteSpace(objFilterTB.Text))
                {
                    return null;
                }
                else
                {
                    return new Regex(objFilterTB.Text, RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
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
                        ToOutput("Synchronizing schema.");
                        using (var db = MakeDatabaseConnection().GetSqlSiphon())
                        {
                            DisplayDelta(CreateDelta(db));
                        }
                        ToOutput("Schema synched.");
                    }
                    catch (Exception exp)
                    {
                        ToError(exp);
                    }
                });
            }
        }

        private void DisplayDelta(DatabaseDelta delta)
        {
            SyncUI(() =>
            {
                var list = new BindingList<ScriptStatus>(delta.Scripts);
                pendingScriptsGV.DataSource = list;
                initialScriptsGV.DataSource = delta.Initial;
                finalScriptsGV.DataSource = delta.Final;
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

        private void optionsDialog_BrowseSQLCMDPathClick(object sender, EventArgs e)
        {
            optionsDialog.SQLCMDPath = BrowseFrom(optionsDialog.SQLCMDPath) ?? optionsDialog.SQLCMDPath;
        }

        private void optionsDialog_BrowsePSQLPathClick(object sender, EventArgs e)
        {
            optionsDialog.PSQLPath = BrowseFrom(optionsDialog.PSQLPath) ?? optionsDialog.PSQLPath;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sesh = new Session(
                savedSessionList.Text,
                serverTB.Text,
                databaseTB.Text,
                adminUserTB.Text,
                adminPassTB.Text,
                sqlUserTB.Text,
                sqlPassTB.Text,
                assemblyTB.Text,
                objFilterTB.Text,
                databaseTypeList.SelectedIndex,
                filterTypesCBL.CheckedItems.Cast<ScriptType>().ToArray());

            if (sessions.ContainsKey(sesh.Name))
            {
                sessions[sesh.Name] = sesh;
            }
            else
            {
                sessions.Add(sesh.Name, sesh);
                names.Add(sesh.Name);
                savedSessionList.SelectedItem = sesh.Name;
            }

            SaveSessions();
            Analyze();
        }

        private Session CurrentSession
        {
            get
            {
                Session value = null;
                SyncUI(() =>
                {
                    if (sessions.ContainsKey(savedSessionList.Text))
                    {
                        value = sessions[savedSessionList.Text];
                    }
                });
                return value;
            }
        }

        private void SaveSessions()
        {
            var lines = sessions.Values.Select(v => v.ToString()).ToArray();
            File.WriteAllLines(SESSIONS_FILENAME, lines);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sessionName = savedSessionList.SelectedItem as string;
            if (sessionName != null
                && sessions.ContainsKey(sessionName)
                && MessageBox.Show(
                    string.Format("Are you sure you want to delete session \"{0}\"?", sessionName),
                    "Confirm delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
            {
                sessions.Remove(sessionName);
                names.Remove(sessionName);
                SaveSessions();
            }
        }

        private void savedSessionList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var sessionName = savedSessionList.SelectedItem as string;
            if (sessionName != null)
            {
                sessionToolStripMenuItem.Enabled = savedSessionList.SelectedIndex > 0;
                if (CurrentSession != null)
                {
                    serverTB.Text = CurrentSession.Server;
                    databaseTB.Text = CurrentSession.DBName;
                    adminUserTB.Text = CurrentSession.AdminName;
                    adminPassTB.Text = CurrentSession.AdminPassword;
                    sqlUserTB.Text = CurrentSession.LoginName;
                    sqlPassTB.Text = CurrentSession.LoginPassword;
                    assemblyTB.Text = CurrentSession.AssemblyFile;
                    objFilterTB.Text = CurrentSession.ObjectFilter ?? optionsDialog.DefaultObjectFilterRegexText;
                    databaseTypeList.SelectedIndex = CurrentSession.DatabaseTypeIndex;
                    var st = CurrentSession.ScriptTypes ?? new ScriptType[] { };
                    for (var i = 0; i < filterTypesCBL.Items.Count; ++i)
                    {
                        filterTypesCBL.SetItemChecked(i, st.Contains((ScriptType)filterTypesCBL.Items[i]));
                    }
                    statusRTB.Text = "";
                    tabControl1.SelectedTab = tabScripts;
                    pendingScriptsGV.DataSource = null;
                    if (sessionName != DEFAULT_SESSION_NAME)
                    {
                        Analyze();
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
            {
                sessionName = DEFAULT_SESSION_NAME;
            }

            saveToolStripMenuItem.Enabled
                = deleteToolStripMenuItem.Enabled
                = (sessionName != DEFAULT_SESSION_NAME);

            if (sessionName == DEFAULT_SESSION_NAME)
            {
                savedSessionList.SelectedItem = DEFAULT_SESSION_NAME;
            }
        }

        private void scriptGV_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var gv = sender as DataGridView;
            if (gv != null)
            {
                gv.Enabled = false;
                try
                {
                    WithErrorCapture(() =>
                    {
                        var row = gv.Rows[e.RowIndex];
                        var scriptObject = (ScriptStatus)row.DataBoundItem;
                        var selectedCell = row.Cells[e.ColumnIndex];
                        var stringValue = selectedCell.Value as string;
                        var succeeded = true;
                        Application.DoEvents();
                        if (stringValue == scriptObject.Script)
                        {
                            var sv = new ScriptView();
                            if (gv == pendingScriptsGV)
                            {
                                sv.Prompt(scriptObject.Name, scriptObject.Script, s => selectedCell.Value = s);
                            }
                            else
                            {
                                sv.Prompt(scriptObject.Name, scriptObject.Script);
                            }
                            // forms get disposed when they get closed, no need to dispose them here.
                        }
                        else if (gv == pendingScriptsGV && stringValue == "run")
                        {
                            using (var db = MakeDatabaseConnection())
                            {
                                succeeded = RunScript(scriptObject, true, db.GetSqlSiphon());
                            }
                            gv.Rows.RemoveAt(e.RowIndex);
                        }
                        else if (gv == pendingScriptsGV && stringValue == "skip")
                        {
                            using (var db = MakeDatabaseConnection())
                            {
                                db.GetSqlSiphon().MarkScriptAsRan(scriptObject);
                            }
                            gv.Rows.RemoveAt(e.RowIndex);
                        }
                        return succeeded;
                    }, (exp) =>
                    {
                        tabControl1.SelectedTab = tabStatus;
                        return string.Format("{0}: {1}", exp.GetType().Name, exp.Message);
                    });
                }
                catch (ConnectionFailedException exp)
                {
                    ToOutput("failed");
                    ToError(exp.Message);
                }
                finally
                {
                    gv.Enabled = true;
                }
            }
        }

        private void optionsBtn_Click(object sender, EventArgs e)
        {
            if (optionsDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                options[SQLCMD_PATH_KEY] = optionsDialog.SQLCMDPath;
                options[PSQL_PATH_KEY] = optionsDialog.PSQLPath;
                options[OBJECT_FILTER_KEY] = optionsDialog.DefaultObjectFilterRegexText;
                File.WriteAllLines(OPTIONS_FILENAME,
                    options.Select(kv => string.Join("=", kv.Key, kv.Value)).ToArray());
            }
            else
            {
                DisplayOptions();
            }
        }


        private void FilterScripts(ItemCheckEventArgs e = null)
        {
            var types = filterTypesCBL.CheckedItems.Cast<ScriptType>().ToList();
            if (e != null)
            {
                if (e.NewValue == CheckState.Checked)
                {
                    types.Add((ScriptType)filterTypesCBL.Items[e.Index]);
                }
                else
                {
                    types.Remove((ScriptType)filterTypesCBL.Items[e.Index]);
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
                var script = generalScriptTB.Text;
                if (generalScriptTB.SelectionLength > 0)
                {
                    script = script.Substring(generalScriptTB.SelectionStart, generalScriptTB.SelectionLength);
                }
                var scriptObj = new ScriptStatus(ScriptType.InstallExtension, "none", script, null);

                using (var db = MakeDatabaseConnection().GetSqlSiphon())
                {
                    RunScript(scriptObj, true, db);
                }
            }
        }

        private string GetExecutable(IDataConnector connector)
        {
            if (connector is SqlSiphon.Postgres.PostgresDataAccessLayer)
            {
                return optionsDialog.PSQLPath;
            }
            else if (connector is SqlSiphon.SqlServer.SqlServerDataAccessLayer)
            {
                return optionsDialog.SQLCMDPath;
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
            catch (ConnectionFailedException)
            {
                throw;
            }
            catch (Exception exp)
            {
                ToOutput("failed");
                ToError(errorHandler(exp));
                return false;
            }
        }

        private void filterTypesCBL_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            FilterScripts(e);
        }

        private void selectAllFiltersCB_CheckedChanged(object sender, EventArgs e)
        {
            for (var i = 0; i < filterTypesCBL.Items.Count; ++i)
            {
                filterTypesCBL.SetItemChecked(i, selectAllFiltersCB.Checked);
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
            Close();
        }

        private void analyzeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Analyze();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (var db = MakeDatabaseConnection().GetSqlSiphon())
                {
                    var delta = CreateDelta(db);
                    var sb = new System.Text.StringBuilder();
                    foreach (var script in delta.Scripts)
                    {
                        sb.AppendLine(script.Script);
                        sb.AppendLine("GO");
                    }
                    File.WriteAllText(saveFileDialog1.FileName, sb.ToString());
                }
            }
        }

        private void generateCodeBTN_Click(object sender, EventArgs e)
        {
            using (var db = MakeDatabaseConnection().GetSqlSiphon())
            {
                var initial = db.GetInitialState(databaseTB.Text, ObjectFilter);
                initial.WriteCodeFiles(@"D:\\Sean\\Desktop", "TestProject.Data", "TestConnector");
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (0 <= Properties.Settings.Default.LastSelectedSession && Properties.Settings.Default.LastSelectedSession < names.Count)
            {
                savedSessionList.SelectedIndex = Properties.Settings.Default.LastSelectedSession;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.LastSelectedSession = savedSessionList.SelectedIndex;
            Properties.Settings.Default.Save();
        }
    }
}
