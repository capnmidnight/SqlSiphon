using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InitDB
{
    public partial class OptionsDialog : Form
    {
        public OptionsDialog()
        {
            InitializeComponent();
        }

        public event EventHandler BrowseSQLCMDPathClick
        {
            add
            {
                this.browseSqlCmdButton.Click += value;
            }
            remove
            {
                this.browseSqlCmdButton.Click -= value;
            }
        }

        public event EventHandler BrowsePSQLPathClick
        {
            add
            {
                this.browsePsqlButton.Click += value;
            }
            remove
            {
                this.browsePsqlButton.Click -= value;
            }
        }

        public string SQLCMDPath
        {
            get
            {
                return this.sqlcmdTB.Text;
            }
            set
            {
                this.sqlcmdTB.Text = value;
            }
        }

        public string PSQLPath
        {
            get
            {
                return psqlTB.Text;
            }
            set
            {
                psqlTB.Text = value;
            }
        }

        public string DefaultObjectFilterRegexText
        {
            get
            {
                return defaultObjFilterTB.Text;
            }
            set
            {
                defaultObjFilterTB.Text = value;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}
