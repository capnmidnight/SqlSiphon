using System;
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
                browseSqlCmdButton.Click += value;
            }
            remove
            {
                browseSqlCmdButton.Click -= value;
            }
        }

        public event EventHandler BrowsePSQLPathClick
        {
            add
            {
                browsePsqlButton.Click += value;
            }
            remove
            {
                browsePsqlButton.Click -= value;
            }
        }

        public string SQLCMDPath
        {
            get
            {
                return sqlcmdTB.Text;
            }
            set
            {
                sqlcmdTB.Text = value;
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
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }
    }
}
