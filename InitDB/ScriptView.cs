using System;
using System.Drawing;
using System.Windows.Forms;

using ScintillaNET;

namespace InitDB
{
    public partial class ScriptView : Form
    {
        public ScriptView()
        {
            InitializeComponent();
            scintilla.StyleResetDefault();
            scintilla.Styles[Style.Default].Font = "Consolas";
            scintilla.Styles[Style.Default].Size = 10;
            scintilla.StyleClearAll();

            scintilla.Styles[Style.Sql.Character].ForeColor = Color.Maroon;
            scintilla.Styles[Style.Sql.String].ForeColor = Color.Maroon;

            scintilla.Styles[Style.Sql.Comment].ForeColor = Color.DarkGreen;
            scintilla.Styles[Style.Sql.CommentDoc].ForeColor = Color.DarkGreen;
            scintilla.Styles[Style.Sql.CommentDocKeyword].ForeColor = Color.DarkGreen;
            scintilla.Styles[Style.Sql.CommentDocKeywordError].ForeColor = Color.DarkGreen;
            scintilla.Styles[Style.Sql.CommentLine].ForeColor = Color.DarkGreen;
            scintilla.Styles[Style.Sql.CommentLineDoc].ForeColor = Color.DarkGreen;
            scintilla.Styles[Style.Sql.SqlPlusComment].ForeColor = Color.DarkGreen;

            scintilla.Styles[Style.Sql.Identifier].ForeColor = Color.Black;
            scintilla.Styles[Style.Sql.Number].ForeColor = Color.Black;
            scintilla.Styles[Style.Sql.Operator].ForeColor = Color.Black;
            scintilla.Styles[Style.Sql.QOperator].ForeColor = Color.Black;

            scintilla.Styles[Style.Sql.QuotedIdentifier].ForeColor = Color.DarkBlue;
            scintilla.Styles[Style.Sql.SqlPlus].ForeColor = Color.DarkBlue;
            scintilla.Styles[Style.Sql.SqlPlusPrompt].ForeColor = Color.DarkBlue;
            scintilla.Styles[Style.Sql.Word].ForeColor = Color.DarkBlue;
            scintilla.Styles[Style.Sql.Word2].ForeColor = Color.DarkRed;

            var keywords = "add external procedure all fetch public alter file " +
                "raiserror and fillfactor read any for readtext as foreign reconfigure " +
                "asc freetext references authorization freetexttable replication backup " +
                "from restore begin full restrict between function return break goto " +
                "revert browse grant revoke bulk group right by having rollback " +
                "cascade holdlock rowcount case identity rowguidcol check identity_insert " +
                "rule checkpoint identitycol save close if schema clustered in " +
                "securityaudit coalesce index select collate inner semantickeyphrasetable " +
                "column insert semanticsimilaritydetailstable commit intersect " +
                "semanticsimilaritytable compute into session_user constraint is set " +
                "contains join setuser containstable key shutdown continue kill some " +
                "convert left statistics create like system_user cross lineno table " +
                "current load tablesample current_date merge textsize current_time " +
                "national then current_timestamp nocheck to current_user nonclustered top " +
                "cursor not tran database null transaction dbcc nullif trigger deallocate " +
                "of truncate declare off try_convert default offsets tsequal delete on " +
                "union deny open unique desc opendatasource unpivot disk openquery update " +
                "distinct openrowset updatetext distributed openxml use double option " +
                "user drop or values dump order varying else outer view end over waitfor " +
                "errlvl percent when escape pivot where except plan while exec precision " +
                "with execute primary within group exists print writetext exit proc";
            scintilla.SetKeywords(0, keywords);
            scintilla.SetKeywords(1, "bool bit byte char short int long bigint real float varchar nvarchar varbinary uniqueidentifier datetime datetime2");
            scintilla.Margins[0].Width = 16;
        }

        public void Prompt(string title, string text)
        {
            Text = title;
            DialogResult = DialogResult.Cancel;
            scintilla.Text = text;
            Show();
        }

        public void Prompt(string title, string text, Action<string> callback)
        {
            FormClosing += (o, e) =>
            {
                if (DialogResult == DialogResult.OK)
                {
                    callback(scintilla.Text);
                }
            };
            Prompt(title, text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }
    }
}
