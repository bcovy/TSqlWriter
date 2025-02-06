using System.Linq.Expressions;
using SqlWriter.Builders.Insert;
using SqlWriter.Builders.RawSql;
using SqlWriter.Builders.TempTable;
using SqlWriter.Builders.Truncate;
using SqlWriter.Builders.Update;
using SqlWriter.Components.Tables;

namespace SqlWriter;

public static class ConcatStatement
{
    private static TablesManager CreateTablesManager<T>(string aliasName) where T : class
    {
        return new TablesManager(typeof(T), aliasName);
    }
    #region Insert
    /// <summary>
    /// Returns an <see cref="IInsert{T}"/> object used to build a SQL Insert statement.
    /// </summary>
    /// <typeparam name="T">Parent entity table.</typeparam>
    /// <param name="concatStatement">Statement to concat.</param>
    /// <returns><see cref="IInsert{T}"/> object to allow user to chain methods.</returns>
    public static IInsert<T> Insert<T>(this IConcatSql concatStatement) where T : class
    { 
        return new InsertBuilder<T>(new TablesManager(typeof(T), "a"), concatStatement.ParameterManager, concatStatement.ParameterPrefix, concatStatement.SqlStatement);
    }
    /// <summary>
    /// Returns an <see cref="IInsert{TInsert, TSelect}"/> object used to build a SQL Insert statement, using data from the <typeparamref name="TSelect"/> Select statement.
    /// Result will compile all methods in the chain to form a single SQL statement.  If INTO method is omitted, compiler will use <typeparamref name="TInsert"/> entity to 
    /// project insert column names and ordinal position.
    /// </summary>
    /// <remarks>
    /// Fluent chaining of methods must be used in order for statement concatenation to be applied.  Chaining is necessary as the last statement builder in the 
    /// chain is responsible for compiling all methods into a single statement.
    /// </remarks>
    /// <typeparam name="TInsert">Insert entity.</typeparam>
    /// <typeparam name="TSelect">Select entity.</typeparam>
    /// <param name="concatStatement">Statement to concat.</param>
    /// <returns><see cref="IInsert{TInsert, TSelect}"/> object to allow user to chain methods.</returns>
    public static IInsert<TInsert, TSelect> Insert<TInsert, TSelect>(this IConcatSql concatStatement)
        where TInsert : class where TSelect : class
    {
        return new InsertBuilderT<TInsert, TSelect>(new TablesManager(typeof(TSelect), "a"),
            concatStatement.ParameterManager, concatStatement.ParameterPrefix, concatStatement.SqlStatement);
    }
    /// <summary>
    /// Returns an <see cref="IInsert{TInsert, T, T2}"/> object used to build a SQL Insert statement, using data
    /// from the <typeparamref name="T"/> and <typeparamref name="T2"/> Select statement.  Result will compile all methods in the chain to form a single SQL statement.
    /// If INTO method is omitted, compiler will use <typeparamref name="TInsert"/> entity to project insert column names and ordinal position.
    /// </summary>
    /// <remarks>
    /// It is recommended that building of the next statement be done using the fluent build methods on this object.  This helps to 
    /// ensure the last statement in the chain is able to compile all preceding statements into one. 
    /// </remarks>
    /// <typeparam name="TInsert">Insert entity.</typeparam>
    /// <typeparam name="T">Select parent entity.  Uses default table alias name of 'a'.</typeparam>
    /// <typeparam name="T2">Select join entity.  Uses default table alias name of 'b'.</typeparam>
    /// <param name="concatStatement">Statement to concat.</param>
    /// <returns><see cref="IInsert{TInsert, T, T2}"/> object to allow user to chain methods.</returns>
    public static IInsert<TInsert, T, T2> Insert<TInsert, T, T2>(this IConcatSql concatStatement)
        where TInsert : class where T : class where T2 : class
    {
        var tables = CreateTablesManager<T>("a").AddTable<T2>("b");

        return new InsertBuilderT2<TInsert, T, T2>(tables, concatStatement.ParameterManager,
            concatStatement.ParameterPrefix, concatStatement.SqlStatement);
    }

    /// <summary>
    /// Returns an <see cref="IInsert{TInsert, T, T2, T3}"/> object used to build a SQL Insert statement, using data
    /// from the <typeparamref name="T"/>, <typeparamref name="T2"/>, and <typeparamref name="T3"/> Select statement.
    /// Result will compile all methods in the chain to form a single SQL statement.
    /// If INTO method is omitted, compiler will use <typeparamref name="TInsert"/> entity to project insert column names and ordinal position.
    /// </summary>
    /// <remarks>
    /// It is recommended that building of the next statement be done using the fluent build methods on this object.  This helps to 
    /// ensure the last statement in the chain is able to compile all preceding statements into one. 
    /// </remarks>
    /// <typeparam name="TInsert">Insert entity.</typeparam>
    /// <typeparam name="T">Select parent entity.  Uses default table alias name of 'a'.</typeparam>
    /// <typeparam name="T2">Select join entity.  Uses default table alias name of 'b'.</typeparam>
    /// <typeparam name="T3">Select join entity.  Uses default table alias name of 'c'.</typeparam>
    /// <param name="concatStatement">Statement to concat.</param>
    /// <returns><see cref="IInsert{TInsert, T, T2, T3}"/> object to allow user to chain methods.</returns>
    public static IInsert<TInsert, T, T2, T3> Insert<TInsert, T, T2, T3>(this IConcatSql concatStatement)
        where TInsert : class where T : class where T2 : class where T3 : class
    {
        var tables = CreateTablesManager<T>("a").AddTable<T2>("b").AddTable<T3>("c");

        return new InsertBuilderT3<TInsert, T, T2, T3>(tables, concatStatement.ParameterManager,
            concatStatement.ParameterPrefix, concatStatement.SqlStatement);
    }
    /// <summary>
    /// Returns an <see cref="IInsert{TInsert, T, T2, T3, T4}"/> object used to build a SQL Insert statement, using data
    /// from the <typeparamref name="T"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, 
    /// and <typeparamref name="T4"/> Select statement.  Result will compile all methods in the chain to form a single SQL statement.
    /// If INTO method is omitted, compiler will use <typeparamref name="TInsert"/> entity to project insert column names and ordinal position.
    /// </summary>
    /// <remarks>
    /// It is recommended that building of the next statement be done using the fluent build methods on this object.  This helps to 
    /// ensure the last statement in the chain is able to compile all preceding statements into one. 
    /// </remarks>
    /// <typeparam name="TInsert">Insert entity.</typeparam>
    /// <typeparam name="T">Select parent entity.  Uses default table alias name of 'a'.</typeparam>
    /// <typeparam name="T2">Select join entity.  Uses default table alias name of 'b'.</typeparam>
    /// <typeparam name="T3">Select join entity.  Uses default table alias name of 'c'.</typeparam>
    /// <typeparam name="T4">Select join entity.  Uses default table alias name of 'd'.</typeparam>
    /// <param name="concatStatement"></param>
    /// <returns><see cref="IInsert{TInsert, T, T2, T3, T4}"/> object to allow user to chain methods.</returns>
    public static IInsert<TInsert, T, T2, T3, T4> Insert<TInsert, T, T2, T3, T4>(this IConcatSql concatStatement)
        where TInsert : class
        where T : class
        where T2 : class
        where T3 : class
        where T4 : class
    {
        var tables = CreateTablesManager<T>("a").AddTable<T2>("b").AddTable<T3>("c").AddTable<T4>("d");

        return new InsertBuilderT4<TInsert, T, T2, T3, T4>(tables, concatStatement.ParameterManager,
            concatStatement.ParameterPrefix, concatStatement.SqlStatement);
    }

    #endregion Insert
 
    #region Update
    /// <summary>
    /// Returns an <see cref="IUpdate{T}"/> object used to build a SQL Update statement.  Result will compile all methods in the chain to form a single SQL statement.
    /// </summary>
    /// <remarks>
    /// It is recommended that building of the next statement be done using the fluent build methods on this object.  This helps to 
    /// ensure the last statement in the chain is able to compile all preceding statements into one. 
    /// </remarks>
    /// <typeparam name="T">Update entity.</typeparam>
    /// <param name="concatStatement">Statement to concatenate.</param>
    /// <returns><see cref="IUpdate{TUpdate, TSelect}"/> object to allow user to chain methods.</returns>
    public static IUpdate<T> Update<T>(this IConcatSql concatStatement) where T : class
    {
        return new UpdateBuilder<T>(concatStatement.ParameterManager, "p", concatStatement.SqlStatement);
    }
    /// <summary>
    /// Returns an <see cref="IUpdate{TUpdate, TSelect}"/> object used to build a SQL Update statement, using data
    /// from the <typeparamref name="TSelect"/> Select statement.  Will join tables <typeparamref name="TUpdate"/> and 
    /// <typeparamref name="TSelect"/> using Primary Key field identified in <typeparamref name="TUpdate"/>'s 
    /// <see cref="TableNameAttribute"/> attribute.  By convention, method will assume Primary Key name in <typeparamref name="TSelect"/>.
    /// Result will compile statements from the previous chained methods to create one concatenated statement.
    /// </summary>
    /// <remarks>
    /// It is recommended that building of the next statement be done using the fluent build methods on this object.  This helps to 
    /// ensure the last statement in the chain is able to compile all preceding statements into one. 
    /// </remarks>
    /// <typeparam name="TUpdate">Update entity.</typeparam>
    /// <typeparam name="TSelect">Select parent entity.</typeparam>
    /// <param name="concatStatement">Statement to concatenate.</param>
    /// <param name="joinType">Join type.</param>
    /// <returns><see cref="IUpdate{TUpdate, TSelect}"/> object to allow user to chain methods.</returns>
    public static IUpdate<TUpdate, TSelect> Update<TUpdate, TSelect>(this IConcatSql concatStatement, JoinType joinType = JoinType.Inner) where TUpdate : class where TSelect : class
    {
        var tables = CreateTablesManager<TUpdate>("a").AddTable<TSelect>("b");
        tables.AddJoin<TUpdate, TSelect>(joinType);

        return new UpdateBuilderT<TUpdate, TSelect>(tables, concatStatement.ParameterManager, concatStatement.ParameterPrefix, concatStatement.SqlStatement);
    }
    /// <summary>
    /// Returns an <see cref="IUpdate{TUpdate, TSelect}"/> object used to build a SQL Update statement, using data
    /// from the <typeparamref name="TSelect"/> Select statement. Result will compile statements from the previous chained methods to create one concatenated statement.
    /// </summary>
    /// <remarks>
    /// It is recommended that building of the next statement be done using the fluent build methods on this object.  This helps to 
    /// ensure the last statement in the chain is able to compile all preceding statements into one. 
    /// </remarks>
    /// <typeparam name="TUpdate">Update entity.</typeparam>
    /// <typeparam name="TSelect">Select parent entity.</typeparam>
    /// <param name="concatStatement">Statement to concatenate.</param>
    /// <param name="joinExpression">Join expression for Select statement.</param>
    /// <param name="joinType">Join type.</param>
    /// <returns><see cref="IUpdate{TUpdate, TSelect}"/> object to allow user to chain methods.</returns>
    public static IUpdate<TUpdate, TSelect> Update<TUpdate, TSelect>(this IConcatSql concatStatement, Expression<Func<TUpdate, TSelect, bool>> joinExpression, JoinType joinType = JoinType.Inner)
        where TUpdate : class where TSelect : class
    {
        var tables = CreateTablesManager<TUpdate>("a").AddTable<TSelect>("b");
        tables.AddJoin(joinType, joinExpression);

        return new UpdateBuilderT<TUpdate, TSelect>(tables, concatStatement.ParameterManager, concatStatement.ParameterPrefix, concatStatement.SqlStatement);
    }

    #endregion Update
    
    #region Temp tables
    /// <summary>
    /// Creates a Table variable to help store data temporarily, similar to the temp table in SQL Server.
    /// Will use name in <see cref="TableVariableAttribute"/> as Table name, and properties in <typeparamref name="T"/>
    /// for column and database types.
    /// </summary>
    /// <typeparam name="T">Table variable entity.</typeparam>
    /// <param name="concatStatement">Statement to concatenate.</param>
    /// <returns><see cref="ITableVariable"/> object to allow user to chain methods.</returns>
    public static ITableVariable TableVariable<T>(this IConcatSql concatStatement) where T : class
    {
        return new TableVariableBuilder(typeof(T), concatStatement.ParameterManager, concatStatement.ParameterPrefix, concatStatement.SqlStatement);
    }
    /// <summary>
    /// Creates a Table variable to help store data temporarily, similar to the temp table in SQL Server.
    /// Will use name in <see cref="TableVariableAttribute"/> as Table name, and properties in associated entity
    /// for column and database types.
    /// </summary>
    /// <typeparam name="T">Table variable entity 1.</typeparam>
    /// <typeparam name="T2">Table variable entity 2.</typeparam>
    /// <param name="concatStatement"></param>
    /// <returns><see cref="ITableVariable"/> object to allow user to chain methods.</returns>
    public static ITableVariable TableVariables<T, T2>(this IConcatSql concatStatement) where T : class where T2 : class
    {
        return new TableVariableBuilderT2<T, T2>(concatStatement.ParameterManager, concatStatement.ParameterPrefix, concatStatement.SqlStatement);
    }
    #endregion Temp tables

    #region Truncate
    /// <summary>
    /// Creates a Truncate Table statement.
    /// </summary>
    /// <remarks>
    /// Requires ALTER permission on SQL server.
    /// </remarks>
    /// <typeparam name="TEntity">Table entity.</typeparam>
    /// <param name="concatStatement">Statement to concatenate.</param>
    /// <returns><see cref="ITruncateTable"/> object to allow user to chain methods.</returns>
    public static ITruncateTable TruncateTable<TEntity>(this IConcatSql concatStatement) where TEntity : class
    {
        return new TruncateBuilder(typeof(TEntity), concatStatement.ParameterManager, concatStatement.ParameterPrefix, concatStatement.SqlStatement);
    }

    #endregion Truncate
    
    #region RawSql
    /// <summary>
    /// Container for a raw SQL statement and associated parameters.
    /// </summary>
    /// <param name="concatStatement">Statement to concatenate.</param>
    /// <param name="sqlStatement">Fully qualified SQL statement.  User is responsible for ensuring statement is
    /// fully-qualified and valid.</param>
    /// <returns><see cref="IRawSql"/> object to allow user to chain methods.</returns>
    public static IRawSql RawSql(this IConcatSql concatStatement, string sqlStatement)
    {
        return new RawSqlBuilder(concatStatement.ParameterManager, sqlStatement, concatStatement.ParameterPrefix, concatStatement.SqlStatement);
    }
    
    #endregion
}