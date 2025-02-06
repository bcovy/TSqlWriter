using System.Linq.Expressions;
using System.Text;
using SqlWriter.Components.Joins;
using SqlWriter.Infrastructure;
using SqlWriter.Interfaces;
using SqlWriter.Interfaces.Internals;

namespace SqlWriter.Components.Tables;

public class TablesManager : ITablesManager
{
    private char _tableAlias = 'a';  // initial table alias.
    private readonly string _parentTableName;
    private readonly string _parentTableAlias;
    private readonly HashSet<string> _usedAliases = [];

    public Dictionary<Type, TableModel> Tables { get; }
    public List<JoinModel> JoinInfo { get; }

    public TablesManager(Type parentTable, string? alias = null)
    {
        alias ??= GenerateAlias();
        var model = new TableModel(parentTable, alias);

        _usedAliases.Add(alias);
        _parentTableAlias = alias;
        _parentTableName = model.TableName;

        JoinInfo = [];
        Tables = new Dictionary<Type, TableModel>
        {
            { parentTable, model }
        };
    }

    #region Add Table
    public ITablesManager AddTable<TTable>(string alias = "") where TTable : class
    {
        if (string.IsNullOrEmpty(alias))
            alias = GenerateAlias();
        else if (_usedAliases.Contains(alias))
            throw new ArgumentOutOfRangeException($"Duplicate alias name: {alias}");

        _usedAliases.Add(alias);
        Tables.Add(typeof(TTable), new TableModel(typeof(TTable), alias));

        return this;
    }

    private string GenerateAlias()
    {
        bool checker = true;
        string result = string.Empty;

        while (checker)
        {
            result = (_tableAlias == 'z' ? 'a' : _tableAlias).ToString();
            _tableAlias = (char)(_tableAlias + 1);

            if (_usedAliases.Contains(result))
                continue;

            checker = false;
            _usedAliases.Add(result);
        }

        return result;
    }

    #endregion Add Table

    public bool ContainsEntity(Type entity) => Tables.ContainsKey(entity);

    public TableModel GetTable(Type entityType)
    {
        if (!Tables.TryGetValue(entityType, out TableModel value))
            throw new MissingMemberException("Table not found.");

        return value;
    }

    public TableModel GetTable(Expression? expression)
    {
        Type? entityType = expression switch
        {
            MemberExpression member => member.Expression?.Type,
            UnaryExpression { Operand: MemberExpression member2 } => member2.Expression?.Type,
            _ => null
        };

        if (entityType == null)
            throw new MissingMemberException("Expecting MemberExpression expression.");
        
        if (!Tables.TryGetValue(entityType, out TableModel value))
            throw new MissingMemberException("Table not found.");

        return value;
    }

    public ColumnModel GetColumn(Type entityType, string columnName)
    {
        return Tables.TryGetValue(entityType, out TableModel value) ? value[columnName] : default;
    }

    #region Add Joins
    public void JoinWithMapper(Action<IJoinMapper> mapper)
    {
        JoinMapper joinMapper = new();
        mapper.Invoke(joinMapper);

        foreach (var item in joinMapper.JoinMaps)
        {
            if (item.UseEntity)
            {
                var table1 = Tables[item.Table1];
                var table2 = Tables[item.Table2];

                if (!table1.HasPrimaryKeyField)
                    throw new MissingFieldException($"Missing primary key field identifier on table {table1.TableName}");

                JoinInfo.Add(new JoinModel(item.JoinType, table2.TableName, table2.TableAlias)
                {
                    ColumnLeft = table1.PrimaryKeyField,
                    ColumnRight = table1.PrimaryKeyField,
                    TargetTableAlias = table1.TableAlias
                });
            }
            else
            {
                ArgumentNullException.ThrowIfNull(item.JoinExpression);
                AddJoin(item.JoinType, item.JoinExpression);
            }
        }
    }
    /// <summary>
    /// Join tables <typeparamref name="TTable1"/> and <typeparamref name="TTable2"/> using Primary Key field
    /// identified in <see cref="TableNameAttribute"/>.  By convention, method will assume Primary Key name
    /// in <typeparamref name="TTable1"/> has an associated field name in <typeparamref name="TTable2"/>.
    /// </summary>
    /// <typeparam name="TTable1">Join table one.  Expects entity to have Primary Key identified in attribute.</typeparam>
    /// <typeparam name="TTable2">Join table two.</typeparam>
    /// <param name="joinType">Join type.</param>
    /// <exception cref="MissingFieldException">Thrown if Primary Key is missing on <typeparamref name="TTable1"/> entity.</exception>
    public void AddJoin<TTable1, TTable2>(JoinType joinType) where TTable1 : class where TTable2 : class
    {
        var table1 = Tables[typeof(TTable1)];
        var table2 = Tables[typeof(TTable2)];

        if (!table1.HasPrimaryKeyField)
            throw new MissingFieldException($"Missing primary key field identifier on table {table1.TableName}");

        JoinInfo.Add(new JoinModel(joinType, table2.TableName, table2.TableAlias)
        {
            ColumnLeft = table1.PrimaryKeyField,
            ColumnRight = table1.PrimaryKeyField,
            TargetTableAlias = table1.TableAlias
        });
    }

    public void AddJoin(JoinType joinType, LambdaExpression expression)
    {
        if (expression.Body is not BinaryExpression binary)
            throw new TypeAccessException("Expected Join parameter type of BinaryExpression.");

        if (binary is { NodeType: ExpressionType.And, Left: BinaryExpression binaryLeft, Right: BinaryExpression binaryRight }) 
        {
            //composite join
            var table1L2 = GetTable(binaryLeft.Right);
            JoinModel model = new(joinType, table1L2.TableName, table1L2.TableAlias)
            {
                ColumnLeft = binaryLeft.Left.ResolveName(),
                ColumnRight = binaryLeft.Right.ResolveName(),
                TargetTableAlias = GetTable(binaryLeft.Left).TableAlias,
                ColumnLeft2 = binaryRight.Left.ResolveName()
            };

            if (IsConstantOrUnary(binaryRight.Right, out var value))
            {
                model.ColumnRight2 = TypeHelper.IsNumeric(value) ? $"{value}" : $"'{value}'";
                model.IsCompositeJoinConstant = true;
                model.CompositeTargetTableAlias = GetTable(binaryRight.Left).TableAlias;
            }
            else
            {
                model.ColumnRight2 = binaryRight.Right.ResolveName();
                model.IsCompositeJoin = true;
            }

            JoinInfo.Add(model);
        }
        else
        {
            var tableRight = GetTable(binary.Right);
            //Standard join.
            JoinInfo.Add(new JoinModel(joinType, tableRight.TableName, tableRight.TableAlias)
            {
                ColumnLeft = binary.Left.ResolveName(),
                ColumnRight = binary.Right.ResolveName(),
                TargetTableAlias = GetTable(binary.Left).TableAlias
            });
        }

        return;

        static bool IsConstantOrUnary(Expression node, out object? value)
        {
            value = null;

            switch (node)
            {
                case ConstantExpression constant:
                    value = constant.Value;
                    return true;
                case UnaryExpression { Operand: ConstantExpression constant2 }:
                    value = constant2.Value;
                    return true;
                default:
                    return false;
            }
        }
    }

    #endregion

    #region CTE Join

    public string AddCteJoin(LambdaExpression joinColumnName, JoinType joinType, string cteAlias)
    {
        var member = joinColumnName.Body as MemberExpression;
        ArgumentNullException.ThrowIfNull(member?.Expression);
        JoinInfo.Add(new JoinModel(joinType, cteAlias, cteAlias)
        {
            ColumnLeft = member.Member.Name,
            ColumnRight = member.Member.Name,
            IsCteJoin = true,
            TargetTableAlias = Tables[member.Expression.Type].TableAlias
        });
        
        return member.Member.Name;
    }

    public JoinModel AddCteJoin(BinaryExpression binary, JoinType joinType, string cteName)
    {
        JoinModel model = new(joinType, cteName, cteName)
        {
            IsCteJoin = true
        };

        if (binary is { NodeType: ExpressionType.And, Right: BinaryExpression binaryRight, Left: BinaryExpression binaryLeft })  //composite join
        {
            var tableL1 = GetTable(binaryLeft.Left);

            model.ColumnLeft = binaryLeft.Left.ResolveName();
            model.ColumnRight = binaryLeft.Right.ResolveName();
            model.TargetTableAlias = tableL1.TableAlias;
            model.ColumnLeft2 = binaryRight.Left.ResolveName();
            model.ColumnRight2 = binaryRight.Right.ResolveName();
            model.IsCompositeJoin = true;
        }
        else
        {
            model.ColumnLeft = binary.Left.ResolveName();
            model.ColumnRight = binary.Right.ResolveName();
            model.TargetTableAlias = GetTable(binary.Left).TableAlias;
        }

        JoinInfo.Add(model);

        return model;
    }

    #endregion

    public string Compile()
    {
        StringBuilder builder = new StringBuilder().Append($" FROM {_parentTableName} AS {_parentTableAlias}").Append('\n');

        foreach (var item in JoinInfo)
        {
            if (item.JoinType == JoinType.Left)
                builder.Append($" LEFT OUTER JOIN {item.TableName}");
            else if (item.JoinType == JoinType.Right)
                builder.Append($" RIGHT OUTER JOIN {item.TableName}");
            else
                builder.Append($" JOIN {item.TableName}");

            if (!item.IsCteJoin)
                builder.Append($" AS {item.TableAlias}");

            builder.Append($" ON {item.TargetTableAlias}.{item.ColumnLeft} = {item.TableAlias}.{item.ColumnRight}");

            if (item.IsCompositeJoin)
                builder.Append($" AND {item.TargetTableAlias}.{item.ColumnLeft2} = {item.TableAlias}.{item.ColumnRight2}");
            else if (item.IsCompositeJoinConstant)
                builder.Append($" AND {item.CompositeTargetTableAlias}.{item.ColumnLeft2} = {item.ColumnRight2}");
        }

        return builder.ToString();
    }
}