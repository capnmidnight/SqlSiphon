using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SqlSiphon;
using SqlSiphon.Mapping;
using SqlSiphon.Postgres;

namespace SqlSiphon.Examples.Tokenizer
{
    class DAL : SqlSiphon.Postgres.PostgresDataAccessLayer
    {
        public DAL()
            : base("localhost", "testdb", "postgres", "ppyptky7")
        {
        }

        [Routine(CommandType = CommandType.Text,
            Query = "select * from test_column_types;")]
        public List<DisplayRow> GetTestColumns()
        {
            var rows = new List<DisplayRow>();
            using (var reader = this.GetReader())
            {
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; ++i)
                    {
                        rows.Add(new DisplayRow(reader.GetName(i), reader.GetValue(i)));
                    }
                }
            }
            return rows;
        }
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (var form = new Form1())
            {
                using (var db = new DAL())
                {
                    form.DataSource = db.GetTestColumns();
                }
                Application.Run(form);
            }
        }
    }
}
