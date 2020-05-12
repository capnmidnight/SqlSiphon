using SqlSiphon.Mapping;

namespace SqlSiphon.Postgres.PgCatalog
{
    [View(
        Include = false,
        Schema = "pg_catalog",
        Name = "pg_user")]
    internal class User
    {
        [Column(Name = "usename")]
        public string Name { get; set; }

        [Column(Name = "useconfig")]
        public string[] Config { get; set; }
    }
}
