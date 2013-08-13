using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SqlSiphon;
using SqlSiphon.Mapping;
using SqlSiphon.Postgres;

namespace Sandbox
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            using (var dal = new Dal())
            {
                textBox1.Text = string.Join(
                    Environment.NewLine + "==============================" + Environment.NewLine,
                    dal.GetAllStoredProcedureScripts().ToArray());
            }
        }
    }

    [MappedType(SqlType = "table test_table")]
    class Name
    {
        public int id { get; set; }
        public string name { get; set; }

        public Name()
        {
        }
    }

    class Dal : DataAccessLayer
    {
        public Dal()
            : base("Server=127.0.0.1;Port=5432;Database=sqlsiphontest;User Id=sqlsiphontest;Password=sqlsiphontest;") { }


        [MappedMethod(
            CommandType = CommandType.StoredProcedure,
            Query = "select * from test_table order by id")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        public List<Name> GetAllNames()
        {
            return this.GetList<Name>();
        }

        //[MappedMethod]
        //[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        //public List<Name> GetAllNames()
        //{
        //    return this.GetList<Name>();
        //}

        //[MappedMethod(
        //    CommandType = CommandType.Text,
        //    Query = "select * from test_table order by id")]
        //[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        //public List<string> GetNamesPrimitiveByName()
        //{
        //    return this.GetList<string>("name");
        //}

        //[MappedMethod(
        //    CommandType = CommandType.Text,
        //    Query = "select * from test_table order by id")]
        //[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        //public List<string> GetNamesPrimitiveByIndex()
        //{
        //    return this.GetList<string>(1);
        //}

        //public List<Name> GetNamesFromTextQuery()
        //{
        //    return this.GetListQuery<Name>("select * from test_table order by id");
        //}

        //[MappedMethod(
        //    CommandType = CommandType.Text,
        //    Query = "select * from test_table where id = :id")]
        //[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        //public Name GetName(int id)
        //{
        //    return this.Get<Name>(id);
        //}


        //[MappedMethod(
        //    CommandType = CommandType.Text,
        //    Query = "select * from test_table where id >= :id limit 1")]
        //[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        //public Name FindName(int id)
        //{
        //    return this.Get<Name>(id);
        //}

        //[MappedMethod(
        //    CommandType = CommandType.Text,
        //    Query = "select * from test_table where id = :id")]
        //[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        //public string GetNamePrimitiveByName(int id)
        //{
        //    return this.Get<string>("name", id);
        //}

        //[MappedMethod(
        //    CommandType = CommandType.Text,
        //    Query = "select * from test_table where id = :id")]
        //[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        //public string GetNamePrimitiveByIndex(int id)
        //{
        //    return this.Get<string>(1, id);
        //}

        //public Name GetNameFromTextQuery()
        //{
        //    return this.GetQuery<Name>("select * from test_table where id = 4");
        //}

        //[MappedMethod(
        //    CommandType = CommandType.Text,
        //    Query = "update test_table set name = :name where id = :id")]
        //[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        //public void UpdateName(int id, string name)
        //{
        //    this.Execute(id, name);
        //}

        //[MappedMethod(
        //    CommandType = CommandType.Text,
        //    Query = "delete from test_table where id >= :id")]
        //[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        //public void DeleteName(int id)
        //{
        //    this.Execute(id);
        //}

        //[MappedMethod(
        //    CommandType = CommandType.Text,
        //    Query = "insert into test_table(name) values(:name)")]
        //[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.Synchronized)]
        //public void InsertName(string name)
        //{
        //    this.Execute(name);
        //}

    }
}
