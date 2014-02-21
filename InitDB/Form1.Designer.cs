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
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabStdOut.SuspendLayout();
            this.tabStdErr.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 255F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 321F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 159F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.serverTB, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.databaseTB, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.sqlUserTB, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.sqlPassTB, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.sqlcmdTB, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.regsqlTB, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.browseSqlCmdButton, 3, 7);
            this.tableLayoutPanel1.Controls.Add(this.browseRegSqlButton, 3, 8);
            this.tableLayoutPanel1.Controls.Add(this.runButton, 3, 13);
            this.tableLayoutPanel1.Controls.Add(this.chkCreateDatabase, 1, 10);
            this.tableLayoutPanel1.Controls.Add(this.chkCreateLogin, 1, 11);
            this.tableLayoutPanel1.Controls.Add(this.chkRegSql, 1, 12);
            this.tableLayoutPanel1.Controls.Add(this.chkCreateTables, 2, 10);
            this.tableLayoutPanel1.Controls.Add(this.chkInitializeData, 2, 11);
            this.tableLayoutPanel1.Controls.Add(this.chkSyncProcedures, 2, 12);
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 13);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.saveSessionButton, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.savedSessionList, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.deleteSessionButton, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 9);
            this.tableLayoutPanel1.Controls.Add(this.assemblyTB, 1, 9);
            this.tableLayoutPanel1.Controls.Add(this.browseAssemblyBtn, 3, 9);
            this.tableLayoutPanel1.Controls.Add(this.label9, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label10, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.textBox2, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.textBox3, 1, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 14;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1377, 1038);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(192, 59);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server:";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(122, 197);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(129, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "Database Name:";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(79, 243);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(172, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "SQL Login User Name:";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(90, 289);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(161, 20);
            this.label4.TabIndex = 3;
            this.label4.Text = "SQL Login Password:";
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(104, 335);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(147, 20);
            this.label5.TabIndex = 4;
            this.label5.Text = "SQLCMD tool path:";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(32, 381);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(219, 20);
            this.label6.TabIndex = 5;
            this.label6.Text = "ASPNET_REGSQL tool path:";
            // 
            // serverTB
            // 
            this.serverTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.serverTB, 2);
            this.serverTB.Location = new System.Drawing.Point(259, 56);
            this.serverTB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.serverTB.Name = "serverTB";
            this.serverTB.Size = new System.Drawing.Size(825, 26);
            this.serverTB.TabIndex = 2;
            this.serverTB.Text = "localhost\\SQLEXPRESS";
            // 
            // databaseTB
            // 
            this.databaseTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.databaseTB, 2);
            this.databaseTB.Location = new System.Drawing.Point(259, 194);
            this.databaseTB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.databaseTB.Name = "databaseTB";
            this.databaseTB.Size = new System.Drawing.Size(825, 26);
            this.databaseTB.TabIndex = 5;
            this.databaseTB.Text = "BBICARS";
            // 
            // sqlUserTB
            // 
            this.sqlUserTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.sqlUserTB, 2);
            this.sqlUserTB.Location = new System.Drawing.Point(259, 240);
            this.sqlUserTB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.sqlUserTB.Name = "sqlUserTB";
            this.sqlUserTB.Size = new System.Drawing.Size(825, 26);
            this.sqlUserTB.TabIndex = 6;
            this.sqlUserTB.Text = "BBICARS";
            // 
            // sqlPassTB
            // 
            this.sqlPassTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.sqlPassTB, 2);
            this.sqlPassTB.Location = new System.Drawing.Point(259, 286);
            this.sqlPassTB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.sqlPassTB.Name = "sqlPassTB";
            this.sqlPassTB.Size = new System.Drawing.Size(825, 26);
            this.sqlPassTB.TabIndex = 7;
            // 
            // sqlcmdTB
            // 
            this.sqlcmdTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.sqlcmdTB, 2);
            this.sqlcmdTB.Location = new System.Drawing.Point(259, 332);
            this.sqlcmdTB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.sqlcmdTB.Name = "sqlcmdTB";
            this.sqlcmdTB.Size = new System.Drawing.Size(825, 26);
            this.sqlcmdTB.TabIndex = 8;
            this.sqlcmdTB.Text = "C:\\Program Files\\Microsoft SQL Server\\110\\Tools\\Binn\\sqlcmd.exe";
            // 
            // regsqlTB
            // 
            this.regsqlTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.regsqlTB, 2);
            this.regsqlTB.Location = new System.Drawing.Point(259, 378);
            this.regsqlTB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.regsqlTB.Name = "regsqlTB";
            this.regsqlTB.Size = new System.Drawing.Size(825, 26);
            this.regsqlTB.TabIndex = 11;
            this.regsqlTB.Text = "C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\aspnet_regsql.exe";
            // 
            // browseSqlCmdButton
            // 
            this.browseSqlCmdButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browseSqlCmdButton.Location = new System.Drawing.Point(1092, 327);
            this.browseSqlCmdButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.browseSqlCmdButton.Name = "browseSqlCmdButton";
            this.browseSqlCmdButton.Size = new System.Drawing.Size(122, 36);
            this.browseSqlCmdButton.TabIndex = 9;
            this.browseSqlCmdButton.Text = "Browse...";
            this.browseSqlCmdButton.UseVisualStyleBackColor = true;
            this.browseSqlCmdButton.Click += new System.EventHandler(this.browse_Click);
            // 
            // browseRegSqlButton
            // 
            this.browseRegSqlButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browseRegSqlButton.Location = new System.Drawing.Point(1092, 373);
            this.browseRegSqlButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.browseRegSqlButton.Name = "browseRegSqlButton";
            this.browseRegSqlButton.Size = new System.Drawing.Size(122, 36);
            this.browseRegSqlButton.TabIndex = 12;
            this.browseRegSqlButton.Text = "Browse...";
            this.browseRegSqlButton.UseVisualStyleBackColor = true;
            this.browseRegSqlButton.Click += new System.EventHandler(this.browse_Click);
            // 
            // runButton
            // 
            this.runButton.Location = new System.Drawing.Point(1092, 603);
            this.runButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(112, 35);
            this.runButton.TabIndex = 22;
            this.runButton.Text = "Run";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // chkCreateDatabase
            // 
            this.chkCreateDatabase.AutoSize = true;
            this.chkCreateDatabase.Location = new System.Drawing.Point(259, 465);
            this.chkCreateDatabase.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkCreateDatabase.Name = "chkCreateDatabase";
            this.chkCreateDatabase.Size = new System.Drawing.Size(154, 24);
            this.chkCreateDatabase.TabIndex = 15;
            this.chkCreateDatabase.Text = "Create database";
            this.chkCreateDatabase.UseVisualStyleBackColor = true;
            // 
            // chkCreateLogin
            // 
            this.chkCreateLogin.AutoSize = true;
            this.chkCreateLogin.Location = new System.Drawing.Point(259, 511);
            this.chkCreateLogin.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkCreateLogin.Name = "chkCreateLogin";
            this.chkCreateLogin.Size = new System.Drawing.Size(191, 24);
            this.chkCreateLogin.TabIndex = 16;
            this.chkCreateLogin.Text = "Create database login";
            this.chkCreateLogin.UseVisualStyleBackColor = true;
            // 
            // chkRegSql
            // 
            this.chkRegSql.AutoSize = true;
            this.chkRegSql.Location = new System.Drawing.Point(259, 557);
            this.chkRegSql.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkRegSql.Name = "chkRegSql";
            this.chkRegSql.Size = new System.Drawing.Size(265, 24);
            this.chkRegSql.TabIndex = 17;
            this.chkRegSql.Text = "Register ASP.NET Memberships";
            this.chkRegSql.UseVisualStyleBackColor = true;
            // 
            // chkCreateTables
            // 
            this.chkCreateTables.AutoSize = true;
            this.chkCreateTables.Location = new System.Drawing.Point(580, 465);
            this.chkCreateTables.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkCreateTables.Name = "chkCreateTables";
            this.chkCreateTables.Size = new System.Drawing.Size(287, 24);
            this.chkCreateTables.TabIndex = 18;
            this.chkCreateTables.Text = "Create tables/indexes/fk constraints";
            this.chkCreateTables.UseVisualStyleBackColor = true;
            // 
            // chkInitializeData
            // 
            this.chkInitializeData.AutoSize = true;
            this.chkInitializeData.Location = new System.Drawing.Point(580, 511);
            this.chkInitializeData.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkInitializeData.Name = "chkInitializeData";
            this.chkInitializeData.Size = new System.Drawing.Size(128, 24);
            this.chkInitializeData.TabIndex = 19;
            this.chkInitializeData.Text = "Initialize data";
            this.chkInitializeData.UseVisualStyleBackColor = true;
            // 
            // chkSyncProcedures
            // 
            this.chkSyncProcedures.AutoSize = true;
            this.chkSyncProcedures.Checked = true;
            this.chkSyncProcedures.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSyncProcedures.Location = new System.Drawing.Point(580, 557);
            this.chkSyncProcedures.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.chkSyncProcedures.Name = "chkSyncProcedures";
            this.chkSyncProcedures.Size = new System.Drawing.Size(203, 24);
            this.chkSyncProcedures.TabIndex = 20;
            this.chkSyncProcedures.Text = "Sync stored procedures";
            this.chkSyncProcedures.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.tabControl1, 3);
            this.tabControl1.Controls.Add(this.tabStdOut);
            this.tabControl1.Controls.Add(this.tabStdErr);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(4, 603);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1080, 430);
            this.tabControl1.TabIndex = 18;
            // 
            // tabStdOut
            // 
            this.tabStdOut.Controls.Add(this.txtStdOut);
            this.tabStdOut.Controls.Add(this.textBox1);
            this.tabStdOut.Location = new System.Drawing.Point(4, 29);
            this.tabStdOut.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabStdOut.Name = "tabStdOut";
            this.tabStdOut.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabStdOut.Size = new System.Drawing.Size(1072, 397);
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
            this.txtStdOut.Location = new System.Drawing.Point(4, 5);
            this.txtStdOut.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtStdOut.Multiline = true;
            this.txtStdOut.Name = "txtStdOut";
            this.txtStdOut.ReadOnly = true;
            this.txtStdOut.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStdOut.Size = new System.Drawing.Size(1064, 387);
            this.txtStdOut.TabIndex = 1;
            this.txtStdOut.TabStop = false;
            this.txtStdOut.Text = "01234567890123456789012345678901234567890123456789012345678901234567890123456789";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(230, 360);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(148, 26);
            this.textBox1.TabIndex = 0;
            // 
            // tabStdErr
            // 
            this.tabStdErr.Controls.Add(this.txtStdErr);
            this.tabStdErr.Location = new System.Drawing.Point(4, 29);
            this.tabStdErr.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabStdErr.Name = "tabStdErr";
            this.tabStdErr.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tabStdErr.Size = new System.Drawing.Size(1072, 397);
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
            this.txtStdErr.Location = new System.Drawing.Point(4, 5);
            this.txtStdErr.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtStdErr.Multiline = true;
            this.txtStdErr.Name = "txtStdErr";
            this.txtStdErr.ReadOnly = true;
            this.txtStdErr.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStdErr.Size = new System.Drawing.Size(1064, 387);
            this.txtStdErr.TabIndex = 0;
            this.txtStdErr.TabStop = false;
            this.txtStdErr.Text = "01234567890123456789012345678901234567890123456789012345678901234567890123456789";
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(124, 13);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(127, 20);
            this.label7.TabIndex = 19;
            this.label7.Text = "Saved Sessions:";
            // 
            // saveSessionButton
            // 
            this.saveSessionButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.saveSessionButton.Location = new System.Drawing.Point(1092, 5);
            this.saveSessionButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.saveSessionButton.Name = "saveSessionButton";
            this.saveSessionButton.Size = new System.Drawing.Size(122, 36);
            this.saveSessionButton.TabIndex = 21;
            this.saveSessionButton.Text = "Save";
            this.saveSessionButton.UseVisualStyleBackColor = true;
            this.saveSessionButton.Click += new System.EventHandler(this.saveSessionButton_Click);
            // 
            // savedSessionList
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.savedSessionList, 2);
            this.savedSessionList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.savedSessionList.FormattingEnabled = true;
            this.savedSessionList.Location = new System.Drawing.Point(259, 5);
            this.savedSessionList.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.savedSessionList.Name = "savedSessionList";
            this.savedSessionList.Size = new System.Drawing.Size(825, 28);
            this.savedSessionList.Sorted = true;
            this.savedSessionList.TabIndex = 1;
            this.savedSessionList.SelectedIndexChanged += new System.EventHandler(this.savedSessionList_SelectedIndexChanged);
            this.savedSessionList.TextUpdate += new System.EventHandler(this.savedSessionList_TextUpdate);
            // 
            // deleteSessionButton
            // 
            this.deleteSessionButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deleteSessionButton.Location = new System.Drawing.Point(1222, 5);
            this.deleteSessionButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.deleteSessionButton.Name = "deleteSessionButton";
            this.deleteSessionButton.Size = new System.Drawing.Size(151, 36);
            this.deleteSessionButton.TabIndex = 23;
            this.deleteSessionButton.Text = "Delete";
            this.deleteSessionButton.UseVisualStyleBackColor = true;
            this.deleteSessionButton.Click += new System.EventHandler(this.deleteSessionButton_Click);
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(131, 427);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(120, 20);
            this.label8.TabIndex = 23;
            this.label8.Text = "Data Assembly:";
            // 
            // assemblyTB
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.assemblyTB, 2);
            this.assemblyTB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.assemblyTB.Location = new System.Drawing.Point(259, 419);
            this.assemblyTB.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.assemblyTB.Name = "assemblyTB";
            this.assemblyTB.Size = new System.Drawing.Size(825, 26);
            this.assemblyTB.TabIndex = 13;
            // 
            // browseAssemblyBtn
            // 
            this.browseAssemblyBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browseAssemblyBtn.Location = new System.Drawing.Point(1092, 419);
            this.browseAssemblyBtn.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.browseAssemblyBtn.Name = "browseAssemblyBtn";
            this.browseAssemblyBtn.Size = new System.Drawing.Size(122, 36);
            this.browseAssemblyBtn.TabIndex = 14;
            this.browseAssemblyBtn.Text = "Browse...";
            this.browseAssemblyBtn.UseVisualStyleBackColor = true;
            this.browseAssemblyBtn.Click += new System.EventHandler(this.browse_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Assemblies|*.exe;*.dll|All files|*.*";
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(110, 105);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(142, 20);
            this.label9.TabIndex = 26;
            this.label9.Text = "Admin User Name:";
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(121, 151);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(131, 20);
            this.label10.TabIndex = 27;
            this.label10.Text = "Admin Password:";
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.textBox2, 2);
            this.textBox2.Location = new System.Drawing.Point(258, 102);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(827, 26);
            this.textBox2.TabIndex = 3;
            // 
            // textBox3
            // 
            this.textBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.textBox3, 2);
            this.textBox3.Location = new System.Drawing.Point(258, 148);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(827, 26);
            this.textBox3.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1377, 1038);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
    }
}

