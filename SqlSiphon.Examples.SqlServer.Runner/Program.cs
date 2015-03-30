using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSiphon.Examples.SqlServer.Runner
{
    class Program
    {
        private static string[] Roles = new string[]{
            "User",
            "Admin",
            "Banned"
        };
        static void Main(string[] args)
        {
            using (var db = new SqlServerDAL("localhost\\SQLEXPRESS", "TestDB", "TestDBUser", "TestDBPassword"))
            {
                var roles = db.GetAllRoles();
                Console.WriteLine("Currently {0} roles", roles.Length);
                if (roles.Length < Program.Roles.Length)
                {
                    if (roles.Length == 0)
                    {
                        db.CreateApplication("TestDBApplication", "hokay mang");
                    }
                    foreach (var role in from r in Roles where !roles.Contains(r) select r)
                    {
                        db.CreateRole(role, "TestDBApplication", "");
                    }
                    var old = roles.Length;
                    roles = db.GetAllRoles();
                    Console.WriteLine("{0} roles created", roles.Length - old);
                }
                foreach (var role in roles)
                {
                    Console.WriteLine(role);
                }
            }
        }
    }
}
