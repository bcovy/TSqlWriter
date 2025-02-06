using System.Linq.Expressions;
using SqlWriter.Components.Joins;
using SqlWriter.Components.Tables;

namespace SqlWriter.Interfaces.Internals;

public interface ITablesManager
{
    Dictionary<Type, TableModel> Tables { get; }
    bool ContainsEntity(Type entity);
    /// <summary>
    /// Returns table model for associated entity type.
    /// </summary>
    /// <param name="entityType">Entity type.</param>
    /// <returns><see cref="TableModel"/> that corresponds to <paramref name="entityType"/>.</returns>
    /// <exception cref="MissingMemberException">Entity type is not found in Tables dictionary.</exception>
    TableModel GetTable(Type entityType);
    /// <summary>
    /// Returns table model for associated entity type.  Method will perform null reference check.
    /// </summary>
    /// <param name="expression">Entity expression.</param>
    /// <returns><see cref="TableModel"/> that corresponds to <paramref name="expression"/>.</returns>
    /// <exception cref="MissingMemberException">Entity type is not found in Tables dictionary.</exception>
    TableModel GetTable(Expression? expression);
    ColumnModel GetColumn(Type entityType, string columnName);
    /// <summary>
    /// Add table entity to the manager's collection.
    /// </summary>
    /// <param name="alias">Table alias name.  If null, method will generate a random alias.</param>
    /// <typeparam name="TTable">Table entity.</typeparam>
    /// <returns><see cref="ITablesManager"/> to allow user to chain additional table additions.</returns>
    ITablesManager AddTable<TTable>(string alias = "") where TTable : class;
    void JoinWithMapper(Action<IJoinMapper> mapper);
    void AddJoin<TTable1, TTable2>(JoinType joinType) where TTable1 : class where TTable2 : class;
    void AddJoin(JoinType joinType, LambdaExpression expression);
    /// <summary>
    /// Add join for CTE table.  Parameter <paramref name="joinColumnName"/> identifies join column name for both tables. 
    /// </summary>
    /// <param name="joinColumnName">Expression that represents member name of join column for both tables.
    /// Assumes CTE SELECT statement includes column of same name.</param>
    /// <param name="joinType">Join type.</param>
    /// <param name="cteAlias">CTE alias name.</param>
    /// <returns>Name of join column to be used for both tables.</returns>
    string AddCteJoin(LambdaExpression joinColumnName, JoinType joinType, string cteAlias);
    JoinModel AddCteJoin(BinaryExpression binary, JoinType joinType, string cteName);
    string Compile();
}