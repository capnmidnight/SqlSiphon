using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Web.Security;
using System.Reflection;
using SqlSiphon;

namespace InitDB
{
    public partial class Form1 : Form
    {
        private static string SETTINGS_FILENAME = "session.dat";
        private static string DEFAULT_SESSION_NAME = "<none>";
        private Dictionary<string, Session> sessions;
        private BindingList<string> names;
        public Form1()
        {
            InitializeComponent();
            this.browseAssemblyBtn.Tag = this.assemblyTB;
            this.browseSqlCmdButton.Tag = this.sqlcmdTB;
            this.browseRegSqlButton.Tag = this.regsqlTB;
            this.txtStdOut.Text = string.Empty;
            this.txtStdErr.Text = string.Empty;
            if (File.Exists(SETTINGS_FILENAME))
                this.sessions = File.ReadAllLines(SETTINGS_FILENAME)
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0)
                    .Select(l => new Session(l))
                    .ToDictionary(s => s.Name);
            else
            {
                this.sessions = new Dictionary<string, Session>();
                this.sessions.Add(DEFAULT_SESSION_NAME, new Session());
            }
            this.names = new BindingList<string>(this.sessions.Keys.ToList());
            this.savedSessionList.DataSource = this.names;
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            this.runButton.Enabled = false;
            Application.DoEvents();
            var sqlcmdGood = File.Exists(sqlcmdTB.Text);
            var regsqlGood = File.Exists(regsqlTB.Text);
            if (!sqlcmdGood)
                this.ToError("Can't find SQLCMD");
            if (!regsqlGood)
                this.ToError("Can't find ASPNET_REGSQL");
            if (sqlcmdGood && regsqlGood)
                Task.Run(new Action(SetupDB));
        }

        private bool RunProcess(string name, params string[] args)
        {
            var shortName = new FileInfo(name).Name;
            this.ToOutput("================================================================================");
            this.ToOutput(string.Format(":> {0} {1}\r\n", shortName, string.Join(" ", args)));
            var procInfo = new ProcessStartInfo(name, string.Join(" ", args.Where(s => s != null)));
            procInfo.CreateNoWindow = true;
            procInfo.ErrorDialog = true;
            procInfo.RedirectStandardError = true;
            procInfo.RedirectStandardInput = true;
            procInfo.RedirectStandardOutput = true;
            procInfo.UseShellExecute = false;
            using (var proc = new Process())
            {
                proc.StartInfo = procInfo;
                proc.ErrorDataReceived += proc_ErrorDataReceived;
                proc.OutputDataReceived += proc_OutputDataReceived;
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
                proc.OutputDataReceived -= proc_OutputDataReceived;
                proc.ErrorDataReceived -= proc_ErrorDataReceived;
            }
            this.ToOutput(string.Format("finished {0}", shortName));
            this.ToOutput("================================================================================");
            // TODO : need to figure out a way to tell when an error occurs
            // in an external process
            return true;
        }

        private void Dump(TextBox box, string txt)
        {
            this.SyncUI(() => box.AppendText(txt + Environment.NewLine));
        }

        private void ToError(string txt)
        {
            if (txt != null)
            {
                this.Dump(this.txtStdErr, txt);
                this.SyncUI(() => this.tabControl1.SelectedTab = this.tabStdErr);
            }
        }

        private void ToOutput(string txt)
        {
            this.Dump(this.txtStdOut, txt);
        }

        void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.ToOutput(e.Data);
        }

        void proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.ToError(e.Data);
        }

        private void SyncUI(Action act)
        {
            if (this.InvokeRequired)
                this.Invoke(act);
            else act();
        }

        private bool RunQueryWithSQLCMD(string qry, string database = null, bool isFile = false)
        {
            bool success = false;
            if (isFile && !File.Exists(qry))
                this.ToError(string.Format("Tried running script from file \"{0}\", but it doesn't exist!", qry));
            else
                success = RunProcess(
                    sqlcmdTB.Text,
                    "-S " + serverTB.Text,
                    (database != null) ? "-d " + database : null,
                    string.Format(" -{0} \"{1}\"", isFile ? "i" : "Q", qry));
            return success;
        }

        private bool RunASPNET_REGSQL()
        {
            return RunProcess(
                regsqlTB.Text,
                "-S " + serverTB.Text,
                "-E",
                "-A mr",
                "-d " + databaseTB.Text);
        }

        private void SetupDB()
        {
            try
            {
                bool succeeded = true;
                if (chkCreateDatabase.Checked)
                    succeeded &= RunQueryWithSQLCMD(string.Format("CREATE DATABASE {0};", databaseTB.Text));

                if (chkCreateLogin.Checked)
                {
                    if (succeeded)
                        succeeded &= RunQueryWithSQLCMD(string.Format("CREATE LOGIN {0} WITH PASSWORD = '{1}', DEFAULT_DATABASE={2};", sqlUserTB.Text, sqlPassTB.Text, databaseTB.Text));
                    if (succeeded)
                        succeeded &= RunQueryWithSQLCMD(string.Format("CREATE USER {0} FOR LOGIN {0};", sqlUserTB.Text), databaseTB.Text);
                    if (succeeded)
                        succeeded &= RunQueryWithSQLCMD(string.Format("ALTER USER {0} WITH DEFAULT_SCHEMA=dbo;", sqlUserTB.Text), databaseTB.Text);
                    if (succeeded)
                        succeeded &= RunQueryWithSQLCMD(string.Format("ALTER ROLE db_owner ADD MEMBER {0};", sqlUserTB.Text), databaseTB.Text);
                }

                if (succeeded && chkRegSql.Checked)
                    succeeded &= RunASPNET_REGSQL();

                if (succeeded 
                    && (chkCreateTables.Checked 
                        || chkSyncProcedures.Checked 
                        || chkInitializeData.Checked))
                    succeeded &= SyncSqlSiphon();

                if (succeeded)
                    this.ToOutput("All done");
                else
                    this.ToError("There was an error. Rerun in debug mode and step through the program in the debugger.");
            }
            catch (Exception exp)
            {
                this.ToError(exp.Message);
            }
            finally
            {
                this.SyncUI(() => runButton.Enabled = true);
            }
        }

        private bool SyncSqlSiphon()
        {
            var conn = MakeConnection();
            var dbs = MakeDatabaseConnection(conn);
            var succeeded = dbs != null;
            if (succeeded)
            {
                foreach (var db in dbs)
                {
                    var t = db.GetType();
                    this.ToOutput(string.Format("Syncing {0}.{1}", t.Namespace, t.Name));
                    try
                    {
                        if (succeeded && chkCreateTables.Checked)
                            succeeded &= SyncTables(db);

                        if (succeeded && chkSyncProcedures.Checked)
                            succeeded &= SyncProcedures(db);

                        if (succeeded && chkInitializeData.Checked)
                            succeeded &= InitData(db);
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        db.Dispose();
                        if (succeeded)
                            this.ToOutput(string.Format("Sync success {0}.{1}", t.Namespace, t.Name));
                        else

                            this.ToError(string.Format("Failed to sync {0}.{1}", t.Namespace, t.Name));
                    }
                }
            }
            return succeeded;
        }

        private IEnumerable<ISqlSiphon> MakeDatabaseConnection(string connectionString)
        {
            if (File.Exists(assemblyTB.Text))
            {
                var ss = typeof(ISqlSiphon);
                var constructorParams = new[] { typeof(string) };
                var constructorArgs = new object[] { connectionString };
                var assembly = Assembly.LoadFrom(assemblyTB.Text);
                return assembly.GetTypes()
                    .Where(t => t.GetInterfaces().Contains(ss)
                                && !t.IsAbstract
                                && !t.IsInterface)
                    .Select(t => (ISqlSiphon)(t.GetConstructor(constructorParams)
                                .Invoke(constructorArgs)));
            }
            return null;
        }

        private bool SyncTables(SqlSiphon.ISqlSiphon db)
        {
            try
            {
                this.ToOutput("Synchronizing tables");
                db.CreateTables();
                db.CreateIndices();
                db.CreateForeignKeys();
                this.ToOutput("Tables synched");
                return true;
            }
            catch (Exception exp)
            {
                this.ToError(exp.Message);
                return false;
            }
        }

        private bool SyncProcedures(SqlSiphon.ISqlSiphon db)
        {
            try
            {
                this.ToOutput("Synchronizing procedures");
                db.DropProcedures();
                db.SynchronizeUserDefinedTableTypes();
                db.CreateProcedures();
                this.ToOutput("Procedures synched");
                return true;
            }
            catch (Exception exp)
            {
                this.ToError(exp.Message);
                return false;
            }
        }

        private bool InitData(SqlSiphon.ISqlSiphon db)
        {
            try
            {
                this.ToOutput("Initializing data");
                db.InitializeData();
                this.ToOutput("Data initialized");
                return true;
            }
            catch (Exception exp)
            {
                this.ToError(exp.Message);
                return false;
            }
        }


        private string MakeConnection()
        {
            var conn = new System.Data.SqlClient.SqlConnectionStringBuilder();
            conn.DataSource = this.serverTB.Text;
            conn.UserID = this.sqlUserTB.Text;
            conn.Password = this.sqlPassTB.Text;
            conn.IntegratedSecurity = false;
            conn.InitialCatalog = this.databaseTB.Text;
            return conn.ConnectionString;
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
                this.sqlUserTB.Text,
                this.sqlPassTB.Text,
                this.assemblyTB.Text,
                this.chkCreateDatabase.Checked,
                this.chkCreateLogin.Checked,
                this.chkRegSql.Checked,
                this.chkCreateTables.Checked,
                this.chkInitializeData.Checked,
                this.chkSyncProcedures.Checked);

            if (this.sessions.ContainsKey(sesh.Name))
                this.sessions[sesh.Name] = sesh;
            else
            {
                this.sessions.Add(sesh.Name, sesh);
                this.names.Add(sesh.Name);
                this.savedSessionList.SelectedItem = sesh.Name;
            }

            this.SaveSettings();
        }

        private void SaveSettings()
        {
            File.WriteAllLines(SETTINGS_FILENAME, this.sessions.Values.Select(v => v.ToString()).ToArray());
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
                    this.SaveSettings();
                }
            }
        }

        private void savedSessionList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var sessionName = savedSessionList.SelectedItem as string;
            if (sessionName != null)
            {
                if (this.sessions.ContainsKey(sessionName))
                {
                    var session = this.sessions[sessionName];
                    this.serverTB.Text = session.Server;
                    this.databaseTB.Text = session.DBName;
                    this.sqlUserTB.Text = session.LoginName;
                    this.sqlPassTB.Text = session.LoginPassword;
                    this.assemblyTB.Text = session.AssemblyFile;
                    this.chkCreateDatabase.Checked = session.CreateDatabase;
                    this.chkCreateLogin.Checked = session.CreateLogin;
                    this.chkRegSql.Checked = session.RegisterASPNETMembership;
                    this.chkCreateTables.Checked = session.CreateSchemaObjects;
                    this.chkInitializeData.Checked = session.InitializeData;
                    this.chkSyncProcedures.Checked = session.SyncStoredProcedures;
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
    }
}
