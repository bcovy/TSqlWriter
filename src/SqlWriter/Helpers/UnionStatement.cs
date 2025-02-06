using SqlWriter.Builders.Query;
using SqlWriter.Components.Tables;

namespace SqlWriter;

public static class UnionStatement
{
    private static TablesManager CreateTablesManager<T>(string aliasName) where T : class
    {
        return new TablesManager(typeof(T), aliasName);
    }

    #region Query
    /// <summary>
    /// Allows user to build an SQL Query statement using a combination of entity classes to define the target table(s), 
    /// and raw input to define SELECT, WHERE, ORDER BY, and GROUP BY logic.  Builder is ideal for queries that involve 
    /// more than four tables.  NOTE: If <paramref name="dynamicWhere"/> is set to true, Where methods that do not have 
    /// an expression argument will not apply the WHERE condition if the parameter value is null.
    /// </summary>
    /// <remarks>
    /// All tables to be used in statement should be added via the Join methods before adding WHERE criteria.
    /// </remarks>
    /// <typeparam name="T">Parent table entity type.</typeparam>
    /// <param name="union">Parent Union statement.</param>
    /// <param name="alias">Parent table alias.</param>
    /// <param name="dynamicWhere">If set to true, methods with string column argument will not apply the WHERE condition if the parameter value is null.</param> 
    /// <returns><see cref="IQueryBuilder"/> for building SQL Select statement.</returns>
    public static IQueryBuilder QueryBuilder<T>(this IUnion union, string alias, bool dynamicWhere = false) where T : class
    {
        return new QueryBuilder(CreateTablesManager<T>(alias), union.ParameterManager, dynamicWhere, union.ParameterPrefix, union.SqlStatement);
    }
    /// <summary>
    /// Returns an <see cref="IQuery{T}"/> object used to build a SQL Query that is a part of a Union statement.
    /// </summary>
    /// <param name="union">Parent Union statement.</param>
    /// <param name="alias">Table alias name.</param>
    /// <typeparam name="T">Entity table to query.</typeparam>
    /// <returns><see cref="IQuery{T}"/> object to allow user to chain methods.</returns>
    public static IQuery<T> Query<T>(this IUnion union, string alias = "a") where T : class
    {
        return new QueryBuilderT<T>(CreateTablesManager<T>(alias), union.ParameterManager, union.ParameterPrefix, union.SqlStatement);
    }
    /// <summary>
    /// Returns an <see cref="IQuery{T, T2}"/> object used to build a SQL Query statement between two tables, and is a 
    /// part of a Union statement. Type <typeparamref name="T"/> represents the parent table entity.
    /// </summary>
    /// <typeparam name="T">Parent entity table.</typeparam>
    /// <typeparam name="T2">Join entity table.</typeparam>
    /// <param name="union">Parent Union statement.</param>
    /// <param name="tableAlias1">Table alias name of parent entity.</param>
    /// <param name="tableAlias2">Table alias name of join entity.</param>
    /// <returns><see cref="IQuery{T, T2}"/> object to allow user to chain methods.</returns>
    public static IQuery<T, T2> Query<T, T2>(this IUnion union, string tableAlias1 = "a", string tableAlias2 = "b") where T : class where T2 : class
    {
        return new QueryBuilderT2<T, T2>(CreateTablesManager<T>(tableAlias1).AddTable<T2>(tableAlias2), union.ParameterManager, union.ParameterPrefix, union.SqlStatement);
    }
    /// <summary>
    /// Returns an <see cref="IQuery{T, T2, T3}"/> object used to build a SQL Query statement between three tables, and
    /// is a part of a Union statement.  Type <typeparamref name="T"/> represents the parent table entity.
    /// </summary>
    /// <typeparam name="T">Parent entity table.</typeparam>
    /// <typeparam name="T2">Join entity table.</typeparam>
    /// <typeparam name="T3">Join entity table.</typeparam>
    /// <param name="union">Parent Union statement.</param>
    /// <param name="tableAlias1">Table alias name of parent entity.</param>
    /// <param name="tableAlias2">Table alias name of join entity.</param>
    /// <param name="tableAlias3">Table alias name of join entity.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain methods.</returns>
    public static IQuery<T, T2, T3> Query<T, T2, T3>(this IUnion union, string tableAlias1 = "a", string tableAlias2 = "b", string tableAlias3 = "c")
        where T : class
        where T2 : class
        where T3 : class
    {
        var table = CreateTablesManager<T>(tableAlias1)
            .AddTable<T2>(tableAlias2)
            .AddTable<T3>(tableAlias3);

        return new QueryBuilderT3<T, T2, T3>(table, union.ParameterManager, union.ParameterPrefix, union.SqlStatement);
    }
    /// <summary>
    /// Returns an <see cref="IQuery{T, T2, T3, T4}"/> object used to build a SQL Query statement between four tables,
    /// and is a part of a Union statement.   Type <typeparamref name="T"/> represents the parent table entity.
    /// </summary>
    /// <typeparam name="T">Parent entity table.</typeparam>
    /// <typeparam name="T2">Join entity table.</typeparam>
    /// <typeparam name="T3">Join entity table.</typeparam>
    /// <typeparam name="T4">Join entity table.</typeparam>
    /// <param name="union">Parent Union statement.</param>
    /// <param name="tableAlias1">Table alias name of parent entity.</param>
    /// <param name="tableAlias2">Table alias name of join entity.</param>
    /// <param name="tableAlias3">Table alias name of join entity.</param>
    /// <param name="tableAlias4">Table alias name of join entity.</param>
    /// <returns><see cref="IQuery{T, T2, T3, T4}"/> object to allow user to chain methods.</returns>
    public static IQuery<T, T2, T3, T4> Query<T, T2, T3, T4>(this IUnion union, string tableAlias1 = "a", string tableAlias2 = "b", string tableAlias3 = "c", string tableAlias4 = "d")
        where T : class
        where T2 : class
        where T3 : class
        where T4 : class
    {
        var table = CreateTablesManager<T>(tableAlias1)
            .AddTable<T2>(tableAlias2)
            .AddTable<T3>(tableAlias3)
            .AddTable<T4>(tableAlias4);

        return new QueryBuilderT4<T, T2, T3, T4>(table, union.ParameterManager, union.ParameterPrefix, union.SqlStatement);
    }
    /// <summary>
    /// Returns an <see cref="IQuery{T, T2, T3, T4, T5}"/> object used to build a SQL Query statement between five
    /// tables, and is a part of a Union statement.  Type <typeparamref name="T"/> represents the parent table entity.
    /// </summary>
    /// <typeparam name="T">Parent entity table.</typeparam>
    /// <typeparam name="T2">Join entity table.</typeparam>
    /// <typeparam name="T3">Join entity table.</typeparam>
    /// <typeparam name="T4">Join entity table.</typeparam>
    /// <typeparam name="T5">Join entity table.</typeparam>
    /// <param name="union">Parent Union statement.</param>
    /// <param name="tableAlias1">Table alias name of parent entity.</param>
    /// <param name="tableAlias2">Table alias name of join entity.</param>
    /// <param name="tableAlias3">Table alias name of join entity.</param>
    /// <param name="tableAlias4">Table alias name of join entity.</param>
    /// <param name="tableAlias5">Table alias name of join entity.</param>
    /// <returns><see cref="IQuery{T, T2, T3, T4, T5}"/> object to allow user to chain methods.</returns>
    public static IQuery<T, T2, T3, T4, T5> Query<T, T2, T3, T4, T5>(this IUnion union, string tableAlias1 = "a", string tableAlias2 = "b", string tableAlias3 = "c", string tableAlias4 = "d", string tableAlias5 = "e")
        where T : class
        where T2 : class
        where T3 : class
        where T4 : class
        where T5 : class
    {
        var table = CreateTablesManager<T>(tableAlias1)
            .AddTable<T2>(tableAlias2)
            .AddTable<T3>(tableAlias3)
            .AddTable<T4>(tableAlias4)
            .AddTable<T5>(tableAlias5);

        return new QueryBuilderT5<T, T2, T3, T4, T5>(table, union.ParameterManager, union.ParameterPrefix, union.SqlStatement);
    }
    
    #endregion
}