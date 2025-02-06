using System.Reflection;

namespace SqlWriter.Infrastructure;

public static class TableNameHelper
{
    public static string GetName(Type entity)
    {
        var table = entity.GetCustomAttribute<TableNameAttribute>();

        if (table != null)
            return table.Name;

        var variable = entity.GetCustomAttribute<TableVariableAttribute>()
            ?? throw new MissingFieldException($"The entity {entity.Name} is missing the required TableName attribute.");

        return variable.Name;
    }

    public static (string, string) GetMetadata(Type entityType)
    {
        var table = entityType.GetCustomAttribute<TableNameAttribute>();

        if (table != null)
            return (table.Name, table.PrimaryKeyField);

        var variable = entityType.GetCustomAttribute<TableVariableAttribute>()
            ?? throw new MissingFieldException($"The entity {entityType.Name} is missing the required TableName attribute.");

        return (variable.Name, variable.PrimaryKeyField);
    }
}
