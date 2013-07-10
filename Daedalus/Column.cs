using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daedalus
{
    [Serializable]
    class Column
    {
        public readonly string Name;
        public readonly string DotNetType;
        public readonly string DefaultValue;
        public readonly bool IsNullable;
        public readonly bool IsPrimaryKey;
        public readonly bool IsIdentity;
        public bool IsForeignKey;
        public readonly bool IsComment;
        public readonly string fkPrefix;

        public string SqlType
        {
            get
            {
                if (this.DotNetType.Equals("string"))
                    return "nvarchar(max)";
                else if (this.DotNetType.Equals("Guid"))
                    return "uniqueidentifier";
                else if (this.DotNetType.Equals("DateTime"))
                    return "datetime2";
                return this.DotNetType;
            }
        }

        public string ForeignKeyTarget
        {
            get
            {
                if (!this.IsForeignKey)
                    return null;

                if (this.SqlType.Contains('.'))
                    return SqlType;
                else
                    return Table.DefaultSchema + "." + this.SqlType;
            }
        }

        public Column(string name, string type, params string[] parts)
        {
            this.Name = name;
            this.DotNetType = type;
            if (this.Name.Equals("--"))
            {
                this.IsComment = true;
                this.DotNetType += " " + string.Join(" ", parts);
            }
            else
            {
                if (this.Name.StartsWith("?"))
                {
                    this.IsNullable = true;
                    this.Name = this.Name.Substring(1);
                }

                if (this.Name.ToLower().Equals("fk"))
                    this.IsForeignKey = true;

                this.DefaultValue = "";
                foreach (var part in parts)
                {
                    if (part.ToLower().Equals("pk"))
                        this.IsPrimaryKey = true;
                    else if (part.StartsWith("default"))
                        DefaultValue = part;
                    else if (part.StartsWith("ident"))
                        this.IsIdentity = true;
                    else if (this.IsForeignKey && this.fkPrefix == null)
                        this.fkPrefix = part;
                }
            }
        }

        public string GetColumnText()
        {
            return string.Format("{0} {1} {2} {3} {4}",
                this.Name,
                this.SqlType,
                this.IsNullable || this.IsComment ? "" : "not null",
                this.IsIdentity ? "identity(1, 1)" : "",
                this.DefaultValue).Trim();
        }
        public string ForeignKeyPrefix
        {
            get
            {
                if (this.IsForeignKey)
                {
                    var prefix = this.fkPrefix ?? ""; // this.SqlType;
                    if (prefix.Contains('.'))
                        prefix = prefix.Substring(prefix.IndexOf('.') + 1);
                    return prefix;
                }
                return null;
            }
        }

        public List<Column> CreateForeignKeyColumns(Dictionary<string, Table> tables)
        {
            var cols = new List<Column>();
            if (this.IsForeignKey)
            {
                var prefix = this.ForeignKeyPrefix;
                if (this.IsNullable)
                    prefix = "?" + prefix;

                foreach (var foreignColumn in tables[this.ForeignKeyTarget].Columns)
                {
                    if (foreignColumn.IsPrimaryKey)
                    {
                        var parts = new List<string>();
                        if (this.DefaultValue.Length > 0)
                            parts.Add(this.DefaultValue);
                        if (this.IsPrimaryKey)
                            parts.Add("pk");

                        var col = new Column(prefix + foreignColumn.Name, foreignColumn.DotNetType, parts.ToArray());
                        cols.Add(col);
                    }
                }
            }
            return cols;
        }

        public string GetAddConstraintsText(Table parentTable, Dictionary<string, Table> tables)
        {
            if (this.IsForeignKey)
            {
                var cols = new List<string>();
                cols.AddRange(from fcol in tables[this.ForeignKeyTarget].Columns where fcol.IsPrimaryKey select this.ForeignKeyPrefix + fcol.Name);

                return string.Format("alter table {5} add constraint FK_{0}_{6}{1} foreign key({2}) references {3}({4})",
                    parentTable.FullName.Replace('.', '_'),
                    this.ForeignKeyTarget.Replace('.', '_'),
                    string.Join(", ", cols.ToArray()),
                    this.ForeignKeyTarget,
                    tables[this.ForeignKeyTarget].PrimaryKey,
                    parentTable.FullName,
                    this.ForeignKeyPrefix);
            }
            return null;
        }


        public string GetDropConstraintsText(Table parentTable, Dictionary<string, Table> tables)
        {
            if (this.IsForeignKey)
                return string.Format("if exists(select * from information_schema.referential_constraints where constraint_name = 'FK_{1}_{3}{2}') alter table {0} drop constraint FK_{1}_{3}{2};",
                    parentTable.FullName,
                    parentTable.FullName.Replace('.', '_'),
                    this.ForeignKeyTarget.Replace('.', '_'),
                    this.ForeignKeyPrefix);
            return null;
        }

        public static Column ParseColumn(string line, string table)
        {
            if (line.Equals("[SPK]"))
                return new Column(table + "ID", "int", "pk", "ident");
            else if (line.Equals("[LCODENAME]"))
                return new Column(table + "Name", "string");
            else
            {
                var parts = line.Split(' ');
                if (parts.Length < 2)
                    throw new FormatException("Invalid row specification: " + line);

                var columnName = parts[0];
                var type = parts[1];
                return new Column(columnName, type, parts.Skip(2).ToArray());
            }
        }

        public string GetCSharpText()
        {
            if (this.IsComment)
                return string.Format("// {0}", this.DotNetType);

            return string.Format("public {0} {1} {get; set;}",
                this.DotNetType,
                this.Name);
        }
    }
}
