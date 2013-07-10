using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Daedalus
{
    static class MapIt
    {
        static void Main(string[] args)
        {
#if DEBUG
            File.WriteAllText("output.sql", ProcessFile(
@"#Customers
    [LCODE]
    Line1 string
    Line2 string
    City string
    State string
    ZipCode string

--#Items
    [LCODE]
    Price decimal

--#Invoices
    [SPK]
    DateCreated datetime
    fk Customers

--#ItemsInInvoices
    fk Invoices pk
    fk Items pk
    Count int",
                "Inventory", AuthType.None, false));

            System.Diagnostics.Process.Start("notepad.exe", "output.sql");
            Console.ReadKey();
#else
            if (args.Length >= 1)
            {
                var file = new FileInfo(args[0]);
                if (file.Exists)
                {
                    string ns = null;
                    if (args.Length == 2)
                        ns = args[1];
                    var script = ProcessFile(File.ReadAllText(args[0]), ns, AuthType.None, false);
                    var filename = string.Format("{0}_alt.sql", Path.GetFileNameWithoutExtension(args[0]));
                    File.WriteAllText(filename, script, Encoding.UTF8);
                }
                else Console.Error.WriteLine("{0} is not a valid file", args[0]);
            }
            else
                Console.Error.WriteLine("Usage: daedalus <inputfile> [namespace]");
#endif
        }

        static string ProcessFile(string input, string Namespace, AuthType auth, bool makeAuditTables)
        {
            var boundaries = FindAllTableBoundaries(input);
            var sb = new StringBuilder();
            var tables = new Dictionary<string, Table>();

            if (auth == AuthType.AspNet)
            {
                // this is a phantom table that will not be included in the generated scripts to 
                // enable interop with the ASP.NET Memberships system.
                tables.Add("dbo.aspnet_Users", Table.ParseTable(
@"#&aspnet_Users
    UserId Guid pk", auth));
            }
            var schemas = new List<string>();

            foreach (var pair in boundaries)
            {
                var chunk = input.Substring(pair[0], pair[1]);
                var table = Table.ParseTable(chunk.Trim(), auth);
                tables.Add(table.FullName, table);
            }

            var origTables = tables.Values
                .Where(table => table.IncludeInScript)
                .ToList();

            foreach (var table in origTables)
            {
                table.AddColumnsForForeignKeys(tables);
                if (!schemas.Contains(table.Schema) && table.Schema != Table.DefaultSchema)
                    schemas.Add(table.Schema);
            }

            foreach (var schema in schemas)
                sb.AppendLine(CreateSchema(schema));

            // the individual actions of dropping constraints, dropping/creating tables, and adding constraints must be done
            // wholey before the next can continue
            origTables
                .ForEach(table => sb.AppendLine(table.GetDropConstraintsText(tables)));

            origTables
                .ForEach(table => sb.AppendLine(table.GetCreateTableText(tables)));

            origTables
                .ForEach(table => sb.AppendLine(table.GetAddConstraintsText(tables)));

            
            return sb.ToString();
        }

        static string CreateSchema(string schema)
        {
            return string.Format(
@"if not exists(select * from information_schema.schemata where schema_name = '{0}') create schema {0};
go", schema);
        }
        static List<int[]> FindAllTableBoundaries(string input)
        {
            var output = new List<int[]>();
            int start = input.IndexOf('#');
            int end;
            while ((end = input.IndexOf('#', start + 1)) >= 0)
            {
                output.Add(new int[] { start, end - start });
                start = end;
            }
            output.Add(new int[] { start, input.Length - start });
            return output;
        }
    }
    enum AuthType
    {
        None,
        AspNet
    }
}
