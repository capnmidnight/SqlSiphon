namespace InitDB
{
    partial class MainForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pendingScriptsGV = new System.Windows.Forms.DataGridView();
            this.pendingScriptRunChoiceColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.pendingScriptTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pendingScriptNameColumn = new System.Windows.Forms.DataGridViewButtonColumn();
            this.pendingScriptScriptColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pendingScriptRunButtonColumn = new System.Windows.Forms.DataGridViewButtonColumn();
            this.objFilterTB = new System.Windows.Forms.TextBox();
            this.saveSessionButton = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
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
            this.createDatabaseChk = new System.Windows.Forms.CheckBox();
            this.createLoginChk = new System.Windows.Forms.CheckBox();
            this.runButton = new System.Windows.Forms.Button();
            this.createTablesChk = new System.Windows.Forms.CheckBox();
            this.createIndexesChk = new System.Windows.Forms.CheckBox();
            this.syncProceduresChk = new System.Windows.Forms.CheckBox();
            this.initializeDataChk = new System.Windows.Forms.CheckBox();
            this.createFKsChk = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.dbType = new System.Windows.Forms.Label();
            this.analyzeButton = new System.Windows.Forms.Button();
            this.installExtensionsChk = new System.Windows.Forms.CheckBox();
            this.deleteSessionButton = new System.Windows.Forms.Button();
            this.optionsBtn = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabConnectionProperties = new System.Windows.Forms.TabPage();
            this.tabStdOut = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.generalScriptTB = new System.Windows.Forms.TextBox();
            this.txtStdOut = new System.Windows.Forms.TextBox();
            this.tabStdErr = new System.Windows.Forms.TabPage();
            this.txtStdErr = new System.Windows.Forms.TextBox();
            this.tabScripts = new System.Windows.Forms.TabPage();
            this.label7 = new System.Windows.Forms.Label();
            this.savedSessionList = new System.Windows.Forms.ComboBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pendingScriptsGV)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabConnectionProperties.SuspendLayout();
            this.tabStdOut.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabStdErr.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 152F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 279F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 161F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 104F));
            this.tableLayoutPanel1.Controls.Add(this.pendingScriptsGV, 0, 10);
            this.tableLayoutPanel1.Controls.Add(this.objFilterTB, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.saveSessionButton, 4, 11);
            this.tableLayoutPanel1.Controls.Add(this.label14, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.browseAssemblyBtn, 4, 0);
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
            this.tableLayoutPanel1.Controls.Add(this.assemblyTB, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.createDatabaseChk, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.createLoginChk, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.runButton, 5, 5);
            this.tableLayoutPanel1.Controls.Add(this.createTablesChk, 1, 9);
            this.tableLayoutPanel1.Controls.Add(this.createIndexesChk, 2, 9);
            this.tableLayoutPanel1.Controls.Add(this.syncProceduresChk, 2, 6);
            this.tableLayoutPanel1.Controls.Add(this.initializeDataChk, 2, 7);
            this.tableLayoutPanel1.Controls.Add(this.createFKsChk, 2, 8);
            this.tableLayoutPanel1.Controls.Add(this.label12, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.dbType, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.analyzeButton, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.installExtensionsChk, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.deleteSessionButton, 5, 11);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 12;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(963, 607);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // pendingScriptsGV
            // 
            this.pendingScriptsGV.AllowUserToAddRows = false;
            this.pendingScriptsGV.AllowUserToDeleteRows = false;
            this.pendingScriptsGV.AllowUserToResizeRows = false;
            this.pendingScriptsGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.pendingScriptsGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.pendingScriptRunChoiceColumn,
            this.pendingScriptTypeColumn,
            this.pendingScriptNameColumn,
            this.pendingScriptScriptColumn,
            this.pendingScriptRunButtonColumn});
            this.tableLayoutPanel1.SetColumnSpan(this.pendingScriptsGV, 6);
            this.pendingScriptsGV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pendingScriptsGV.Location = new System.Drawing.Point(3, 283);
            this.pendingScriptsGV.Name = "pendingScriptsGV";
            this.pendingScriptsGV.ReadOnly = true;
            this.pendingScriptsGV.Size = new System.Drawing.Size(957, 291);
            this.pendingScriptsGV.TabIndex = 4;
            this.pendingScriptsGV.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.scriptGV_CellContentClick);
            // 
            // pendingScriptRunChoiceColumn
            // 
            this.pendingScriptRunChoiceColumn.DataPropertyName = "Run";
            this.pendingScriptRunChoiceColumn.HeaderText = "run";
            this.pendingScriptRunChoiceColumn.Name = "pendingScriptRunChoiceColumn";
            this.pendingScriptRunChoiceColumn.ReadOnly = true;
            // 
            // pendingScriptTypeColumn
            // 
            this.pendingScriptTypeColumn.DataPropertyName = "ScriptType";
            this.pendingScriptTypeColumn.HeaderText = "type";
            this.pendingScriptTypeColumn.Name = "pendingScriptTypeColumn";
            this.pendingScriptTypeColumn.ReadOnly = true;
            // 
            // pendingScriptNameColumn
            // 
            this.pendingScriptNameColumn.DataPropertyName = "Name";
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.pendingScriptNameColumn.DefaultCellStyle = dataGridViewCellStyle1;
            this.pendingScriptNameColumn.HeaderText = "name";
            this.pendingScriptNameColumn.Name = "pendingScriptNameColumn";
            this.pendingScriptNameColumn.ReadOnly = true;
            this.pendingScriptNameColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.pendingScriptNameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.pendingScriptNameColumn.Width = 200;
            // 
            // pendingScriptScriptColumn
            // 
            this.pendingScriptScriptColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.pendingScriptScriptColumn.DataPropertyName = "Script";
            this.pendingScriptScriptColumn.HeaderText = "script";
            this.pendingScriptScriptColumn.Name = "pendingScriptScriptColumn";
            this.pendingScriptScriptColumn.ReadOnly = true;
            // 
            // pendingScriptRunButtonColumn
            // 
            this.pendingScriptRunButtonColumn.HeaderText = "";
            this.pendingScriptRunButtonColumn.Name = "pendingScriptRunButtonColumn";
            this.pendingScriptRunButtonColumn.ReadOnly = true;
            this.pendingScriptRunButtonColumn.Text = "run";
            this.pendingScriptRunButtonColumn.UseColumnTextForButtonValue = true;
            // 
            // objFilterTB
            // 
            this.objFilterTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.objFilterTB, 4);
            this.objFilterTB.Location = new System.Drawing.Point(155, 155);
            this.objFilterTB.Name = "objFilterTB";
            this.objFilterTB.Size = new System.Drawing.Size(701, 20);
            this.objFilterTB.TabIndex = 35;
            // 
            // saveSessionButton
            // 
            this.saveSessionButton.Location = new System.Drawing.Point(762, 580);
            this.saveSessionButton.Name = "saveSessionButton";
            this.saveSessionButton.Size = new System.Drawing.Size(94, 24);
            this.saveSessionButton.TabIndex = 21;
            this.saveSessionButton.Text = "Save";
            this.saveSessionButton.UseVisualStyleBackColor = true;
            this.saveSessionButton.Click += new System.EventHandler(this.saveSessionButton_Click);
            // 
            // label14
            // 
            this.label14.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(86, 158);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(63, 13);
            this.label14.TabIndex = 34;
            this.label14.Text = "Object filter:";
            // 
            // browseAssemblyBtn
            // 
            this.browseAssemblyBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browseAssemblyBtn.Location = new System.Drawing.Point(762, 3);
            this.browseAssemblyBtn.Name = "browseAssemblyBtn";
            this.browseAssemblyBtn.Size = new System.Drawing.Size(94, 24);
            this.browseAssemblyBtn.TabIndex = 14;
            this.browseAssemblyBtn.Text = "Browse...";
            this.browseAssemblyBtn.UseVisualStyleBackColor = true;
            this.browseAssemblyBtn.Click += new System.EventHandler(this.browseAssemblyBtn_Click);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(46, 128);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "App login username:";
            // 
            // sqlUserTB
            // 
            this.sqlUserTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.sqlUserTB.Location = new System.Drawing.Point(155, 125);
            this.sqlUserTB.Name = "sqlUserTB";
            this.sqlUserTB.Size = new System.Drawing.Size(273, 20);
            this.sqlUserTB.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(487, 128);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(102, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "App login password:";
            // 
            // sqlPassTB
            // 
            this.sqlPassTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.sqlPassTB, 2);
            this.sqlPassTB.Location = new System.Drawing.Point(595, 125);
            this.sqlPassTB.Name = "sqlPassTB";
            this.sqlPassTB.Size = new System.Drawing.Size(261, 20);
            this.sqlPassTB.TabIndex = 7;
            // 
            // adminPassTB
            // 
            this.adminPassTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.adminPassTB, 2);
            this.adminPassTB.Location = new System.Drawing.Point(594, 95);
            this.adminPassTB.Margin = new System.Windows.Forms.Padding(2);
            this.adminPassTB.Name = "adminPassTB";
            this.adminPassTB.Size = new System.Drawing.Size(263, 20);
            this.adminPassTB.TabIndex = 4;
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(455, 98);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(135, 13);
            this.label10.TabIndex = 27;
            this.label10.Text = "Database admin password:";
            // 
            // adminUserTB
            // 
            this.adminUserTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.adminUserTB.Location = new System.Drawing.Point(154, 95);
            this.adminUserTB.Margin = new System.Windows.Forms.Padding(2);
            this.adminUserTB.Name = "adminUserTB";
            this.adminUserTB.Size = new System.Drawing.Size(275, 20);
            this.adminUserTB.TabIndex = 3;
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(20, 98);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(130, 13);
            this.label9.TabIndex = 26;
            this.label9.Text = "Database admin userame:";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(108, 68);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server:";
            // 
            // serverTB
            // 
            this.serverTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.serverTB.Location = new System.Drawing.Point(155, 65);
            this.serverTB.Name = "serverTB";
            this.serverTB.Size = new System.Drawing.Size(273, 20);
            this.serverTB.TabIndex = 2;
            this.serverTB.Text = "localhost\\SQLEXPRESS";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(504, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Database name:";
            // 
            // databaseTB
            // 
            this.databaseTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.databaseTB, 2);
            this.databaseTB.Location = new System.Drawing.Point(595, 65);
            this.databaseTB.Name = "databaseTB";
            this.databaseTB.Size = new System.Drawing.Size(261, 20);
            this.databaseTB.TabIndex = 5;
            // 
            // assemblyTB
            // 
            this.assemblyTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.SetColumnSpan(this.assemblyTB, 3);
            this.assemblyTB.Location = new System.Drawing.Point(155, 5);
            this.assemblyTB.Name = "assemblyTB";
            this.assemblyTB.Size = new System.Drawing.Size(601, 20);
            this.assemblyTB.TabIndex = 13;
            // 
            // label8
            // 
            this.label8.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(70, 8);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(79, 13);
            this.label8.TabIndex = 23;
            this.label8.Text = "Data assembly:";
            // 
            // createDatabaseChk
            // 
            this.createDatabaseChk.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.createDatabaseChk.AutoSize = true;
            this.createDatabaseChk.Location = new System.Drawing.Point(155, 184);
            this.createDatabaseChk.Name = "createDatabaseChk";
            this.createDatabaseChk.Size = new System.Drawing.Size(104, 17);
            this.createDatabaseChk.TabIndex = 15;
            this.createDatabaseChk.Text = "Create database";
            this.createDatabaseChk.UseVisualStyleBackColor = true;
            // 
            // createLoginChk
            // 
            this.createLoginChk.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.createLoginChk.AutoSize = true;
            this.createLoginChk.Location = new System.Drawing.Point(155, 209);
            this.createLoginChk.Name = "createLoginChk";
            this.createLoginChk.Size = new System.Drawing.Size(129, 17);
            this.createLoginChk.TabIndex = 16;
            this.createLoginChk.Text = "Create database login";
            this.createLoginChk.UseVisualStyleBackColor = true;
            // 
            // runButton
            // 
            this.runButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.runButton.Location = new System.Drawing.Point(862, 153);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(98, 24);
            this.runButton.TabIndex = 22;
            this.runButton.Text = "Run";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // createTablesChk
            // 
            this.createTablesChk.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.createTablesChk.AutoSize = true;
            this.createTablesChk.Location = new System.Drawing.Point(155, 259);
            this.createTablesChk.Name = "createTablesChk";
            this.createTablesChk.Size = new System.Drawing.Size(151, 17);
            this.createTablesChk.TabIndex = 18;
            this.createTablesChk.Text = "Create tables and columns";
            this.createTablesChk.UseVisualStyleBackColor = true;
            this.createTablesChk.CheckedChanged += new System.EventHandler(this.createTablesChk_CheckedChanged);
            // 
            // createIndexesChk
            // 
            this.createIndexesChk.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.createIndexesChk.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.createIndexesChk, 2);
            this.createIndexesChk.Location = new System.Drawing.Point(434, 259);
            this.createIndexesChk.Name = "createIndexesChk";
            this.createIndexesChk.Size = new System.Drawing.Size(300, 17);
            this.createIndexesChk.TabIndex = 29;
            this.createIndexesChk.Text = "Create indices (NOT recommended for existing databases)";
            this.createIndexesChk.UseVisualStyleBackColor = true;
            this.createIndexesChk.CheckedChanged += new System.EventHandler(this.createIndicesChk_CheckedChanged);
            // 
            // syncProceduresChk
            // 
            this.syncProceduresChk.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.syncProceduresChk.AutoSize = true;
            this.syncProceduresChk.Checked = true;
            this.syncProceduresChk.CheckState = System.Windows.Forms.CheckState.Checked;
            this.syncProceduresChk.Location = new System.Drawing.Point(434, 184);
            this.syncProceduresChk.Name = "syncProceduresChk";
            this.syncProceduresChk.Size = new System.Drawing.Size(138, 17);
            this.syncProceduresChk.TabIndex = 20;
            this.syncProceduresChk.Text = "Sync stored procedures";
            this.syncProceduresChk.UseVisualStyleBackColor = true;
            this.syncProceduresChk.CheckedChanged += new System.EventHandler(this.syncProceduresChk_CheckedChanged);
            // 
            // initializeDataChk
            // 
            this.initializeDataChk.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.initializeDataChk.AutoSize = true;
            this.initializeDataChk.Location = new System.Drawing.Point(434, 209);
            this.initializeDataChk.Name = "initializeDataChk";
            this.initializeDataChk.Size = new System.Drawing.Size(87, 17);
            this.initializeDataChk.TabIndex = 19;
            this.initializeDataChk.Text = "Initialize data";
            this.initializeDataChk.UseVisualStyleBackColor = true;
            this.initializeDataChk.CheckedChanged += new System.EventHandler(this.initializeDataChk_CheckedChanged);
            // 
            // createFKsChk
            // 
            this.createFKsChk.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.createFKsChk.AutoSize = true;
            this.createFKsChk.Location = new System.Drawing.Point(434, 234);
            this.createFKsChk.Name = "createFKsChk";
            this.createFKsChk.Size = new System.Drawing.Size(111, 17);
            this.createFKsChk.TabIndex = 28;
            this.createFKsChk.Text = "Create constraints";
            this.createFKsChk.UseVisualStyleBackColor = true;
            this.createFKsChk.CheckedChanged += new System.EventHandler(this.createFKsChk_CheckedChanged);
            // 
            // label12
            // 
            this.label12.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(70, 38);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(79, 13);
            this.label12.TabIndex = 31;
            this.label12.Text = "Database type:";
            // 
            // dbType
            // 
            this.dbType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.dbType.AutoSize = true;
            this.dbType.Location = new System.Drawing.Point(155, 38);
            this.dbType.Name = "dbType";
            this.dbType.Size = new System.Drawing.Size(65, 13);
            this.dbType.TabIndex = 36;
            this.dbType.Text = "UNKNOWN";
            // 
            // analyzeButton
            // 
            this.analyzeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.analyzeButton.Location = new System.Drawing.Point(862, 3);
            this.analyzeButton.Name = "analyzeButton";
            this.analyzeButton.Size = new System.Drawing.Size(98, 23);
            this.analyzeButton.TabIndex = 30;
            this.analyzeButton.Text = "Analyze";
            this.analyzeButton.UseVisualStyleBackColor = true;
            this.analyzeButton.Click += new System.EventHandler(this.analyzeButton_Click);
            // 
            // installExtensionsChk
            // 
            this.installExtensionsChk.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.installExtensionsChk.AutoSize = true;
            this.installExtensionsChk.Location = new System.Drawing.Point(155, 234);
            this.installExtensionsChk.Name = "installExtensionsChk";
            this.installExtensionsChk.Size = new System.Drawing.Size(106, 17);
            this.installExtensionsChk.TabIndex = 33;
            this.installExtensionsChk.Text = "Install extensions";
            this.installExtensionsChk.UseVisualStyleBackColor = true;
            // 
            // deleteSessionButton
            // 
            this.deleteSessionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.deleteSessionButton.Location = new System.Drawing.Point(862, 580);
            this.deleteSessionButton.Name = "deleteSessionButton";
            this.deleteSessionButton.Size = new System.Drawing.Size(98, 24);
            this.deleteSessionButton.TabIndex = 23;
            this.deleteSessionButton.Text = "Delete";
            this.deleteSessionButton.UseVisualStyleBackColor = true;
            this.deleteSessionButton.Click += new System.EventHandler(this.deleteSessionButton_Click);
            // 
            // optionsBtn
            // 
            this.optionsBtn.Location = new System.Drawing.Point(914, 7);
            this.optionsBtn.Name = "optionsBtn";
            this.optionsBtn.Size = new System.Drawing.Size(75, 23);
            this.optionsBtn.TabIndex = 37;
            this.optionsBtn.Text = "Options";
            this.optionsBtn.UseVisualStyleBackColor = true;
            this.optionsBtn.Click += new System.EventHandler(this.optionsBtn_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabConnectionProperties);
            this.tabControl1.Controls.Add(this.tabStdOut);
            this.tabControl1.Controls.Add(this.tabStdErr);
            this.tabControl1.Controls.Add(this.tabScripts);
            this.tabControl1.Location = new System.Drawing.Point(12, 36);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(977, 639);
            this.tabControl1.TabIndex = 18;
            // 
            // tabConnectionProperties
            // 
            this.tabConnectionProperties.Controls.Add(this.tableLayoutPanel1);
            this.tabConnectionProperties.Location = new System.Drawing.Point(4, 22);
            this.tabConnectionProperties.Name = "tabConnectionProperties";
            this.tabConnectionProperties.Padding = new System.Windows.Forms.Padding(3);
            this.tabConnectionProperties.Size = new System.Drawing.Size(969, 613);
            this.tabConnectionProperties.TabIndex = 9;
            this.tabConnectionProperties.Text = "Properties";
            this.tabConnectionProperties.UseVisualStyleBackColor = true;
            // 
            // tabStdOut
            // 
            this.tabStdOut.Controls.Add(this.splitContainer1);
            this.tabStdOut.Location = new System.Drawing.Point(4, 22);
            this.tabStdOut.Name = "tabStdOut";
            this.tabStdOut.Padding = new System.Windows.Forms.Padding(3);
            this.tabStdOut.Size = new System.Drawing.Size(969, 613);
            this.tabStdOut.TabIndex = 0;
            this.tabStdOut.Text = "Standard output";
            this.tabStdOut.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.generalScriptTB);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtStdOut);
            this.splitContainer1.Size = new System.Drawing.Size(963, 607);
            this.splitContainer1.SplitterDistance = 120;
            this.splitContainer1.TabIndex = 2;
            // 
            // generalScriptTB
            // 
            this.generalScriptTB.AcceptsReturn = true;
            this.generalScriptTB.AcceptsTab = true;
            this.generalScriptTB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.generalScriptTB.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.generalScriptTB.Location = new System.Drawing.Point(0, 0);
            this.generalScriptTB.Multiline = true;
            this.generalScriptTB.Name = "generalScriptTB";
            this.generalScriptTB.Size = new System.Drawing.Size(963, 120);
            this.generalScriptTB.TabIndex = 0;
            this.generalScriptTB.Text = "-- (hit CTRL+Enter to run script)";
            // 
            // txtStdOut
            // 
            this.txtStdOut.BackColor = System.Drawing.Color.Black;
            this.txtStdOut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtStdOut.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStdOut.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.txtStdOut.Location = new System.Drawing.Point(0, 0);
            this.txtStdOut.Multiline = true;
            this.txtStdOut.Name = "txtStdOut";
            this.txtStdOut.ReadOnly = true;
            this.txtStdOut.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStdOut.Size = new System.Drawing.Size(963, 483);
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
            this.tabStdErr.Size = new System.Drawing.Size(969, 613);
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
            this.txtStdErr.Size = new System.Drawing.Size(963, 607);
            this.txtStdErr.TabIndex = 0;
            this.txtStdErr.TabStop = false;
            this.txtStdErr.Text = "01234567890123456789012345678901234567890123456789012345678901234567890123456789";
            // 
            // tabScripts
            // 
            this.tabScripts.Location = new System.Drawing.Point(4, 22);
            this.tabScripts.Name = "tabScripts";
            this.tabScripts.Padding = new System.Windows.Forms.Padding(3);
            this.tabScripts.Size = new System.Drawing.Size(969, 613);
            this.tabScripts.TabIndex = 10;
            this.tabScripts.Text = "Pending scripts";
            this.tabScripts.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 13);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(84, 13);
            this.label7.TabIndex = 19;
            this.label7.Text = "Saved sessions:";
            // 
            // savedSessionList
            // 
            this.savedSessionList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.savedSessionList.FormattingEnabled = true;
            this.savedSessionList.Location = new System.Drawing.Point(102, 9);
            this.savedSessionList.Name = "savedSessionList";
            this.savedSessionList.Size = new System.Drawing.Size(806, 21);
            this.savedSessionList.Sorted = true;
            this.savedSessionList.TabIndex = 1;
            this.savedSessionList.SelectedIndexChanged += new System.EventHandler(this.savedSessionList_SelectedIndexChanged);
            this.savedSessionList.TextUpdate += new System.EventHandler(this.savedSessionList_TextUpdate);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Assemblies|*.exe;*.dll|All files|*.*";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1001, 687);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.savedSessionList);
            this.Controls.Add(this.optionsBtn);
            this.Name = "MainForm";
            this.Text = "Initialize database";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pendingScriptsGV)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabConnectionProperties.ResumeLayout(false);
            this.tabStdOut.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabStdErr.ResumeLayout(false);
            this.tabStdErr.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox serverTB;
        private System.Windows.Forms.TextBox databaseTB;
        private System.Windows.Forms.TextBox sqlUserTB;
        private System.Windows.Forms.TextBox sqlPassTB;
        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.CheckBox createDatabaseChk;
        private System.Windows.Forms.CheckBox createLoginChk;
        private System.Windows.Forms.CheckBox createTablesChk;
        private System.Windows.Forms.CheckBox initializeDataChk;
        private System.Windows.Forms.CheckBox syncProceduresChk;
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
        private System.Windows.Forms.CheckBox createFKsChk;
        private System.Windows.Forms.CheckBox createIndexesChk;
        private System.Windows.Forms.Button analyzeButton;
        private System.Windows.Forms.TextBox txtStdOut;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox installExtensionsChk;
        private System.Windows.Forms.TextBox objFilterTB;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label dbType;
        private System.Windows.Forms.TabPage tabConnectionProperties;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox generalScriptTB;
        private System.Windows.Forms.Button optionsBtn;
        private System.Windows.Forms.TabPage tabScripts;
        private System.Windows.Forms.DataGridView pendingScriptsGV;
        private System.Windows.Forms.DataGridViewCheckBoxColumn pendingScriptRunChoiceColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn pendingScriptTypeColumn;
        private System.Windows.Forms.DataGridViewButtonColumn pendingScriptNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn pendingScriptScriptColumn;
        private System.Windows.Forms.DataGridViewButtonColumn pendingScriptRunButtonColumn;
    }
}

