﻿using System;
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

        public void Prompt(string text)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.textBox1.Text = text;
            this.Show();
        }

        public void Prompt(string text, Action<string> callback)
        {
            this.FormClosing += (o, e) =>
            {
                if (this.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    callback(this.textBox1.Text);
                }
            };
            this.Prompt(text);
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
