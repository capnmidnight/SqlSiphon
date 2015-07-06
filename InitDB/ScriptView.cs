using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScintillaNET;

namespace InitDB
{
    public partial class ScriptView : Form
    {
        public ScriptView()
        {
            InitializeComponent();
            this.scintilla.StyleResetDefault();
            this.scintilla.Styles[Style.Default].Font = "Consolas";
            this.scintilla.Styles[Style.Default].Size = 10;
            this.scintilla.StyleClearAll();

            this.scintilla.Styles[Style.Sql.Character].ForeColor = Color.Maroon;
            this.scintilla.Styles[Style.Sql.String].ForeColor = Color.Maroon;

            this.scintilla.Styles[Style.Sql.Comment].ForeColor = Color.DarkGreen;
            this.scintilla.Styles[Style.Sql.CommentDoc].ForeColor = Color.DarkGreen;
            this.scintilla.Styles[Style.Sql.CommentDocKeyword].ForeColor = Color.DarkGreen;
            this.scintilla.Styles[Style.Sql.CommentDocKeywordError].ForeColor = Color.DarkGreen;
            this.scintilla.Styles[Style.Sql.CommentLine].ForeColor = Color.DarkGreen;
            this.scintilla.Styles[Style.Sql.CommentLineDoc].ForeColor = Color.DarkGreen;
            this.scintilla.Styles[Style.Sql.SqlPlusComment].ForeColor = Color.DarkGreen;

            this.scintilla.Styles[Style.Sql.Identifier].ForeColor = Color.Black;
            this.scintilla.Styles[Style.Sql.Number].ForeColor = Color.Black;
            this.scintilla.Styles[Style.Sql.Operator].ForeColor = Color.Black;
            this.scintilla.Styles[Style.Sql.QOperator].ForeColor = Color.Black;

            this.scintilla.Styles[Style.Sql.QuotedIdentifier].ForeColor = Color.DarkBlue;
            this.scintilla.Styles[Style.Sql.SqlPlus].ForeColor = Color.DarkBlue;
            this.scintilla.Styles[Style.Sql.SqlPlusPrompt].ForeColor = Color.DarkBlue;
            this.scintilla.Styles[Style.Sql.Word].ForeColor = Color.DarkBlue;
            this.scintilla.Styles[Style.Sql.Word2].ForeColor = Color.DarkBlue;
            this.scintilla.Styles[Style.Sql.User1].ForeColor = Color.DarkBlue;
            this.scintilla.Styles[Style.Sql.User2].ForeColor = Color.DarkBlue;
            this.scintilla.Styles[Style.Sql.User3].ForeColor = Color.DarkBlue;
            this.scintilla.Styles[Style.Sql.User4].ForeColor = Color.DarkBlue;

            var keywords = "ADD EXTERNAL PROCEDURE ALL FETCH PUBLIC ALTER FILE RAISERROR AND FILLFACTOR READ ANY FOR READTEXT AS FOREIGN RECONFIGURE ASC FREETEXT REFERENCES AUTHORIZATION FREETEXTTABLE REPLICATION BACKUP FROM RESTORE BEGIN FULL RESTRICT BETWEEN FUNCTION RETURN BREAK GOTO REVERT BROWSE GRANT REVOKE BULK GROUP RIGHT BY HAVING ROLLBACK CASCADE HOLDLOCK ROWCOUNT CASE IDENTITY ROWGUIDCOL CHECK IDENTITY_INSERT RULE CHECKPOINT IDENTITYCOL SAVE CLOSE IF SCHEMA CLUSTERED IN SECURITYAUDIT COALESCE INDEX SELECT COLLATE INNER SEMANTICKEYPHRASETABLE COLUMN INSERT SEMANTICSIMILARITYDETAILSTABLE COMMIT INTERSECT SEMANTICSIMILARITYTABLE COMPUTE INTO SESSION_USER CONSTRAINT IS SET CONTAINS JOIN SETUSER CONTAINSTABLE KEY SHUTDOWN CONTINUE KILL SOME CONVERT LEFT STATISTICS CREATE LIKE SYSTEM_USER CROSS LINENO TABLE CURRENT LOAD TABLESAMPLE CURRENT_DATE MERGE TEXTSIZE CURRENT_TIME NATIONAL THEN CURRENT_TIMESTAMP NOCHECK TO CURRENT_USER NONCLUSTERED TOP CURSOR NOT TRAN DATABASE NULL TRANSACTION DBCC NULLIF TRIGGER DEALLOCATE OF TRUNCATE DECLARE OFF TRY_CONVERT DEFAULT OFFSETS TSEQUAL DELETE ON UNION DENY OPEN UNIQUE DESC OPENDATASOURCE UNPIVOT DISK OPENQUERY UPDATE DISTINCT OPENROWSET UPDATETEXT DISTRIBUTED OPENXML USE DOUBLE OPTION USER DROP OR VALUES DUMP ORDER VARYING ELSE OUTER VIEW END OVER WAITFOR ERRLVL PERCENT WHEN ESCAPE PIVOT WHERE EXCEPT PLAN WHILE EXEC PRECISION WITH EXECUTE PRIMARY WITHIN GROUP EXISTS PRINT WRITETEXT EXIT PROC";
            this.scintilla.SetKeywords(0, keywords);
            this.scintilla.SetKeywords(1, keywords.ToLowerInvariant());

            this.scintilla.Margins[0].Width = 16;
        }

        public void Prompt(string text)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.scintilla.Text = text;
            this.Show();
        }

        public void Prompt(string text, Action<string> callback)
        {
            this.FormClosing += (o, e) =>
            {
                if (this.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    callback(this.scintilla.Text);
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
