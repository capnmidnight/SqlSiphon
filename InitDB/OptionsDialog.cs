using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SqlSiphon;

namespace InitDB
{
    public partial class OptionsDialog : Form
    {
        private readonly Dictionary<string, TextBox> textBoxes = new Dictionary<string, TextBox>();
        private readonly List<Control> controls = new List<Control>();

        public OptionsDialog()
        {
            InitializeComponent();
            Disposed += OptionsDialog_Disposed;
        }

        public void SetTypes(Type[] types)
        {
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.RowCount = types.Length + 1;
            tableLayoutPanel1.RowStyles.Clear();
            for (var i = 0; i < types.Length; ++i)
            {
                var row = new RowStyle(SizeType.Absolute, 30);
                tableLayoutPanel1.RowStyles.Add(row);
            }

            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var y = 0;
            foreach (var type in types)
            {
                var name = DataConnector.GetDatabaseVendorName(type);

                var label = new Label
                {
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    Text = $"{name} path:"
                };
                controls.Add(label);
                tableLayoutPanel1.Controls.Add(label, 0, y);

                var txtBox = new TextBox
                {
                    Dock = DockStyle.Fill
                };
                textBoxes[name] = txtBox;
                controls.Add(txtBox);
                tableLayoutPanel1.Controls.Add(txtBox, 1, y);
                
                var btn = new Button();
                controls.Add(btn);
                btn.Text = "Browse";
                btn.Name = name;
                btn.Click += Btn_Click;
                btn.Dock = DockStyle.Fill;

                tableLayoutPanel1.Controls.Add(btn, 2, y);
                ++y;
            }

            tableLayoutPanel1.ResumeLayout();
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                BrowseCommandPathClick?.Invoke(this, new BrowseCommandPathEventArgs(btn.Name, textBoxes[btn.Name].Text));
            }
        }

        private void OptionsDialog_Disposed(object sender, EventArgs e)
        {
            foreach (var control in controls)
            {
                control.Dispose();
            }

            controls.Clear();
            textBoxes.Clear();
        }

        public void SetPath(string name, string path)
        {
            textBoxes[name].Text = path;
        }

        public string GetPath(string name)
        {
            return textBoxes[name].Text;
        }

        public event BrowseCommandPathEventHandler BrowseCommandPathClick;

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
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
