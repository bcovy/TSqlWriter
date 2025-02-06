using SqlWriter.Components.Parameters;
using SqlWriter.Components.Tables;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Integration.Fixtures;

public static class ManagersFixture
{
    public static ITablesManager GetTablesManager(bool addSecondTable = false)
    {
        var manager = new TablesManager(typeof(QueryableMod1), "a");
        
        if (addSecondTable)
            manager.AddTable<QueryableMod2>("b");
        
        return manager;
    }

    public static ParameterManager GetParameterManager() => new ParameterManager();
}