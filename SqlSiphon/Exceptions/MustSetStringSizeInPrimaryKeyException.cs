namespace SqlSiphon
{
    public class MustSetStringSizeInPrimaryKeyException : TableColumnRulesException
    {
        public MustSetStringSizeInPrimaryKeyException(Mapping.TableAttribute table, Mapping.ColumnAttribute[] columns)
            : base(table, columns, "has string columns that do not have a set size")
        {
        }
    }
}
