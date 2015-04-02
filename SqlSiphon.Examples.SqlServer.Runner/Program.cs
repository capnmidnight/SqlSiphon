using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Examples.SqlServer.Runner
{
    [Mapping.Table]
    public class MyTable : BoundObject
    {
        public int MyNumber { get { return Get<int>(); } set { Set(value); } }
        public string MyWord { get { return Get<string>(); } set { Set(value); } }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine
            (
                Begin.Declare<int>("count")
                    .Declare<string>("asdf")
                .From<MyTable>()
                    .InnerJoin<MyTable, int>("a", a => a.MyNumber, Ops.LessThan, MyTable => MyTable.MyNumber)
                        .And<MyTable, MyTable, string>(a => a.MyWord, Ops.Equal, MyTable => MyTable.MyWord)
                        .Or<MyTable, MyTable, int>(a => a.MyNumber, Ops.GreaterThan, MyTable => MyTable.MyNumber)
                .Select<MyTable, int>(a => a.MyNumber)
                    ._<MyTable, string>(a => a.MyWord)
                    .Min<MyTable, int>(MyTable => MyTable.MyNumber, "MinMyNumber")
                    .Max<MyTable, string>(a => a.MyWord, "MaxWord")
                .Where<MyTable, int>(a => a.MyNumber, Ops.Equal, 2)
                    .And<MyTable, string>(a => a.MyWord, Ops.Equal, "asdf")
            );
        }
    }
}
