using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter;

public interface ISubquery
{
    string ParameterPrefix { get; }
    Prefix ConditionPrefix { get; }
    Predicates ConditionPredicate { get; }
    string GetSqlStatement();
    public IEnumerable<IParameterModel> Parameters { get; }
    SqlParameter[] GetSqlParameters { get; }
    IDictionary<string, object> GetParameters { get; }
}

public interface ISubquery<TSub> : ISubquery where TSub : class
{
    ISubquery<TSub> Select<T>(Expression<Func<TSub, T>> column);
    ISubquery<TSub> Avg<T>(Expression<Func<TSub, T>> column);
    ISubquery<TSub> Min<T>(Expression<Func<TSub, T>> column);
    ISubquery<TSub> Max<T>(Expression<Func<TSub, T>> column);
    ISubquery<TSub> Raw(string statement);
    ISubquery<TSub> Count();
    ISubquery<TSub> Where(Expression<Func<TSub, bool>> expression);
}