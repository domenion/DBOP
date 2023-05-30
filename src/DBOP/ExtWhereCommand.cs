using System.Data.Common;
using System.Text;
using DBOP.Interfaces;
using DBOP.Queries;

namespace DBOP;

public static class ExtWhereCommand
{
    public static Query<TEntity, TConnection, TCommand, TTransaction> Where<TEntity, TConnection, TCommand, TTransaction>(
        this Query<TEntity, TConnection, TCommand, TTransaction> query,
        string clause
    )
        where TEntity : IEntityBase, new()
        where TConnection : DbConnection
        where TCommand : DbCommand
        where TTransaction : DbTransaction
    {
        AddWHERECondition(query, clause);
        return query;
    }

    public static Query<TEntity, TConnection, TCommand, TTransaction> Where<TEntity, TConnection, TCommand, TTransaction, TValue>(
        this Query<TEntity, TConnection, TCommand, TTransaction> query,
        string field,
        TValue value,
        string sign = "="
    )
        where TEntity : IEntityBase, new()
        where TConnection : DbConnection
        where TCommand : DbCommand
        where TTransaction : DbTransaction
    {
        var param = query.ParameterPrefix + field;
        param = query.VerifyParameterName(param);
        query.AddParameter(param, value!);

        var aliases = string.IsNullOrWhiteSpace(query.Aliases) ? "" : $"{query.Aliases}.";
        var clause = $"{aliases}{field} {sign} {param}";
        AddWHERECondition(query, clause);

        return query;
    }

    /// <summary>
    /// Where field IN (...)
    /// </summary>
    /// <param name="query"></param>
    /// <param name="field"></param>
    /// <param name="values"></param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TConnection"></typeparam>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TTransaction"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns>Query object</returns>
    public static Query<TEntity, TConnection, TCommand, TTransaction> Where<TEntity, TConnection, TCommand, TTransaction, TValue>(
        this Query<TEntity, TConnection, TCommand, TTransaction> query,
        string field,
        params TValue[] values
    )
        where TEntity : IEntityBase, new()
        where TConnection : DbConnection
        where TCommand : DbCommand
        where TTransaction : DbTransaction
    {
        var param = query.ParameterPrefix + field;
        StringBuilder sb = new();
        foreach (var value in values)
        {
            var verifiedParam = query.VerifyParameterName(param);
            query.AddParameter(verifiedParam, value!);
            sb.Append(',');
            sb.Append(verifiedParam);
        }

        var cv = sb.Length > 1 ? sb.ToString()[1..] : "''";
        var clause = $"{field} IN ({cv})";
        AddWHERECondition(query, clause);

        return query;
    }

    private static void AddWHERECondition<TEntity, TConnection, TCommand, TTransaction>(
        Query<TEntity, TConnection, TCommand, TTransaction> query,
        string clause,
        string separator = "AND"
    )
        where TEntity : IEntityBase, new()
        where TConnection : DbConnection
        where TCommand : DbCommand
        where TTransaction : DbTransaction
    {
        clause = clause.Trim();
        if ((clause.StartsWith("AND") || clause.StartsWith("and")) && clause.Length > 3)
        {
            clause = clause[3..].Trim();
        }

        query.ConditionCommand ??= "";
        var connector = query.ConditionCommand.IndexOf("WHERE") > -1 ? separator : "WHERE";
        query.ConditionCommand = $"{query.ConditionCommand} {connector} {clause}".TrimStart();
    }
}
