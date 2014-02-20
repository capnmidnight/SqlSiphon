namespace InitDB
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.serverTB = new System.Windows.Forms.TextBox();
            this.databaseTB = new System.Windows.Forms.TextBox();
            this.sqlUserTB = new System.Windows.Forms.TextBox();
            this.sqlPassTB = new System.Windows.Forms.TextBox();
            this.sqlcmdTB = new System.Windows.Forms.TextBox();
            this.regsqlTB = new System.Windows.Forms.TextBox();
            this.browseSqlCmdButton = new System.Windows.Forms.Button();
            this.browseRegSqlButton = new System.Windows.Forms.Button();
            this.runButton = new System.Windows.Forms.Button();
            this.chkCreateDatabase = new System.Windows.Forms.CheckBox();
            this.chkCreateLogin = new System.Windows.Forms.CheckBox();
            this.chkRegSql = new System.Windows.Forms.CheckBox();
            this.chkCreateTables = new System.Windows.Forms.CheckBox();
            this.chkInitializeData = new System.Windows.Forms.CheckBox();
            this.chkSyncProcedures = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabStdOut = new System.Windows.Forms.TabPage();
            this.txtStdOut = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tabStdErr = new System.Windows.Forms.TabPage();
            this.txtStdErr = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.saveSessionButton = new System.Windows.Forms.Button();
            this.savedSessionList = new System.Windows.Forms.ComboBox();
            this.deleteSessionButton = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.assemblyTB = new System.Windows.Forms.TextBox();
            this.browseAssemblyBtn = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabStdOut.SuspendLayout();
            this.tabStdErr.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 170F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 214F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 87F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 106F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.serverTB, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.databaseTB, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.sqlUserTB, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.sqlPassTB, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.sqlcmdTB, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.regsqlTB, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.browseSqlCmdButton, 3, 5);
            this.tableLayoutPanel1.Controls.Add(this.browseRegSqlButton, 3, 6);
            this.tableLayoutPanel1.Controls.Add(this.runButton, 3, 11);
            this.tableLayoutPanel1.Controls.Add(this.chkCreateDatabase, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.chkCreateLogin, 1, 9);
            this.tableLayoutPanel1.Controls.Add(this.chkRegSql, 1, 10);
            this.tableLayoutPanel1.Controls.Add(this.chkCreateTables, 2, 8);
            this.tableLayoutPanel1.Controls.Add(this.chkInitializeData, 2, 9);
            this.tableLayoutPanel1.Controls.Add(this.chkSyncProcedures, 2, 10);
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 11);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.saveSessionButton, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.savedSessionList, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.deleteSessionButton, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.assemblyTB, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.browseAssemblyBtn, 3, 7);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 12;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(918, 702);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(122, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server:";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(67, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Database Name:";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 97);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(134, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = "SQL Login User Name:";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(42, 127);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(125, 15);
            this.label4.TabIndex = 3;
            this.label4.Text = "SQL Login Password:";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(55, 157);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(112, 15);
            this.label5.TabIndex = 4;
            this.label5.Text = "SQLCMD tool path:";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 187);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(164, 15);
            this.label6.TabIndex = 5;
            this.label6.Text = "ASPNET_REGSQL tool path:";
            // 
            // serverTB
            // 
            this.serverTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.serverTB, 2);
            this.serverTB.Location = new System.Drawing.Point(173, 35);
            this.serverTB.Name = "serverTB";
            this.serverTB.Size = new System.Drawing.Size(549, 20);
            this.serverTB.TabIndex = 0;
            this.serverTB.Text = "localhost\\SQLEXPRESS";
            // 
            // databaseTB
            // 
            this.databaseTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.databaseTB, 2);
            this.databaseTB.Location = new System.Drawing.Point(173, 65);
            this.databaseTB.Name = "databaseTB";
            this.databaseTB.Size = new System.Drawing.Size(549, 20);
            this.databaseTB.TabIndex = 1;
            this.databaseTB.Text = "BBICARS";
            // 
            // sqlUserTB
            // 
            this.sqlUserTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.sqlUserTB, 2);
            this.sqlUserTB.Location = new System.Drawing.Point(173, 95);
            this.sqlUserTB.Name = "sqlUserTB";
            this.sqlUserTB.Size = new System.Drawing.Size(549, 20);
            this.sqlUserTB.TabIndex = 2;
            this.sqlUserTB.Text = "BBICARS";
            // 
            // sqlPassTB
            // 
            this.sqlPassTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.sqlPassTB, 2);
            this.sqlPassTB.Location = new System.Drawing.Point(173, 125);
            this.sqlPassTB.Name = "sqlPassTB";
            this.sqlPassTB.Size = new System.Drawing.Size(549, 20);
            this.sqlPassTB.TabIndex = 3;
            // 
            // sqlcmdTB
            // 
            this.sqlcmdTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.sqlcmdTB, 2);
            this.sqlcmdTB.Location = new System.Drawing.Point(173, 155);
            this.sqlcmdTB.Name = "sqlcmdTB";
            this.sqlcmdTB.Size = new System.Drawing.Size(549, 20);
            this.sqlcmdTB.TabIndex = 7;
            this.sqlcmdTB.Text = "C:\\Program Files\\Microsoft SQL Server\\110\\Tools\\Binn\\sqlcmd.exe";
            // 
            // regsqlTB
            // 
            this.regsqlTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.regsqlTB, 2);
            this.regsqlTB.Location = new System.Drawing.Point(173, 185);
            this.regsqlTB.Name = "regsqlTB";
            this.regsqlTB.Size = new System.Drawing.Size(549, 20);
            this.regsqlTB.TabIndex = 9;
            this.regsqlTB.Text = "C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\aspnet_regsql.exe";
            // 
            // browseSqlCmdButton
            // 
            this.browseSqlCmdButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browseSqlCmdButton.Location = new System.Drawing.Point(728, 153);
            this.browseSqlCmdButton.Name = "browseSqlCmdButton";
            this.browseSqlCmdButton.Size = new System.Drawing.Size(81, 24);
            this.browseSqlCmdButton.TabIndex = 8;
            this.browseSqlCmdButton.Text = "Browse...";
            this.browseSqlCmdButton.UseVisualStyleBackColor = true;
            this.browseSqlCmdButton.Click += new System.EventHandler(this.browse_Click);
            // 
            // browseRegSqlButton
            // 
            this.browseRegSqlButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browseRegSqlButton.Location = new System.Drawing.Point(728, 183);
            this.browseRegSqlButton.Name = "browseRegSqlButton";
            this.browseRegSqlButton.Size = new System.Drawing.Size(81, 24);
            this.browseRegSqlButton.TabIndex = 10;
            this.browseRegSqlButton.Text = "Browse...";
            this.browseRegSqlButton.UseVisualStyleBackColor = true;
            this.browseRegSqlButton.Click += new System.EventHandler(this.browse_Click);
            // 
            // runButton
            // 
            this.runButton.Location = new System.Drawing.Point(728, 333);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(75, 23);
            this.runButton.TabIndex = 11;
            this.runButton.Text = "Run";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // chkCreateDatabase
            // 
            this.chkCreateDatabase.AutoSize = true;
            this.chkCreateDatabase.Location = new System.Drawing.Point(173, 243);
            this.chkCreateDatabase.Name = "chkCreateDatabase";
            this.chkCreateDatabase.Size = new System.Drawing.Size(116, 19);
            this.chkCreateDatabase.TabIndex = 12;
            this.chkCreateDatabase.Text = "Create database";
            this.chkCreateDatabase.UseVisualStyleBackColor = true;
            // 
            // chkCreateLogin
            // 
            this.chkCreateLogin.AutoSize = true;
            this.chkCreateLogin.Location = new System.Drawing.Point(173, 273);
            this.chkCreateLogin.Name = "chkCreateLogin";
            this.chkCreateLogin.Size = new System.Drawing.Size(146, 19);
            this.chkCreateLogin.TabIndex = 13;
            this.chkCreateLogin.Text = "Create database login";
            this.chkCreateLogin.UseVisualStyleBackColor = true;
            // 
            // chkRegSql
            // 
            this.chkRegSql.AutoSize = true;
            this.chkRegSql.Location = new System.Drawing.Point(173, 303);
            this.chkRegSql.Name = "chkRegSql";
            this.chkRegSql.Size = new System.Drawing.Size(204, 19);
            this.chkRegSql.TabIndex = 14;
            this.chkRegSql.Text = "Register ASP.NET Memberships";
            this.chkRegSql.UseVisualStyleBackColor = true;
            // 
            // chkCreateTables
            // 
            this.chkCreateTables.AutoSize = true;
            this.chkCreateTables.Location = new System.Drawing.Point(387, 243);
            this.chkCreateTables.Name = "chkCreateTables";
            this.chkCreateTables.Size = new System.Drawing.Size(218, 19);
            this.chkCreateTables.TabIndex = 15;
            this.chkCreateTables.Text = "Create tables/indexes/fk constraints";
            this.chkCreateTables.UseVisualStyleBackColor = true;
            // 
            // chkInitializeData
            // 
            this.chkInitializeData.AutoSize = true;
            this.chkInitializeData.Location = new System.Drawing.Point(387, 273);
            this.chkInitializeData.Name = "chkInitializeData";
            this.chkInitializeData.Size = new System.Drawing.Size(98, 19);
            this.chkInitializeData.TabIndex = 16;
            this.chkInitializeData.Text = "Initialize data";
            this.chkInitializeData.UseVisualStyleBackColor = true;
            // 
            // chkSyncProcedures
            // 
            this.chkSyncProcedures.AutoSize = true;
            this.chkSyncProcedures.Checked = true;
            this.chkSyncProcedures.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSyncProcedures.Location = new System.Drawing.Point(387, 303);
            this.chkSyncProcedures.Name = "chkSyncProcedures";
            this.chkSyncProcedures.Size = new System.Drawing.Size(154, 19);
            this.chkSyncProcedures.TabIndex = 17;
            this.chkSyncProcedures.Text = "Sync stored procedures";
            this.chkSyncProcedures.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.tabControl1, 3);
            this.tabControl1.Controls.Add(this.tabStdOut);
            this.tabControl1.Controls.Add(this.tabStdErr);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 333);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(719, 366);
            this.tabControl1.TabIndex = 18;
            // 
            // tabStdOut
            // 
            this.tabStdOut.Controls.Add(this.txtStdOut);
            this.tabStdOut.Controls.Add(this.textBox1);
            this.tabStdOut.Location = new System.Drawing.Point(4, 22);
            this.tabStdOut.Name = "tabStdOut";
            this.tabStdOut.Padding = new System.Windows.Forms.Padding(3);
            this.tabStdOut.Size = new System.Drawing.Size(711, 340);
            this.tabStdOut.TabIndex = 0;
            this.tabStdOut.Text = "Standard output";
            this.tabStdOut.UseVisualStyleBackColor = true;
            // 
            // txtStdOut
            // 
            this.txtStdOut.BackColor = System.Drawing.Color.Black;
            this.txtStdOut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtStdOut.Font = new System.Drawing.Font("Consolas", 9.980198F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStdOut.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.txtStdOut.Location = new System.Drawing.Point(3, 3);
            this.txtStdOut.Multiline = true;
            this.txtStdOut.Name = "txtStdOut";
            this.txtStdOut.ReadOnly = true;
            this.txtStdOut.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStdOut.Size = new System.Drawing.Size(705, 334);
            this.txtStdOut.TabIndex = 1;
            this.txtStdOut.Text = "01234567890123456789012345678901234567890123456789012345678901234567890123456789";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(153, 234);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 0;
            // 
            // tabStdErr
            // 
            this.tabStdErr.Controls.Add(this.txtStdErr);
            this.tabStdErr.Location = new System.Drawing.Point(4, 22);
            this.tabStdErr.Name = "tabStdErr";
            this.tabStdErr.Padding = new System.Windows.Forms.Padding(3);
            this.tabStdErr.Size = new System.Drawing.Size(711, 340);
            this.tabStdErr.TabIndex = 1;
            this.tabStdErr.Text = "Standard error";
            this.tabStdErr.UseVisualStyleBackColor = true;
            // 
            // txtStdErr
            // 
            this.txtStdErr.BackColor = System.Drawing.Color.Black;
            this.txtStdErr.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtStdErr.Font = new System.Drawing.Font("Consolas", 9.980198F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStdErr.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.txtStdErr.Location = new System.Drawing.Point(3, 3);
            this.txtStdErr.Multiline = true;
            this.txtStdErr.Name = "txtStdErr";
            this.txtStdErr.ReadOnly = true;
            this.txtStdErr.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStdErr.Size = new System.Drawing.Size(705, 334);
            this.txtStdErr.TabIndex = 0;
            this.txtStdErr.Text = "01234567890123456789012345678901234567890123456789012345678901234567890123456789";
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(70, 7);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(97, 15);
            this.label7.TabIndex = 19;
            this.label7.Text = "Saved Sessions:";
            // 
            // saveSessionButton
            // 
            this.saveSessionButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.saveSessionButton.Location = new System.Drawing.Point(728, 3);
            this.saveSessionButton.Name = "saveSessionButton";
            this.saveSessionButton.Size = new System.Drawing.Size(81, 24);
            this.saveSessionButton.TabIndex = 20;
            this.saveSessionButton.Text = "Save";
            this.saveSessionButton.UseVisualStyleBackColor = true;
            this.saveSessionButton.Click += new System.EventHandler(this.saveSessionButton_Click);
            // 
            // savedSessionList
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.savedSessionList, 2);
            this.savedSessionList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.savedSessionList.FormattingEnabled = true;
            this.savedSessionList.Location = new System.Drawing.Point(173, 3);
            this.savedSessionList.Name = "savedSessionList";
            this.savedSessionList.Size = new System.Drawing.Size(549, 21);
            this.savedSessionList.TabIndex = 21;
            this.savedSessionList.SelectedIndexChanged += new System.EventHandler(this.savedSessionList_SelectedIndexChanged);
            this.savedSessionList.TextUpdate += new System.EventHandler(this.savedSessionList_TextUpdate);
            // 
            // deleteSessionButton
            // 
            this.deleteSessionButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deleteSessionButton.Location = new System.Drawing.Point(815, 3);
            this.deleteSessionButton.Name = "deleteSessionButton";
            this.deleteSessionButton.Size = new System.Drawing.Size(100, 24);
            this.deleteSessionButton.TabIndex = 22;
            this.deleteSessionButton.Text = "Delete";
            this.deleteSessionButton.UseVisualStyleBackColor = true;
            this.deleteSessionButton.Click += new System.EventHandler(this.deleteSessionButton_Click);
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(76, 217);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(91, 15);
            this.label8.TabIndex = 23;
            this.label8.Text = "Data Assembly:";
            // 
            // assemblyTB
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.assemblyTB, 2);
            this.assemblyTB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.assemblyTB.Location = new System.Drawing.Point(173, 213);
            this.assemblyTB.Name = "assemblyTB";
            this.assemblyTB.Size = new System.Drawing.Size(549, 20);
            this.assemblyTB.TabIndex = 24;
            // 
            // browseAssemblyBtn
            // 
            this.browseAssemblyBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browseAssemblyBtn.Location = new System.Drawing.Point(728, 213);
            this.browseAssemblyBtn.Name = "browseAssemblyBtn";
            this.browseAssemblyBtn.Size = new System.Drawing.Size(81, 24);
            this.browseAssemblyBtn.TabIndex = 25;
            this.browseAssemblyBtn.Text = "Browse...";
            this.browseAssemblyBtn.UseVisualStyleBackColor = true;
            this.browseAssemblyBtn.Click += new System.EventHandler(this.browse_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Assemblies|*.exe;*.dll|All files|*.*";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(918, 702);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.Text = "Initialize CARS database";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabStdOut.ResumeLayout(false);
            this.tabStdOut.PerformLayout();
            this.tabStdErr.ResumeLayout(false);
            this.tabStdErr.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox serverTB;
        private System.Windows.Forms.TextBox databaseTB;
        private System.Windows.Forms.TextBox sqlUserTB;
        private System.Windows.Forms.TextBox sqlPassTB;
        private System.Windows.Forms.TextBox sqlcmdTB;
        private System.Windows.Forms.TextBox regsqlTB;
        private System.Windows.Forms.Button browseSqlCmdButton;
        private System.Windows.Forms.Button browseRegSqlButton;
        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.CheckBox chkCreateDatabase;
        private System.Windows.Forms.CheckBox chkCreateLogin;
        private System.Windows.Forms.CheckBox chkRegSql;
        private System.Windows.Forms.CheckBox chkCreateTables;
        private System.Windows.Forms.CheckBox chkInitializeData;
        private System.Windows.Forms.CheckBox chkSyncProcedures;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabStdOut;
        private System.Windows.Forms.TextBox txtStdOut;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TabPage tabStdErr;
        private System.Windows.Forms.TextBox txtStdErr;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button saveSessionButton;
        private System.Windows.Forms.ComboBox savedSessionList;
        private System.Windows.Forms.Button deleteSessionButton;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox assemblyTB;
        private System.Windows.Forms.Button browseAssemblyBtn;
    }
}

