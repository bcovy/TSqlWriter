using SqlWriter.Compilers;
using System.Reflection;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Builders.Truncate;

public class TruncateBuilder : BuilderBase, ITruncateTable
{
    private readonly string _concatSql;
    private readonly string _tableName;
    private readonly string _parameterPrefix;

    public TruncateBuilder(Type entityType, IParameterManager parameterManager, string parameterPrefix = "p", string concatSqlStatement = "")
        : base(parameterManager)
    {
        var table = entityType.GetCustomAttribute<TableNameAttribute>()
            ?? throw new MissingFieldException($"The entity {entityType.Name} is missing the required TableName attribute.");

        _parameterPrefix = parameterPrefix;
        _concatSql = concatSqlStatement;
        _tableName = table.Name;
    }

    public string GetSqlStatement()
    {
        return BuildStatement();
    }

    public IConcatSql Concat()
    {
        return new ConcatSql(ParameterManager, BuildStatement(), _parameterPrefix);
    }

    private string BuildStatement()
    {
        return !string.IsNullOrEmpty(_concatSql) ? $"{_concatSql};\nTRUNCATE TABLE {_tableName}" : $"TRUNCATE TABLE {_tableName}";
    }
}
