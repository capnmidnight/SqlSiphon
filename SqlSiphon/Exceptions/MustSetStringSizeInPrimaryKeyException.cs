namespace SqlSiphon
{
    public class MustSetStringSizeInPrimaryKeyException : TableColumnRulesException
    {
        public MustSetStringSizeInPrimaryKeyException(Mapping.TableAttribute table, Mapping.ColumnAttribute[] columns)
            : base(
                "The table `{0}`.`{1}` defined by type `{2}` has string columns that do not have a set size.{3}{4}",
                table,
                columns)
        {
        }
    }
}
