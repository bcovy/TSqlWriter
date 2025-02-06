using System.Data;
using SqlWriter.Compilers;
using SqlWriter.Components.Parameters;
using SqlWriter.Infrastructure;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Builders.RawSql;

public class RawSqlBuilder(IParameterManager parameterManager, string sqlStatement, string parameterPrefix = "p", string concatSqlStatement = "")
    : BuilderBase(parameterManager), IRawSql
{
    public string GetSqlStatement()
    {
        return BuildSqlStatement();
    }
    
    public IConcatSql Concat()
    {
        return new ConcatSql(ParameterManager, BuildSqlStatement(), parameterPrefix);
    }

    private string BuildSqlStatement()
    {
        return !string.IsNullOrEmpty(concatSqlStatement) ? $"{concatSqlStatement};\n{sqlStatement}" : sqlStatement;
    }
    
    public IRawSql AddParameter<TParam>(TParam value, string parameterName)
    {
        SqlDbType dbType = value.TranslateSqlDbType();
        AddParam(value, parameterName, dbType);
        return this;
    }
    
    public IRawSql AddParameter<TParam>(TParam value, string parameterName, SqlDbType dbType)
    {
        AddParam(value, parameterName, dbType);
        return this;
    }

    private void AddParam<TParam>(TParam value, string parameterName, SqlDbType dbType)
    {
        ParameterManager.AddParameters([new ParameterModel<TParam>(value, parameterName, dbType)]);
    }
}