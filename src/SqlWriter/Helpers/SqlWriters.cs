using System.Linq.Expressions;
using SqlWriter.Builders.Delete;
using SqlWriter.Builders.Insert;
using SqlWriter.Builders.Query;
using SqlWriter.Builders.QueryCte;
using SqlWriter.Builders.RawSql;
using SqlWriter.Builders.Subquery;
using SqlWriter.Builders.TempTable;
using SqlWriter.Builders.Truncate;
using SqlWriter.Builders.Update;
using SqlWriter.Components.Parameters;
using SqlWriter.Components.Tables;

namespace SqlWriter;

public static class SqlWriters
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
    /// <param name="alias">Parent table alias.</param>
    /// <param name="dynamicWhere">If set to true, methods with string column argument will not apply the WHERE condition if the parameter value is null.</param> 
    /// <param name="parameterPrefix">Name to be used for parameters.  Each parameter will be incremented with a numeric suffix: p1, p2, etc.</param>
    /// <returns><see cref="IQueryBuilder"/> for building SQL Select statement.</returns>
    public static IQueryBuilder QueryBuilder<T>(string alias, bool dynamicWhere = false, string parameterPrefix = "p") where T : class
    {
        return new QueryBuilder(CreateTablesManager<T>(alias), new ParameterManager(), dynamicWhere, parameterPrefix);
    }
    /// <summary>
    /// Returns an <see cref="IQuery{T}"/> object used to build a SQL Query statement.
    /// </summary>
    /// <typeparam name="T">Entity table to query.</typeparam>
    /// <param name="alias">Table alias name.</param>
    /// <param name="parameterPrefix">Name to be used for parameters.  Each parameter will be incremented with a numeric suffix: p1, p2, etc.</param>
    /// <returns><see cref="IQuery{T}"/> object to allow user to chain methods.</returns>
    public static IQuery<T> Query<T>(string alias = "a", string parameterPrefix = "p") where T : class
    {
        return new QueryBuilderT<T>(CreateTablesManager<T>(alias), new ParameterManager(), parameterPrefix);
    }
    /// <summary>
    /// Returns an <see cref="IQuery{T, T2}"/> object used to build a SQL Query statement between two tables.  
    /// Type <typeparamref name="T"/> represents the parent table entity.
    /// </summary>
    /// <typeparam name="T">Parent entity table.</typeparam>
    /// <typeparam name="T2">Join entity table.</typeparam>
    /// <param name="tableAlias1">Table alias name of parent entity.</param>
    /// <param name="tableAlias2">Table alias name of join entity.</param>
    /// <param name="parameterPrefix">Name to be used for parameters.  Each parameter will be incremented with a numeric suffix: p1, p2, etc.</param>
    /// <returns><see cref="IQuery{T, T2}"/> object to allow user to chain methods.</returns>
    public static IQuery<T, T2> Query<T, T2>(string tableAlias1 = "a", string tableAlias2 = "b", string parameterPrefix = "p") where T : class where T2 : class
    {
        return new QueryBuilderT2<T, T2>(CreateTablesManager<T>(tableAlias1).AddTable<T2>(tableAlias2), new ParameterManager(), parameterPrefix);
    }
    /// <summary>
    /// Returns an <see cref="IQuery{T, T2, T3}"/> object used to build a SQL Query statement between three tables.  
    /// Type <typeparamref name="T"/> represents the parent table entity.
    /// </summary>
    /// <typeparam name="T">Parent entity table.</typeparam>
    /// <typeparam name="T2">Join entity table.</typeparam>
    /// <typeparam name="T3">Join entity table.</typeparam>
    /// <param name="tableAlias1">Table alias name of parent entity.</param>
    /// <param name="tableAlias2">Table alias name of join entity.</param>
    /// <param name="tableAlias3">Table alias name of join entity.</param>
    /// <param name="parameterPrefix">Name to be used for parameters.  Each parameter will be incremented with a numeric suffix: p1, p2, etc.</param>
    /// <returns><see cref="IQuery{T, T2, T3}"/> object to allow user to chain methods.</returns>
    public static IQuery<T, T2, T3> Query<T, T2, T3>(string tableAlias1 = "a", string tableAlias2 = "b", string tableAlias3 = "c", string parameterPrefix = "p")
        where T : class
        where T2 : class
        where T3 : class
    {
        var table = CreateTablesManager<T>(tableAlias1)
            .AddTable<T2>(tableAlias2)
            .AddTable<T3>(tableAlias3);

        return new QueryBuilderT3<T, T2, T3>(table, new ParameterManager(), parameterPrefix);
    }
    /// <summary>
    /// Returns an <see cref="IQuery{T, T2, T3, T4}"/> object used to build a SQL Query statement between four tables.  
    /// Type <typeparamref name="T"/> represents the parent table entity.
    /// </summary>
    /// <typeparam name="T">Parent entity table.</typeparam>
    /// <typeparam name="T2">Join entity table.</typeparam>
    /// <typeparam name="T3">Join entity table.</typeparam>
    /// <typeparam name="T4">Join entity table.</typeparam>
    /// <param name="tableAlias1">Table alias name of parent entity.</param>
    /// <param name="tableAlias2">Table alias name of join entity.</param>
    /// <param name="tableAlias3">Table alias name of join entity.</param>
    /// <param name="tableAlias4">Table alias name of join entity.</param>
    /// <param name="parameterPrefix">Name to be used for parameters.  Each parameter will be incremented with a numeric suffix: p1, p2, etc.</param>
    /// <returns><see cref="IQuery{T, T2, T3, T4}"/> object to allow user to chain methods.</returns>
    public static IQuery<T, T2, T3, T4> Query<T, T2, T3, T4>(string tableAlias1 = "a", string tableAlias2 = "b", string tableAlias3 = "c", string tableAlias4 = "d", string parameterPrefix = "p")
        where T : class
        where T2 : class
        where T3 : class
        where T4 : class
    {
        var table = CreateTablesManager<T>(tableAlias1)
            .AddTable<T2>(tableAlias2)
            .AddTable<T3>(tableAlias3)
            .AddTable<T4>(tableAlias4);

        return new QueryBuilderT4<T, T2, T3, T4>(table, new ParameterManager(), parameterPrefix);
    }
    /// <summary>
    /// Returns an <see cref="IQuery{T, T2, T3, T4, T5}"/> object used to build a SQL Query statement between five tables.  
    /// Type <typeparamref name="T"/> represents the parent table entity.
    /// </summary>
    /// <typeparam name="T">Parent entity table.</typeparam>
    /// <typeparam name="T2">Join entity table.</typeparam>
    /// <typeparam name="T3">Join entity table.</typeparam>
    /// <typeparam name="T4">Join entity table.</typeparam>
    /// <typeparam name="T5">Join entity table.</typeparam>
    /// <param name="tableAlias1">Table alias name of parent entity.</param>
    /// <param name="tableAlias2">Table alias name of join entity.</param>
    /// <param name="tableAlias3">Table alias name of join entity.</param>
    /// <param name="tableAlias4">Table alias name of join entity.</param>
    /// <param name="tableAlias5">Table alias name of join entity.</param>
    /// <param name="parameterPrefix">Name to be used for parameters.  Each parameter will be incremented with a numeric suffix: p1, p2, etc.</param>
    /// <returns><see cref="IQuery{T, T2, T3, T4, T5}"/> object to allow user to chain methods.</returns>
    public static IQuery<T, T2, T3, T4, T5> Query<T, T2, T3, T4, T5>(string tableAlias1 = "a", string tableAlias2 = "b", string tableAlias3 = "c", string tableAlias4 = "d", string tableAlias5 = "e", string parameterPrefix = "p")
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

        return new QueryBuilderT5<T, T2, T3, T4, T5>(table, new ParameterManager(), parameterPrefix);
    }

    #endregion Query

    #region CTE
    /// <summary>
    /// Returns an <see cref="ICte{T}"/> object that can be used as a common table expression (CTE).  Parent entity <typeparamref name="T"/> 
    /// will use default table alias name of 'a'.  All columns (excluding the target Join column) in the  CTE query will be 
    /// projected into the parent query by default.  To stop or modify this behavior, set the
    /// <paramref name="stopColumnsProjection"/> to true to stop all projection.  To include the target Join column, set the
    /// <paramref name="includeCteJoinColumn"/> to true.
    /// </summary>
    /// <typeparam name="T">Parent entity table.</typeparam>
    /// <param name="cteAlias">CTE alias name.  Must be unique if more than one CTE will be used.</param>
    /// <param name="stopColumnsProjection">If true, none of the CTE columns will be included in the parent query's SELECT statement.</param>
    /// <param name="includeCteJoinColumn">If true, CTE join column will be included in parent query projection results.  Default is set to exclude column.</param>
    /// <returns><see cref="ICte{T}"/> object to allow user to chain methods.</returns>
    public static ICte<T> QueryAsCte<T>(string cteAlias = "cteA", bool stopColumnsProjection = false, bool includeCteJoinColumn = false) where T : class
    {
        return new CteBuilderT<T>(new TablesManager(typeof(T), "a"),  cteAlias, stopColumnsProjection, includeCteJoinColumn, cteAlias);
    }
    /// <summary>
    /// Returns an <see cref="ICte{T, T2}"/> object that can be used as a common table expression (CTE).  Parent entity <typeparamref name="T"/> 
    /// will use default table alias name 'a', and entity <typeparamref name="T2"/> will use alias name 'b'.  All columns 
    /// (excluding the target Join column) in the  CTE query will be projected into the parent query by default.  To stop or modify 
    /// this behavior, set the <paramref name="stopColumnsProjection"/> to true to stop all projection.  To include the target Join column, 
    /// set the <paramref name="includeCteJoinColumn"/> to true.
    /// </summary>
    /// <typeparam name="T">Parent entity table.</typeparam>
    /// <typeparam name="T2">Join entity table.</typeparam>
    /// <param name="cteAlias">CTE alias name.  Must be unique if more than one CTE will be used.</param>
    /// <param name="stopColumnsProjection">If true, none of the CTE columns will be included in the parent query's SELECT statement.</param>
    /// <param name="includeCteJoinColumn">If true, CTE join column will be included in parent query projection results.  Default is set to exclude column.</param>
    /// <returns><see cref="ICte{T, T2}"/> object to allow user to chain methods.</returns>
    public static ICte<T, T2> QueryAsCte<T, T2>(string cteAlias = "cteA", bool stopColumnsProjection = false, bool includeCteJoinColumn = false) where T : class where T2 : class
    {
        var table = CreateTablesManager<T>("a").AddTable<T2>("b");

        return new CteBuilderT2<T, T2>(table, cteAlias, stopColumnsProjection, includeCteJoinColumn, cteAlias);
    }

    #endregion CTE

    #region Subquery
    /// <summary>
    /// Returns object that represents a subquery statement.
    /// </summary>
    /// <typeparam name="TSub">Entity class model.</typeparam>
    /// <param name="parameterPrefix">Name to use for parameters</param>
    /// <param name="predicate">Condition predicate.</param>
    /// <param name="prefix">Condition And/Or prefix.</param>
    /// <returns><see cref="ISubquery{TSub}"/> object to allow user to chain methods.</returns>
    public static ISubquery<TSub> Subquery<TSub>(string parameterPrefix = "sub1", Predicates predicate = Predicates.Equal, Prefix prefix = Prefix.AND) where TSub : class
    {
        return new SubqueryBuilder<TSub>(parameterPrefix, predicate, prefix);
    }

    #endregion Subquery

    #region Insert
    /// <summary>
    /// Returns an <see cref="IInsert{T}"/> object used to build a SQL Insert statement.
    /// </summary>
    /// <typeparam name="T">Parent entity table.</typeparam>
    /// <returns><see cref="IInsert{T}"/> object to allow user to chain methods.</returns>
    public static IInsert<T> Insert<T>() where T : class
    {
        return new InsertBuilder<T>(new TablesManager(typeof(T), "a"), new ParameterManager());
    }
    /// <summary>
    /// Returns an <see cref="IInsert{TInsert, TSelect}"/> object used to build a SQL Insert statement, using data
    /// from the <typeparamref name="TSelect"/> Select statement.
    /// </summary>
    /// <remarks>
    /// If INTO method is omitted, compiler will use <typeparamref name="TInsert"/> entity to project insert column names and ordinal position.
    /// </remarks>
    /// <typeparam name="TInsert">Insert entity.</typeparam>
    /// <typeparam name="TSelect">Select entity.</typeparam>
    /// <returns><see cref="IInsert{TInsert, TSelect}"/> object to allow user to chain methods.</returns>
    public static IInsert<TInsert, TSelect> Insert<TInsert, TSelect>() where TInsert : class where TSelect : class
    {
        return new InsertBuilderT<TInsert, TSelect>(new TablesManager(typeof(TSelect), "a"), new ParameterManager());
    }
    /// <summary>
    /// Returns an <see cref="IInsert{TInsert, T, T2}"/> object used to build a SQL Insert statement, using data
    /// from the <typeparamref name="T"/> and <typeparamref name="T2"/> Select statement.
    /// </summary>
    /// <remarks>
    /// If INTO method is omitted, compiler will use <typeparamref name="TInsert"/> entity to project insert column names and ordinal position.
    /// </remarks>
    /// <typeparam name="TInsert">Insert entity.</typeparam>
    /// <typeparam name="T">Select parent entity.  Uses default table alias name of 'a'.</typeparam>
    /// <typeparam name="T2">Select join entity.  Uses default table alias name of 'b'.</typeparam>
    /// <returns><see cref="IInsert{TInsert, T, T2}"/> object to allow user to chain methods.</returns>
    public static IInsert<TInsert, T, T2> Insert<TInsert, T, T2>() where TInsert : class where T : class where T2 : class
    {
        var tables = CreateTablesManager<T>("a").AddTable<T2>("b");

        return new InsertBuilderT2<TInsert, T, T2>(tables, new ParameterManager());
    }
    /// <summary>
    /// Returns an <see cref="IInsert{TInsert, T, T2, T3}"/> object used to build a SQL Insert statement, using data
    /// from the <typeparamref name="T"/>, <typeparamref name="T2"/>, and <typeparamref name="T3"/> Select statement.
    /// </summary>
    /// <remarks>
    /// If INTO method is omitted, compiler will use <typeparamref name="TInsert"/> entity to project insert column names and ordinal position.
    /// </remarks>
    /// <typeparam name="TInsert">Insert entity.</typeparam>
    /// <typeparam name="T">Select parent entity.  Uses default table alias name of 'a'.</typeparam>
    /// <typeparam name="T2">Select join entity.  Uses default table alias name of 'b'.</typeparam>
    /// <typeparam name="T3">Select join entity.  Uses default table alias name of 'c'.</typeparam>
    /// <returns><see cref="IInsert{TInsert, T, T2, T3}"/> object to allow user to chain methods.</returns>
    public static IInsert<TInsert, T, T2, T3> Insert<TInsert, T, T2, T3>() where TInsert : class where T : class where T2 : class  where T3 : class
    {
        var tables = CreateTablesManager<T>("a").AddTable<T2>("b").AddTable<T3>("c");

        return new InsertBuilderT3<TInsert, T, T2, T3>(tables, new ParameterManager());
    }
    /// <summary>
    /// Returns an <see cref="IInsert{TInsert, T, T2, T3, T4}"/> object used to build a SQL Insert statement, using data
    /// from the <typeparamref name="T"/>, <typeparamref name="T2"/>, <typeparamref name="T3"/>, 
    /// and <typeparamref name="T4"/> Select statement.
    /// </summary>
    /// <remarks>
    /// If INTO method is omitted, compiler will use <typeparamref name="TInsert"/> entity to project insert column names and ordinal position.
    /// </remarks>
    /// <typeparam name="TInsert">Insert entity.</typeparam>
    /// <typeparam name="T">Select parent entity.  Uses default table alias name of 'a'.</typeparam>
    /// <typeparam name="T2">Select join entity.  Uses default table alias name of 'b'.</typeparam>
    /// <typeparam name="T3">Select join entity.  Uses default table alias name of 'c'.</typeparam>
    /// <typeparam name="T4">Select join entity.  Uses default table alias name of 'd'.</typeparam>
    /// <returns><see cref="IInsert{TInsert, T, T2, T3, T4}"/> object to allow user to chain methods.</returns>
    public static IInsert<TInsert, T, T2, T3, T4> Insert<TInsert, T, T2, T3, T4>()
        where TInsert : class
        where T : class
        where T2 : class
        where T3 : class
        where T4 : class
    {
        var tables = CreateTablesManager<T>("a").AddTable<T2>("b").AddTable<T3>("c").AddTable<T4>("d");

        return new InsertBuilderT4<TInsert, T, T2, T3, T4>(tables, new ParameterManager());
    }
    /// <summary>
    /// Returns an <see cref="IInsertMany{T}"/> object used to build a SQL Insert statement of multiple values.  
    /// </summary>
    /// <remarks>
    /// Object will not parameterize any values.  As such, the <see cref="IInsertMany{T}"/> object should not be 
    /// used with user/client supplied data.
    /// </remarks>
    /// <typeparam name="T">Insert entity.</typeparam>
    /// <returns><see cref="IInsertMany{T}"/> object to allow user to chain methods.</returns>
    public static IInsertMany<T> InsertMany<T>() where T : class
    {
        return new InsertManyBuilder<T>(new TableModel(typeof(T), "a"));
    }
    /// <summary>
    /// Returns an <see cref="IInsertMany{T}"/> object used to build a SQL Insert statement of multiple values.  Use parameter 
    /// <paramref name="insertColumns"/> to target specific columns.
    /// </summary>
    /// <remarks>
    /// Object will not parameterize any values.  As such, the <see cref="IInsertMany{T}"/> object should not be 
    /// used with user/client supplied data.
    /// </remarks>
    /// <typeparam name="T">Insert entity.</typeparam>
    /// <param name="insertColumns">Insert column targets.</param>
    /// <returns><see cref="IInsertMany{T}"/> object to allow user to chain methods.</returns>
    public static IInsertMany<T> InsertMany<T>(Expression<Func<T, object>> insertColumns) where T : class
    {
        return new InsertManyBuilder<T>(new TableModel(typeof(T), "a"), insertColumns);
    }

    #endregion Insert
    
    #region Update
    /// <summary>
    /// Returns an <see cref="IUpdate{T}"/> object used to build a SQL Update statement. 
    /// </summary>
    /// <typeparam name="T">Update entity.</typeparam>
    /// <returns><see cref="IUpdate{T}"/> object to allow user to chain methods.</returns>
    public static IUpdate<T> Update<T>() where T : class
    {
        return new UpdateBuilder<T>(new ParameterManager());
    }
    /// <summary>
    /// Returns an <see cref="IUpdate{TUpdate, TSelect}"/> object used to build a SQL Update statement, using data
    /// from the <typeparamref name="TSelect"/> Select statement.  Will join tables <typeparamref name="TUpdate"/> and 
    /// <typeparamref name="TSelect"/> using Primary Key field identified in <typeparamref name="TUpdate"/>'s 
    /// <see cref="TableNameAttribute"/> attribute.  By convention, method will assume Primary Key name in <typeparamref name="TSelect"/> 
    /// has an associated field name in <typeparamref name="TSelect"/>.
    /// </summary>
    /// <typeparam name="TUpdate">Update entity.</typeparam>
    /// <typeparam name="TSelect">Select parent entity.</typeparam>
    /// <param name="joinType">Join type.</param>
    /// <returns><see cref="IUpdate{TUpdate, TSelect}"/> object to allow user to chain methods.</returns>
    public static IUpdate<TUpdate, TSelect> Update<TUpdate, TSelect>(JoinType joinType = JoinType.Inner) where TUpdate : class where TSelect : class
    {
        var tables = CreateTablesManager<TUpdate>("a").AddTable<TSelect>("b");
        tables.AddJoin<TUpdate, TSelect>(joinType);

        return new UpdateBuilderT<TUpdate, TSelect>(tables, new ParameterManager());
    }
    /// <summary>
    /// Returns an <see cref="IUpdate{TUpdate, TSelect}"/> object used to build a SQL Update statement, using data
    /// from the <typeparamref name="TSelect"/> Select statement. 
    /// </summary>
    /// <typeparam name="TUpdate">Update entity.</typeparam>
    /// <typeparam name="TSelect">Select parent entity.</typeparam>
    /// <param name="joinExpression">Join expression for Select statement.</param>
    /// <param name="joinType">Join type.</param>
    /// <returns><see cref="IUpdate{TUpdate, TSelect}"/> object to allow user to chain methods.</returns>
    public static IUpdate<TUpdate, TSelect> Update<TUpdate, TSelect>(Expression<Func<TUpdate, TSelect, bool>> joinExpression, JoinType joinType = JoinType.Inner)
        where TUpdate : class where TSelect : class
    {
        var tables = CreateTablesManager<TUpdate>("a").AddTable<TSelect>("b");
        tables.AddJoin(joinType, joinExpression);

        return new UpdateBuilderT<TUpdate, TSelect>(tables, new ParameterManager());
    }

    #endregion Update
    
    #region Delete
    /// <summary>
    /// Returns an <see cref="IDelete{T}"/> object used to build a SQL Delete statement.
    /// </summary>
    /// <typeparam name="T">Delete entity.</typeparam>
    /// <returns><see cref="IDelete{T}"/> object to allow user to chain methods.</returns>
    public static IDelete<T> Delete<T>() where T : class
    {
        return new DeleteBuilder<T>();
    }

    #endregion Delete

    #region Temp tables
    /// <summary>
    /// Creates a Table variable to help store data temporarily, similar to the temp table in SQL Server.
    /// Will use name in <see cref="TableVariableAttribute"/> as Table name, and properties in <typeparamref name="T"/>
    /// for column and database types.
    /// </summary>
    /// <typeparam name="T">Table variable entity.</typeparam>
    /// <returns><see cref="ITableVariable"/> object to allow user to chain methods.</returns>
    public static ITableVariable TableVariable<T>() where T : class
    {
        return new TableVariableBuilder(typeof(T), new ParameterManager());
    }
    /// <summary>
    /// Creates a Table variables to help store data temporarily, similar to the temp table in SQL Server.
    /// Will use name in <see cref="TableVariableAttribute"/> as Table name, and properties in associated entity
    /// for column and database types.
    /// </summary>
    /// <typeparam name="T">Table variable entity 1.</typeparam>
    /// <typeparam name="T2">Table variable entity 2.</typeparam>
    /// <returns><see cref="ITableVariable"/> object to allow user to chain methods.</returns>
    public static ITableVariable TableVariables<T, T2>() where T : class where T2 : class
    {
        return new TableVariableBuilderT2<T, T2>(new ParameterManager());
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
    /// <returns><see cref="ITruncateTable"/> object to allow user to chain methods.</returns>
    public static ITruncateTable TruncateTable<TEntity>() where TEntity : class
    {
        return new TruncateBuilder(typeof(TEntity), new ParameterManager());
    }

    #endregion Truncate

    #region RawSql
    /// <summary>
    /// Container for a raw SQL statement and associated parameters.
    /// </summary>
    /// <param name="sqlStatement">Fully qualified SQL statement.  User is responsible for
    /// ensuring statement is fully-qualified and valid.</param>
    /// <returns><see cref="IRawSql"/> object to allow user to chain methods.</returns>
    public static IRawSql RawSql(string sqlStatement)
    {
        return new RawSqlBuilder(new ParameterManager(), sqlStatement);
    }
    
    #endregion
}