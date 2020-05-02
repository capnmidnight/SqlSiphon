namespace SqlSiphon
{
    public class PrimaryKeyColumnNotNullableException : TableColumnRulesException
    {
        public PrimaryKeyColumnNotNullableException(Mapping.TableAttribute table, Mapping.ColumnAttribute[] columns)
            : base(
                table,
                columns,
                "has one or more nullable columns in the primary key")
        {
        }
    }
}
