using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SqlSiphon.Examples.Tokenizer
{
    class DisplayRow
    {
        public string Name { get; private set; }
        public object Value { get; private set; }
        public string Type { get { return this.Value.GetType().FullName; } }
        public DisplayRow(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.dataGridView1.AutoGenerateColumns = false;
        }

        public List<DisplayRow> DataSource
        {
            get
            {
                return (List<DisplayRow>)this.dataGridView1.DataSource;
            }
            set
            {
                this.dataGridView1.DataSource = value;
            }
        }
    }
}
