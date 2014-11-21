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
    public partial class ScriptView : Form
    {
        public ScriptView()
        {
            InitializeComponent();
        }

        public string Prompt(string text)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.textBox1.Text = text;
            return this.ShowDialog() == System.Windows.Forms.DialogResult.OK
                ? this.textBox1.Text
                : text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}
