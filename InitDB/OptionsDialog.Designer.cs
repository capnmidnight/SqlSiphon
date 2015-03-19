namespace InitDB
{
    partial class OptionsDialog
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
            this.defaultObjFilterTB = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.browseSqlCmdButton = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.sqlcmdTB = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.browsePsqlButton = new System.Windows.Forms.Button();
            this.psqlTB = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // defaultObjFilterTB
            // 
            this.defaultObjFilterTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.defaultObjFilterTB.Location = new System.Drawing.Point(117, 58);
            this.defaultObjFilterTB.Name = "defaultObjFilterTB";
            this.defaultObjFilterTB.Size = new System.Drawing.Size(439, 20);
            this.defaultObjFilterTB.TabIndex = 19;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(13, 61);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(98, 13);
            this.label13.TabIndex = 18;
            this.label13.Text = "Default object filter:";
            // 
            // browseSqlCmdButton
            // 
            this.browseSqlCmdButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseSqlCmdButton.Location = new System.Drawing.Point(562, 4);
            this.browseSqlCmdButton.Name = "browseSqlCmdButton";
            this.browseSqlCmdButton.Size = new System.Drawing.Size(80, 23);
            this.browseSqlCmdButton.TabIndex = 9;
            this.browseSqlCmdButton.Text = "Browse...";
            this.browseSqlCmdButton.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(99, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "SQLCMD tool path:";
            // 
            // sqlcmdTB
            // 
            this.sqlcmdTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sqlcmdTB.Location = new System.Drawing.Point(117, 6);
            this.sqlcmdTB.Name = "sqlcmdTB";
            this.sqlcmdTB.Size = new System.Drawing.Size(439, 20);
            this.sqlcmdTB.TabIndex = 8;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(567, 102);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 13;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(486, 102);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 14;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(28, 35);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(82, 13);
            this.label11.TabIndex = 15;
            this.label11.Text = "PSQL tool path:";
            // 
            // browsePsqlButton
            // 
            this.browsePsqlButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browsePsqlButton.Location = new System.Drawing.Point(562, 30);
            this.browsePsqlButton.Name = "browsePsqlButton";
            this.browsePsqlButton.Size = new System.Drawing.Size(80, 23);
            this.browsePsqlButton.TabIndex = 16;
            this.browsePsqlButton.Text = "Browse...";
            this.browsePsqlButton.UseVisualStyleBackColor = true;
            // 
            // psqlTB
            // 
            this.psqlTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.psqlTB.Location = new System.Drawing.Point(117, 32);
            this.psqlTB.Name = "psqlTB";
            this.psqlTB.Size = new System.Drawing.Size(439, 20);
            this.psqlTB.TabIndex = 17;
            // 
            // OptionsDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(654, 137);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.browseSqlCmdButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.defaultObjFilterTB);
            this.Controls.Add(this.browsePsqlButton);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.sqlcmdTB);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.psqlTB);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsDialog";
            this.ShowInTaskbar = false;
            this.Text = "Options";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox defaultObjFilterTB;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button browseSqlCmdButton;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox sqlcmdTB;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button browsePsqlButton;
        private System.Windows.Forms.TextBox psqlTB;
    }
}