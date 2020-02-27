namespace SqlSiphon
{
    public interface ISqlSiphon :
        IDataConnector,
        IDatabaseStateReader,
        IDatabaseStateWriter,
        IAssemblyStateReader,
        IDatabaseScriptGenerator
    {
    }
}
