namespace SqlSiphon
{
    public interface IDatabaseObjectHandler
    {
        string DefaultSchemaName { get; }
        string MakeIdentifier(params string[] parts);
    }
}
