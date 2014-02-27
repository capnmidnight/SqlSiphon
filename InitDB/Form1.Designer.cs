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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabStdOut = new System.Windows.Forms.TabPage();
            this.txtStdOut = new System.Windows.Forms.TextBox();
            this.tabStdErr = new System.Windows.Forms.TabPage();
            this.txtStdErr = new System.Windows.Forms.TextBox();
            this.tabCreates = new System.Windows.Forms.TabPage();
            this.createsGV = new System.Windows.Forms.DataGridView();
            this.createsGVName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.createsGVScript = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.createsGVRunButton = new System.Windows.Forms.DataGridViewButtonColumn();
            this.tabDrops = new System.Windows.Forms.TabPage();
            this.dropsGV = new System.Windows.Forms.DataGridView();
            this.dropsGVName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dropsGVScript = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dropsGVRunButton = new System.Windows.Forms.DataGridViewButtonColumn();
            this.tabAlters = new System.Windows.Forms.TabPage();
            this.altersGV = new System.Windows.Forms.DataGridView();
            this.altersGVName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.altersGVScript = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.altersGVRunButton = new System.Windows.Forms.DataGridViewButtonColumn();
            this.tabOptions = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.browseRegSqlButton = new System.Windows.Forms.Button();
            this.browseSqlCmdButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.sqlcmdTB = new System.Windows.Forms.TextBox();
            this.regsqlTB = new System.Windows.Forms.TextBox();
            this.cancelOptionsButton = new System.Windows.Forms.Button();
            this.saveOptionsButton = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.saveSessionButton = new System.Windows.Forms.Button();
            this.savedSessionList = new System.Windows.Forms.ComboBox();
            this.deleteSessionButton = new System.Windows.Forms.Button();
            this.browseAssemblyBtn = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.sqlUserTB = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.sqlPassTB = new System.Windows.Forms.TextBox();
            this.adminPassTB = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.adminUserTB = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.serverTB = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.databaseTB = new System.Windows.Forms.TextBox();
            this.assemblyTB = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.chkCreateDatabase = new System.Windows.Forms.CheckBox();
            this.chkCreateTables = new System.Windows.Forms.CheckBox();
            this.chkCreateLogin = new System.Windows.Forms.CheckBox();
            this.chkRegSql = new System.Windows.Forms.CheckBox();
            this.runButton = new System.Windows.Forms.Button();
            this.chkSyncProcedures = new System.Windows.Forms.CheckBox();
            this.chkInitializeData = new System.Windows.Forms.CheckBox();
            this.chkCreateFKs = new System.Windows.Forms.CheckBox();
            this.chkCreateIndices = new System.Windows.Forms.CheckBox();
            this.analyzeButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tabOther = new System.Windows.Forms.TabPage();
            this.othersGV = new System.Windows.Forms.DataGridView();
            this.othersGVName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.othersGVScript = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.othersGVRunButton = new System.Windows.Forms.DataGridViewButtonColumn();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabStdOut.SuspendLayout();
            this.tabStdErr.SuspendLayout();
            this.tabCreates.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.createsGV)).BeginInit();
            this.tabDrops.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dropsGV)).BeginInit();
            this.tabAlters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.altersGV)).BeginInit();
            this.tabOptions.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tabOther.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.othersGV)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 186F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 245F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 147F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 10);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.saveSessionButton, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.savedSessionList, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.deleteSessionButton, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.browseAssemblyBtn, 5, 5);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.sqlUserTB, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.label4, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.sqlPassTB, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.adminPassTB, 3, 3);
            this.tableLayoutPanel1.Controls.Add(this.label10, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.adminUserTB, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label9, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.serverTB, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label2, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.databaseTB, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.assemblyTB, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.chkCreateDatabase, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.chkCreateTables, 2, 6);
            this.tableLayoutPanel1.Controls.Add(this.chkCreateLogin, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.chkRegSql, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.runButton, 5, 2);
            this.tableLayoutPanel1.Controls.Add(this.chkSyncProcedures, 2, 9);
            this.tableLayoutPanel1.Controls.Add(this.chkInitializeData, 1, 9);
            this.tableLayoutPanel1.Controls.Add(this.chkCreateFKs, 2, 7);
            this.tableLayoutPanel1.Controls.Add(this.chkCreateIndices, 2, 8);
            this.tableLayoutPanel1.Controls.Add(this.analyzeButton, 5, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 11;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(918, 671);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.tabControl1, 6);
            this.tabControl1.Controls.Add(this.tabStdOut);
            this.tabControl1.Controls.Add(this.tabStdErr);
            this.tabControl1.Controls.Add(this.tabCreates);
            this.tabControl1.Controls.Add(this.tabDrops);
            this.tabControl1.Controls.Add(this.tabAlters);
            this.tabControl1.Controls.Add(this.tabOther);
            this.tabControl1.Controls.Add(this.tabOptions);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 261);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(912, 407);
            this.tabControl1.TabIndex = 18;
            // 
            // tabStdOut
            // 
            this.tabStdOut.Controls.Add(this.txtStdOut);
            this.tabStdOut.Location = new System.Drawing.Point(4, 22);
            this.tabStdOut.Name = "tabStdOut";
            this.tabStdOut.Padding = new System.Windows.Forms.Padding(3);
            this.tabStdOut.Size = new System.Drawing.Size(904, 381);
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
            this.txtStdOut.Size = new System.Drawing.Size(898, 375);
            this.txtStdOut.TabIndex = 1;
            this.txtStdOut.TabStop = false;
            this.txtStdOut.Text = "01234567890123456789012345678901234567890123456789012345678901234567890123456789";
            // 
            // tabStdErr
            // 
            this.tabStdErr.Controls.Add(this.txtStdErr);
            this.tabStdErr.Location = new System.Drawing.Point(4, 22);
            this.tabStdErr.Name = "tabStdErr";
            this.tabStdErr.Padding = new System.Windows.Forms.Padding(3);
            this.tabStdErr.Size = new System.Drawing.Size(904, 381);
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
            this.txtStdErr.Size = new System.Drawing.Size(898, 375);
            this.txtStdErr.TabIndex = 0;
            this.txtStdErr.TabStop = false;
            this.txtStdErr.Text = "01234567890123456789012345678901234567890123456789012345678901234567890123456789";
            // 
            // tabCreates
            // 
            this.tabCreates.Controls.Add(this.createsGV);
            this.tabCreates.Location = new System.Drawing.Point(4, 22);
            this.tabCreates.Name = "tabCreates";
            this.tabCreates.Size = new System.Drawing.Size(904, 381);
            this.tabCreates.TabIndex = 5;
            this.tabCreates.Text = "Objects to add";
            this.tabCreates.UseVisualStyleBackColor = true;
            // 
            // createsGV
            // 
            this.createsGV.AllowUserToAddRows = false;
            this.createsGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.createsGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.createsGVName,
            this.createsGVScript,
            this.createsGVRunButton});
            this.createsGV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.createsGV.Location = new System.Drawing.Point(0, 0);
            this.createsGV.Name = "createsGV";
            this.createsGV.Size = new System.Drawing.Size(904, 381);
            this.createsGV.TabIndex = 3;
            this.createsGV.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.scriptGV_CellContentClick);
            // 
            // createsGVName
            // 
            this.createsGVName.DataPropertyName = "Key";
            this.createsGVName.HeaderText = "name";
            this.createsGVName.Name = "createsGVName";
            // 
            // createsGVScript
            // 
            this.createsGVScript.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.createsGVScript.DataPropertyName = "Value";
            this.createsGVScript.HeaderText = "script";
            this.createsGVScript.Name = "createsGVScript";
            // 
            // createsGVRunButton
            // 
            this.createsGVRunButton.HeaderText = "";
            this.createsGVRunButton.Name = "createsGVRunButton";
            this.createsGVRunButton.Text = "run";
            this.createsGVRunButton.UseColumnTextForButtonValue = true;
            // 
            // tabDrops
            // 
            this.tabDrops.Controls.Add(this.dropsGV);
            this.tabDrops.Location = new System.Drawing.Point(4, 22);
            this.tabDrops.Name = "tabDrops";
            this.tabDrops.Size = new System.Drawing.Size(904, 381);
            this.tabDrops.TabIndex = 2;
            this.tabDrops.Text = "Objects to drop";
            this.tabDrops.UseVisualStyleBackColor = true;
            // 
            // dropsGV
            // 
            this.dropsGV.AllowUserToAddRows = false;
            this.dropsGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dropsGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dropsGVName,
            this.dropsGVScript,
            this.dropsGVRunButton});
            this.dropsGV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dropsGV.Location = new System.Drawing.Point(0, 0);
            this.dropsGV.Name = "dropsGV";
            this.dropsGV.Size = new System.Drawing.Size(904, 381);
            this.dropsGV.TabIndex = 2;
            this.dropsGV.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.scriptGV_CellContentClick);
            // 
            // dropsGVName
            // 
            this.dropsGVName.DataPropertyName = "Key";
            this.dropsGVName.HeaderText = "name";
            this.dropsGVName.Name = "dropsGVName";
            // 
            // dropsGVScript
            // 
            this.dropsGVScript.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dropsGVScript.DataPropertyName = "Value";
            this.dropsGVScript.HeaderText = "script";
            this.dropsGVScript.Name = "dropsGVScript";
            // 
            // dropsGVRunButton
            // 
            this.dropsGVRunButton.HeaderText = "";
            this.dropsGVRunButton.Name = "dropsGVRunButton";
            this.dropsGVRunButton.Text = "run";
            this.dropsGVRunButton.UseColumnTextForButtonValue = true;
            // 
            // tabAlters
            // 
            this.tabAlters.Controls.Add(this.altersGV);
            this.tabAlters.Location = new System.Drawing.Point(4, 22);
            this.tabAlters.Name = "tabAlters";
            this.tabAlters.Size = new System.Drawing.Size(904, 381);
            this.tabAlters.TabIndex = 3;
            this.tabAlters.Text = "Objects to alter";
            this.tabAlters.UseVisualStyleBackColor = true;
            // 
            // altersGV
            // 
            this.altersGV.AllowUserToAddRows = false;
            this.altersGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.altersGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.altersGVName,
            this.altersGVScript,
            this.altersGVRunButton});
            this.altersGV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.altersGV.Location = new System.Drawing.Point(0, 0);
            this.altersGV.Name = "altersGV";
            this.altersGV.Size = new System.Drawing.Size(904, 381);
            this.altersGV.TabIndex = 5;
            this.altersGV.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.scriptGV_CellContentClick);
            // 
            // altersGVName
            // 
            this.altersGVName.DataPropertyName = "Key";
            this.altersGVName.HeaderText = "name";
            this.altersGVName.Name = "altersGVName";
            // 
            // altersGVScript
            // 
            this.altersGVScript.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.altersGVScript.DataPropertyName = "Value";
            this.altersGVScript.HeaderText = "script";
            this.altersGVScript.Name = "altersGVScript";
            // 
            // altersGVRunButton
            // 
            this.altersGVRunButton.HeaderText = "";
            this.altersGVRunButton.Name = "altersGVRunButton";
            this.altersGVRunButton.Text = "run";
            this.altersGVRunButton.UseColumnTextForButtonValue = true;
            // 
            // tabOptions
            // 
            this.tabOptions.Controls.Add(this.tableLayoutPanel4);
            this.tabOptions.Location = new System.Drawing.Point(4, 22);
            this.tabOptions.Name = "tabOptions";
            this.tabOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabOptions.Size = new System.Drawing.Size(904, 381);
            this.tabOptions.TabIndex = 4;
            this.tabOptions.Text = "Options";
            this.tabOptions.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 3;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 189F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 86F));
            this.tableLayoutPanel4.Controls.Add(this.browseRegSqlButton, 2, 1);
            this.tableLayoutPanel4.Controls.Add(this.browseSqlCmdButton, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.label6, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.sqlcmdTB, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.regsqlTB, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this.cancelOptionsButton, 2, 2);
            this.tableLayoutPanel4.Controls.Add(this.saveOptionsButton, 1, 2);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(898, 375);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // browseRegSqlButton
            // 
            this.browseRegSqlButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.browseRegSqlButton.Location = new System.Drawing.Point(815, 33);
            this.browseRegSqlButton.Name = "browseRegSqlButton";
            this.browseRegSqlButton.Size = new System.Drawing.Size(80, 23);
            this.browseRegSqlButton.TabIndex = 12;
            this.browseRegSqlButton.Text = "Browse...";
            this.browseRegSqlButton.UseVisualStyleBackColor = true;
            this.browseRegSqlButton.Click += new System.EventHandler(this.browse_Click);
            // 
            // browseSqlCmdButton
            // 
            this.browseSqlCmdButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.browseSqlCmdButton.Location = new System.Drawing.Point(815, 3);
            this.browseSqlCmdButton.Name = "browseSqlCmdButton";
            this.browseSqlCmdButton.Size = new System.Drawing.Size(80, 23);
            this.browseSqlCmdButton.TabIndex = 9;
            this.browseSqlCmdButton.Text = "Browse...";
            this.browseSqlCmdButton.UseVisualStyleBackColor = true;
            this.browseSqlCmdButton.Click += new System.EventHandler(this.browse_Click);
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(87, 8);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(99, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "SQLCMD tool path:";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(39, 38);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(147, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "ASPNET_REGSQL tool path:";
            // 
            // sqlcmdTB
            // 
            this.sqlcmdTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.sqlcmdTB.Location = new System.Drawing.Point(192, 5);
            this.sqlcmdTB.Name = "sqlcmdTB";
            this.sqlcmdTB.Size = new System.Drawing.Size(617, 20);
            this.sqlcmdTB.TabIndex = 8;
            this.sqlcmdTB.TextChanged += new System.EventHandler(this.enableSaveCancelButtons);
            // 
            // regsqlTB
            // 
            this.regsqlTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.regsqlTB.Location = new System.Drawing.Point(192, 35);
            this.regsqlTB.Name = "regsqlTB";
            this.regsqlTB.Size = new System.Drawing.Size(617, 20);
            this.regsqlTB.TabIndex = 11;
            this.regsqlTB.TextChanged += new System.EventHandler(this.enableSaveCancelButtons);
            // 
            // cancelOptionsButton
            // 
            this.cancelOptionsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelOptionsButton.Enabled = false;
            this.cancelOptionsButton.Location = new System.Drawing.Point(820, 349);
            this.cancelOptionsButton.Name = "cancelOptionsButton";
            this.cancelOptionsButton.Size = new System.Drawing.Size(75, 23);
            this.cancelOptionsButton.TabIndex = 13;
            this.cancelOptionsButton.Text = "Cancel";
            this.cancelOptionsButton.UseVisualStyleBackColor = true;
            this.cancelOptionsButton.Click += new System.EventHandler(this.cancelOptionsButton_Click);
            // 
            // saveOptionsButton
            // 
            this.saveOptionsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveOptionsButton.Enabled = false;
            this.saveOptionsButton.Location = new System.Drawing.Point(734, 349);
            this.saveOptionsButton.Name = "saveOptionsButton";
            this.saveOptionsButton.Size = new System.Drawing.Size(75, 23);
            this.saveOptionsButton.TabIndex = 14;
            this.saveOptionsButton.Text = "Save";
            this.saveOptionsButton.UseVisualStyleBackColor = true;
            this.saveOptionsButton.Click += new System.EventHandler(this.saveOptionsButton_Click);
            // 
            // label7
            // 
            this.label7.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(97, 8);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(86, 13);
            this.label7.TabIndex = 19;
            this.label7.Text = "Saved Sessions:";
            // 
            // saveSessionButton
            // 
            this.saveSessionButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.saveSessionButton.Location = new System.Drawing.Point(741, 3);
            this.saveSessionButton.Name = "saveSessionButton";
            this.saveSessionButton.Size = new System.Drawing.Size(86, 24);
            this.saveSessionButton.TabIndex = 21;
            this.saveSessionButton.Text = "Save";
            this.saveSessionButton.UseVisualStyleBackColor = true;
            this.saveSessionButton.Click += new System.EventHandler(this.saveSessionButton_Click);
            // 
            // savedSessionList
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.savedSessionList, 3);
            this.savedSessionList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.savedSessionList.FormattingEnabled = true;
            this.savedSessionList.Location = new System.Drawing.Point(189, 3);
            this.savedSessionList.Name = "savedSessionList";
            this.savedSessionList.Size = new System.Drawing.Size(546, 21);
            this.savedSessionList.Sorted = true;
            this.savedSessionList.TabIndex = 1;
            this.savedSessionList.SelectedIndexChanged += new System.EventHandler(this.savedSessionList_SelectedIndexChanged);
            this.savedSessionList.TextUpdate += new System.EventHandler(this.savedSessionList_TextUpdate);
            // 
            // deleteSessionButton
            // 
            this.deleteSessionButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deleteSessionButton.Location = new System.Drawing.Point(833, 3);
            this.deleteSessionButton.Name = "deleteSessionButton";
            this.deleteSessionButton.Size = new System.Drawing.Size(82, 24);
            this.deleteSessionButton.TabIndex = 23;
            this.deleteSessionButton.Text = "Delete";
            this.deleteSessionButton.UseVisualStyleBackColor = true;
            this.deleteSessionButton.Click += new System.EventHandler(this.deleteSessionButton_Click);
            // 
            // browseAssemblyBtn
            // 
            this.browseAssemblyBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browseAssemblyBtn.Location = new System.Drawing.Point(833, 131);
            this.browseAssemblyBtn.Name = "browseAssemblyBtn";
            this.browseAssemblyBtn.Size = new System.Drawing.Size(82, 24);
            this.browseAssemblyBtn.TabIndex = 14;
            this.browseAssemblyBtn.Text = "Browse...";
            this.browseAssemblyBtn.UseVisualStyleBackColor = true;
            this.browseAssemblyBtn.Click += new System.EventHandler(this.browse_Click);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(67, 106);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "SQL Login User Name:";
            // 
            // sqlUserTB
            // 
            this.sqlUserTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.sqlUserTB.Location = new System.Drawing.Point(189, 103);
            this.sqlUserTB.Name = "sqlUserTB";
            this.sqlUserTB.Size = new System.Drawing.Size(239, 20);
            this.sqlUserTB.TabIndex = 6;
            this.sqlUserTB.Text = "BBICARS";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(466, 106);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(109, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "SQL Login Password:";
            // 
            // sqlPassTB
            // 
            this.sqlPassTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.sqlPassTB, 2);
            this.sqlPassTB.Location = new System.Drawing.Point(581, 103);
            this.sqlPassTB.Name = "sqlPassTB";
            this.sqlPassTB.Size = new System.Drawing.Size(246, 20);
            this.sqlPassTB.TabIndex = 7;
            // 
            // adminPassTB
            // 
            this.adminPassTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.adminPassTB, 2);
            this.adminPassTB.Location = new System.Drawing.Point(580, 73);
            this.adminPassTB.Margin = new System.Windows.Forms.Padding(2);
            this.adminPassTB.Name = "adminPassTB";
            this.adminPassTB.Size = new System.Drawing.Size(248, 20);
            this.adminPassTB.TabIndex = 4;
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(488, 76);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(88, 13);
            this.label10.TabIndex = 27;
            this.label10.Text = "Admin Password:";
            // 
            // adminUserTB
            // 
            this.adminUserTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.adminUserTB.Location = new System.Drawing.Point(188, 73);
            this.adminUserTB.Margin = new System.Windows.Forms.Padding(2);
            this.adminUserTB.Name = "adminUserTB";
            this.adminUserTB.Size = new System.Drawing.Size(241, 20);
            this.adminUserTB.TabIndex = 3;
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(89, 76);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(95, 13);
            this.label9.TabIndex = 26;
            this.label9.Text = "Admin User Name:";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(142, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server:";
            // 
            // serverTB
            // 
            this.serverTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.serverTB.Location = new System.Drawing.Point(189, 43);
            this.serverTB.Name = "serverTB";
            this.serverTB.Size = new System.Drawing.Size(239, 20);
            this.serverTB.TabIndex = 2;
            this.serverTB.Text = "localhost\\SQLEXPRESS";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(488, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Database Name:";
            // 
            // databaseTB
            // 
            this.databaseTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.databaseTB, 2);
            this.databaseTB.Location = new System.Drawing.Point(581, 43);
            this.databaseTB.Name = "databaseTB";
            this.databaseTB.Size = new System.Drawing.Size(246, 20);
            this.databaseTB.TabIndex = 5;
            this.databaseTB.Text = "BBICARS";
            // 
            // assemblyTB
            // 
            this.assemblyTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.assemblyTB, 4);
            this.assemblyTB.Location = new System.Drawing.Point(189, 133);
            this.assemblyTB.Name = "assemblyTB";
            this.assemblyTB.Size = new System.Drawing.Size(638, 20);
            this.assemblyTB.TabIndex = 13;
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(103, 136);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(80, 13);
            this.label8.TabIndex = 23;
            this.label8.Text = "Data Assembly:";
            // 
            // chkCreateDatabase
            // 
            this.chkCreateDatabase.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkCreateDatabase.AutoSize = true;
            this.chkCreateDatabase.Location = new System.Drawing.Point(189, 162);
            this.chkCreateDatabase.Name = "chkCreateDatabase";
            this.chkCreateDatabase.Size = new System.Drawing.Size(104, 17);
            this.chkCreateDatabase.TabIndex = 15;
            this.chkCreateDatabase.Text = "Create database";
            this.chkCreateDatabase.UseVisualStyleBackColor = true;
            // 
            // chkCreateTables
            // 
            this.chkCreateTables.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkCreateTables.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkCreateTables, 2);
            this.chkCreateTables.Location = new System.Drawing.Point(434, 162);
            this.chkCreateTables.Name = "chkCreateTables";
            this.chkCreateTables.Size = new System.Drawing.Size(88, 17);
            this.chkCreateTables.TabIndex = 18;
            this.chkCreateTables.Text = "Create tables";
            this.chkCreateTables.UseVisualStyleBackColor = true;
            // 
            // chkCreateLogin
            // 
            this.chkCreateLogin.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkCreateLogin.AutoSize = true;
            this.chkCreateLogin.Location = new System.Drawing.Point(189, 187);
            this.chkCreateLogin.Name = "chkCreateLogin";
            this.chkCreateLogin.Size = new System.Drawing.Size(129, 17);
            this.chkCreateLogin.TabIndex = 16;
            this.chkCreateLogin.Text = "Create database login";
            this.chkCreateLogin.UseVisualStyleBackColor = true;
            // 
            // chkRegSql
            // 
            this.chkRegSql.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkRegSql.AutoSize = true;
            this.chkRegSql.Location = new System.Drawing.Point(189, 212);
            this.chkRegSql.Name = "chkRegSql";
            this.chkRegSql.Size = new System.Drawing.Size(179, 17);
            this.chkRegSql.TabIndex = 17;
            this.chkRegSql.Text = "Register ASP.NET Memberships";
            this.chkRegSql.UseVisualStyleBackColor = true;
            // 
            // runButton
            // 
            this.runButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.runButton.Location = new System.Drawing.Point(833, 41);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(82, 24);
            this.runButton.TabIndex = 22;
            this.runButton.Text = "Run";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // chkSyncProcedures
            // 
            this.chkSyncProcedures.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkSyncProcedures.AutoSize = true;
            this.chkSyncProcedures.Checked = true;
            this.chkSyncProcedures.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tableLayoutPanel1.SetColumnSpan(this.chkSyncProcedures, 2);
            this.chkSyncProcedures.Location = new System.Drawing.Point(434, 237);
            this.chkSyncProcedures.Name = "chkSyncProcedures";
            this.chkSyncProcedures.Size = new System.Drawing.Size(138, 17);
            this.chkSyncProcedures.TabIndex = 20;
            this.chkSyncProcedures.Text = "Sync stored procedures";
            this.chkSyncProcedures.UseVisualStyleBackColor = true;
            // 
            // chkInitializeData
            // 
            this.chkInitializeData.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkInitializeData.AutoSize = true;
            this.chkInitializeData.Location = new System.Drawing.Point(189, 237);
            this.chkInitializeData.Name = "chkInitializeData";
            this.chkInitializeData.Size = new System.Drawing.Size(87, 17);
            this.chkInitializeData.TabIndex = 19;
            this.chkInitializeData.Text = "Initialize data";
            this.chkInitializeData.UseVisualStyleBackColor = true;
            // 
            // chkCreateFKs
            // 
            this.chkCreateFKs.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkCreateFKs.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkCreateFKs, 2);
            this.chkCreateFKs.Location = new System.Drawing.Point(434, 187);
            this.chkCreateFKs.Name = "chkCreateFKs";
            this.chkCreateFKs.Size = new System.Drawing.Size(117, 17);
            this.chkCreateFKs.TabIndex = 28;
            this.chkCreateFKs.Text = "Create foreign keys";
            this.chkCreateFKs.UseVisualStyleBackColor = true;
            // 
            // chkCreateIndices
            // 
            this.chkCreateIndices.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkCreateIndices.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkCreateIndices, 2);
            this.chkCreateIndices.Location = new System.Drawing.Point(434, 212);
            this.chkCreateIndices.Name = "chkCreateIndices";
            this.chkCreateIndices.Size = new System.Drawing.Size(300, 17);
            this.chkCreateIndices.TabIndex = 29;
            this.chkCreateIndices.Text = "Create indices (NOT recommended for existing databases)";
            this.chkCreateIndices.UseVisualStyleBackColor = true;
            // 
            // analyzeButton
            // 
            this.analyzeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.analyzeButton.Location = new System.Drawing.Point(833, 71);
            this.analyzeButton.Name = "analyzeButton";
            this.analyzeButton.Size = new System.Drawing.Size(82, 23);
            this.analyzeButton.TabIndex = 30;
            this.analyzeButton.Text = "Analyze";
            this.analyzeButton.UseVisualStyleBackColor = true;
            this.analyzeButton.Click += new System.EventHandler(this.analyzeButton_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Assemblies|*.exe;*.dll|All files|*.*";
            // 
            // tabOther
            // 
            this.tabOther.Controls.Add(this.othersGV);
            this.tabOther.Location = new System.Drawing.Point(4, 22);
            this.tabOther.Name = "tabOther";
            this.tabOther.Size = new System.Drawing.Size(904, 381);
            this.tabOther.TabIndex = 6;
            this.tabOther.Text = "Other scripts";
            this.tabOther.UseVisualStyleBackColor = true;
            // 
            // othersGV
            // 
            this.othersGV.AllowUserToAddRows = false;
            this.othersGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.othersGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.othersGVName,
            this.othersGVScript,
            this.othersGVRunButton});
            this.othersGV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.othersGV.Location = new System.Drawing.Point(0, 0);
            this.othersGV.Name = "othersGV";
            this.othersGV.Size = new System.Drawing.Size(904, 381);
            this.othersGV.TabIndex = 6;
            this.othersGV.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.scriptGV_CellContentClick);
            // 
            // othersGVName
            // 
            this.othersGVName.DataPropertyName = "Key";
            this.othersGVName.HeaderText = "name";
            this.othersGVName.Name = "othersGVName";
            // 
            // othersGVScript
            // 
            this.othersGVScript.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.othersGVScript.DataPropertyName = "Value";
            this.othersGVScript.HeaderText = "script";
            this.othersGVScript.Name = "othersGVScript";
            // 
            // othersGVRunButton
            // 
            this.othersGVRunButton.HeaderText = "";
            this.othersGVRunButton.Name = "othersGVRunButton";
            this.othersGVRunButton.Text = "run";
            this.othersGVRunButton.UseColumnTextForButtonValue = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(918, 671);
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
            this.tabCreates.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.createsGV)).EndInit();
            this.tabDrops.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dropsGV)).EndInit();
            this.tabAlters.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.altersGV)).EndInit();
            this.tabOptions.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.tabOther.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.othersGV)).EndInit();
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
        private System.Windows.Forms.TextBox adminUserTB;
        private System.Windows.Forms.TextBox adminPassTB;
        private System.Windows.Forms.TabPage tabDrops;
        private System.Windows.Forms.TabPage tabOptions;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Button cancelOptionsButton;
        private System.Windows.Forms.Button saveOptionsButton;
        private System.Windows.Forms.CheckBox chkCreateFKs;
        private System.Windows.Forms.CheckBox chkCreateIndices;
        private System.Windows.Forms.Button analyzeButton;
        private System.Windows.Forms.TextBox txtStdOut;
        private System.Windows.Forms.TabPage tabCreates;
        private System.Windows.Forms.DataGridView createsGV;
        private System.Windows.Forms.DataGridViewTextBoxColumn createsGVName;
        private System.Windows.Forms.DataGridViewTextBoxColumn createsGVScript;
        private System.Windows.Forms.DataGridViewButtonColumn createsGVRunButton;
        private System.Windows.Forms.DataGridView dropsGV;
        private System.Windows.Forms.DataGridViewTextBoxColumn dropsGVName;
        private System.Windows.Forms.DataGridViewTextBoxColumn dropsGVScript;
        private System.Windows.Forms.DataGridViewButtonColumn dropsGVRunButton;
        private System.Windows.Forms.TabPage tabAlters;
        private System.Windows.Forms.DataGridView altersGV;
        private System.Windows.Forms.DataGridViewTextBoxColumn altersGVName;
        private System.Windows.Forms.DataGridViewTextBoxColumn altersGVScript;
        private System.Windows.Forms.DataGridViewButtonColumn altersGVRunButton;
        private System.Windows.Forms.TabPage tabOther;
        private System.Windows.Forms.DataGridView othersGV;
        private System.Windows.Forms.DataGridViewTextBoxColumn othersGVName;
        private System.Windows.Forms.DataGridViewTextBoxColumn othersGVScript;
        private System.Windows.Forms.DataGridViewButtonColumn othersGVRunButton;
    }
}

