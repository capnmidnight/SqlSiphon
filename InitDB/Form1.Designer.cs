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
            this.tabDrops = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label11 = new System.Windows.Forms.Label();
            this.searchDropsTB = new System.Windows.Forms.TextBox();
            this.dropsGV = new System.Windows.Forms.DataGridView();
            this.ignoreDropsButton = new System.Windows.Forms.Button();
            this.dropSelectedButton = new System.Windows.Forms.Button();
            this.tabAlters = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.altersGV = new System.Windows.Forms.DataGridView();
            this.label12 = new System.Windows.Forms.Label();
            this.filterAltersTB = new System.Windows.Forms.TextBox();
            this.ignoreAltersButton = new System.Windows.Forms.Button();
            this.alterSelectedButton = new System.Windows.Forms.Button();
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
            this.chkInitializeData = new System.Windows.Forms.CheckBox();
            this.chkSyncProcedures = new System.Windows.Forms.CheckBox();
            this.runButton = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.altersGVName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.altersGVScript = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.altersGVRunButton = new System.Windows.Forms.DataGridViewButtonColumn();
            this.dropsGVName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dropsGVScript = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dropsGVRunButton = new System.Windows.Forms.DataGridViewButtonColumn();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabStdOut.SuspendLayout();
            this.tabStdErr.SuspendLayout();
            this.tabDrops.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dropsGV)).BeginInit();
            this.tabAlters.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.altersGV)).BeginInit();
            this.tabOptions.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
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
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 9);
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
            this.tableLayoutPanel1.Controls.Add(this.chkInitializeData, 2, 7);
            this.tableLayoutPanel1.Controls.Add(this.chkSyncProcedures, 2, 8);
            this.tableLayoutPanel1.Controls.Add(this.runButton, 5, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 10;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 8F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(918, 671);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.tabControl1, 6);
            this.tabControl1.Controls.Add(this.tabStdOut);
            this.tabControl1.Controls.Add(this.tabStdErr);
            this.tabControl1.Controls.Add(this.tabDrops);
            this.tabControl1.Controls.Add(this.tabAlters);
            this.tabControl1.Controls.Add(this.tabOptions);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 251);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(912, 417);
            this.tabControl1.TabIndex = 18;
            // 
            // tabStdOut
            // 
            this.tabStdOut.Controls.Add(this.txtStdOut);
            this.tabStdOut.Location = new System.Drawing.Point(4, 22);
            this.tabStdOut.Name = "tabStdOut";
            this.tabStdOut.Padding = new System.Windows.Forms.Padding(3);
            this.tabStdOut.Size = new System.Drawing.Size(904, 391);
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
            this.txtStdOut.Size = new System.Drawing.Size(898, 385);
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
            this.tabStdErr.Size = new System.Drawing.Size(904, 391);
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
            this.txtStdErr.Size = new System.Drawing.Size(898, 385);
            this.txtStdErr.TabIndex = 0;
            this.txtStdErr.TabStop = false;
            this.txtStdErr.Text = "01234567890123456789012345678901234567890123456789012345678901234567890123456789";
            // 
            // tabDrops
            // 
            this.tabDrops.Controls.Add(this.tableLayoutPanel2);
            this.tabDrops.Location = new System.Drawing.Point(4, 22);
            this.tabDrops.Name = "tabDrops";
            this.tabDrops.Size = new System.Drawing.Size(904, 391);
            this.tabDrops.TabIndex = 2;
            this.tabDrops.Text = "Objects to drop";
            this.tabDrops.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 94F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 87F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 89F));
            this.tableLayoutPanel2.Controls.Add(this.label11, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.searchDropsTB, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.dropsGV, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.ignoreDropsButton, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.dropSelectedButton, 3, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(904, 391);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // label11
            // 
            this.label11.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(24, 8);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(67, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "Filter (regex):";
            // 
            // searchDropsTB
            // 
            this.searchDropsTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.searchDropsTB.Location = new System.Drawing.Point(97, 5);
            this.searchDropsTB.Name = "searchDropsTB";
            this.searchDropsTB.Size = new System.Drawing.Size(628, 20);
            this.searchDropsTB.TabIndex = 1;
            // 
            // dropsGV
            // 
            this.dropsGV.AllowUserToAddRows = false;
            this.dropsGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dropsGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dropsGVName,
            this.dropsGVScript,
            this.dropsGVRunButton});
            this.tableLayoutPanel2.SetColumnSpan(this.dropsGV, 4);
            this.dropsGV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dropsGV.Location = new System.Drawing.Point(3, 33);
            this.dropsGV.Name = "dropsGV";
            this.dropsGV.Size = new System.Drawing.Size(898, 355);
            this.dropsGV.TabIndex = 2;
            this.dropsGV.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.scriptGV_CellContentClick);
            // 
            // ignoreDropsButton
            // 
            this.ignoreDropsButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ignoreDropsButton.Location = new System.Drawing.Point(731, 3);
            this.ignoreDropsButton.Name = "ignoreDropsButton";
            this.ignoreDropsButton.Size = new System.Drawing.Size(81, 24);
            this.ignoreDropsButton.TabIndex = 3;
            this.ignoreDropsButton.Text = "Ignore";
            this.ignoreDropsButton.UseVisualStyleBackColor = true;
            // 
            // dropSelectedButton
            // 
            this.dropSelectedButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dropSelectedButton.Location = new System.Drawing.Point(818, 3);
            this.dropSelectedButton.Name = "dropSelectedButton";
            this.dropSelectedButton.Size = new System.Drawing.Size(83, 24);
            this.dropSelectedButton.TabIndex = 4;
            this.dropSelectedButton.Text = "Drop";
            this.dropSelectedButton.UseVisualStyleBackColor = true;
            // 
            // tabAlters
            // 
            this.tabAlters.Controls.Add(this.tableLayoutPanel3);
            this.tabAlters.Location = new System.Drawing.Point(4, 22);
            this.tabAlters.Name = "tabAlters";
            this.tabAlters.Size = new System.Drawing.Size(904, 391);
            this.tabAlters.TabIndex = 3;
            this.tabAlters.Text = "Objects to alter";
            this.tabAlters.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 4;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 94F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 87F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 89F));
            this.tableLayoutPanel3.Controls.Add(this.altersGV, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.label12, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.filterAltersTB, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.ignoreAltersButton, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.alterSelectedButton, 3, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(904, 391);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // altersGV
            // 
            this.altersGV.AllowUserToAddRows = false;
            this.altersGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.altersGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.altersGVName,
            this.altersGVScript,
            this.altersGVRunButton});
            this.tableLayoutPanel3.SetColumnSpan(this.altersGV, 4);
            this.altersGV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.altersGV.Location = new System.Drawing.Point(3, 33);
            this.altersGV.Name = "altersGV";
            this.altersGV.Size = new System.Drawing.Size(898, 355);
            this.altersGV.TabIndex = 5;
            this.altersGV.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.scriptGV_CellContentClick);
            // 
            // label12
            // 
            this.label12.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(24, 8);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(67, 13);
            this.label12.TabIndex = 0;
            this.label12.Text = "Filter (regex):";
            // 
            // filterAltersTB
            // 
            this.filterAltersTB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.filterAltersTB.Location = new System.Drawing.Point(97, 5);
            this.filterAltersTB.Name = "filterAltersTB";
            this.filterAltersTB.Size = new System.Drawing.Size(628, 20);
            this.filterAltersTB.TabIndex = 1;
            // 
            // ignoreAltersButton
            // 
            this.ignoreAltersButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ignoreAltersButton.Location = new System.Drawing.Point(731, 3);
            this.ignoreAltersButton.Name = "ignoreAltersButton";
            this.ignoreAltersButton.Size = new System.Drawing.Size(81, 24);
            this.ignoreAltersButton.TabIndex = 3;
            this.ignoreAltersButton.Text = "Ignore";
            this.ignoreAltersButton.UseVisualStyleBackColor = true;
            // 
            // alterSelectedButton
            // 
            this.alterSelectedButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.alterSelectedButton.Location = new System.Drawing.Point(818, 3);
            this.alterSelectedButton.Name = "alterSelectedButton";
            this.alterSelectedButton.Size = new System.Drawing.Size(83, 24);
            this.alterSelectedButton.TabIndex = 4;
            this.alterSelectedButton.Text = "Alter";
            this.alterSelectedButton.UseVisualStyleBackColor = true;
            // 
            // tabOptions
            // 
            this.tabOptions.Controls.Add(this.tableLayoutPanel4);
            this.tabOptions.Location = new System.Drawing.Point(4, 22);
            this.tabOptions.Name = "tabOptions";
            this.tabOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabOptions.Size = new System.Drawing.Size(904, 391);
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
            this.tableLayoutPanel4.Size = new System.Drawing.Size(898, 385);
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
            this.cancelOptionsButton.Location = new System.Drawing.Point(820, 359);
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
            this.saveOptionsButton.Location = new System.Drawing.Point(734, 359);
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
            this.chkCreateDatabase.AutoSize = true;
            this.chkCreateDatabase.Location = new System.Drawing.Point(189, 161);
            this.chkCreateDatabase.Name = "chkCreateDatabase";
            this.chkCreateDatabase.Size = new System.Drawing.Size(104, 17);
            this.chkCreateDatabase.TabIndex = 15;
            this.chkCreateDatabase.Text = "Create database";
            this.chkCreateDatabase.UseVisualStyleBackColor = true;
            // 
            // chkCreateTables
            // 
            this.chkCreateTables.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkCreateTables, 2);
            this.chkCreateTables.Location = new System.Drawing.Point(434, 161);
            this.chkCreateTables.Name = "chkCreateTables";
            this.chkCreateTables.Size = new System.Drawing.Size(156, 17);
            this.chkCreateTables.TabIndex = 18;
            this.chkCreateTables.Text = "Create tables/fk constraints";
            this.chkCreateTables.UseVisualStyleBackColor = true;
            // 
            // chkCreateLogin
            // 
            this.chkCreateLogin.AutoSize = true;
            this.chkCreateLogin.Location = new System.Drawing.Point(189, 191);
            this.chkCreateLogin.Name = "chkCreateLogin";
            this.chkCreateLogin.Size = new System.Drawing.Size(129, 17);
            this.chkCreateLogin.TabIndex = 16;
            this.chkCreateLogin.Text = "Create database login";
            this.chkCreateLogin.UseVisualStyleBackColor = true;
            // 
            // chkRegSql
            // 
            this.chkRegSql.AutoSize = true;
            this.chkRegSql.Location = new System.Drawing.Point(189, 221);
            this.chkRegSql.Name = "chkRegSql";
            this.chkRegSql.Size = new System.Drawing.Size(179, 17);
            this.chkRegSql.TabIndex = 17;
            this.chkRegSql.Text = "Register ASP.NET Memberships";
            this.chkRegSql.UseVisualStyleBackColor = true;
            // 
            // chkInitializeData
            // 
            this.chkInitializeData.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.chkInitializeData, 2);
            this.chkInitializeData.Location = new System.Drawing.Point(434, 191);
            this.chkInitializeData.Name = "chkInitializeData";
            this.chkInitializeData.Size = new System.Drawing.Size(87, 17);
            this.chkInitializeData.TabIndex = 19;
            this.chkInitializeData.Text = "Initialize data";
            this.chkInitializeData.UseVisualStyleBackColor = true;
            // 
            // chkSyncProcedures
            // 
            this.chkSyncProcedures.AutoSize = true;
            this.chkSyncProcedures.Checked = true;
            this.chkSyncProcedures.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tableLayoutPanel1.SetColumnSpan(this.chkSyncProcedures, 2);
            this.chkSyncProcedures.Location = new System.Drawing.Point(434, 221);
            this.chkSyncProcedures.Name = "chkSyncProcedures";
            this.chkSyncProcedures.Size = new System.Drawing.Size(138, 17);
            this.chkSyncProcedures.TabIndex = 20;
            this.chkSyncProcedures.Text = "Sync stored procedures";
            this.chkSyncProcedures.UseVisualStyleBackColor = true;
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
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Assemblies|*.exe;*.dll|All files|*.*";
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
            this.tabDrops.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dropsGV)).EndInit();
            this.tabAlters.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.altersGV)).EndInit();
            this.tabOptions.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
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
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox searchDropsTB;
        private System.Windows.Forms.DataGridView dropsGV;
        private System.Windows.Forms.Button ignoreDropsButton;
        private System.Windows.Forms.Button dropSelectedButton;
        private System.Windows.Forms.TabPage tabAlters;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox filterAltersTB;
        private System.Windows.Forms.Button ignoreAltersButton;
        private System.Windows.Forms.Button alterSelectedButton;
        private System.Windows.Forms.TabPage tabOptions;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Button cancelOptionsButton;
        private System.Windows.Forms.Button saveOptionsButton;
        private System.Windows.Forms.DataGridView altersGV;
        private System.Windows.Forms.DataGridViewTextBoxColumn altersGVName;
        private System.Windows.Forms.DataGridViewTextBoxColumn altersGVScript;
        private System.Windows.Forms.DataGridViewButtonColumn altersGVRunButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn dropsGVName;
        private System.Windows.Forms.DataGridViewTextBoxColumn dropsGVScript;
        private System.Windows.Forms.DataGridViewButtonColumn dropsGVRunButton;
    }
}

