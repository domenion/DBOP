using System.Data.Common;
using DBOP.Interfaces;
using DBOP.Queries;

namespace DBOP;
public static class ExtSetCommand
{
    /// <summary>
    /// SQL SET clause, e.g. SET field = @param
    /// </summary>
    /// <param name="query"></param>
    /// <param name="fieldGetter"></param>
    /// <param name="value"></param>
    /// <param name="excludeParam">Enabled to set not include value to parameter list</param>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TConnection"></typeparam>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TTransaction"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns>Query object</returns>
    public static Query<TEntity, TConnection, TCommand, TTransaction> Set<TEntity, TConnection, TCommand, TTransaction, TValue>(
        this Query<TEntity, TConnection, TCommand, TTransaction> query,
        Func<TEntity, string> fieldGetter,
        TValue value,
        bool excludeParam = false
    )
        where TEntity : IEntityBase, new()
        where TConnection : DbConnection
        where TCommand : DbCommand
        where TTransaction : DbTransaction
    {
        var field = fieldGetter(new TEntity());
        return query.Set(field, value, excludeParam);
    }

    public static Query<TEntity, TConnection, TCommand, TTransaction> Set<TEntity, TConnection, TCommand, TTransaction, TValue>(
        this Query<TEntity, TConnection, TCommand, TTransaction> query,
        string field,
        TValue value,
        bool excludeParam = false
    )
        where TEntity : IEntityBase, new()
        where TConnection : DbConnection
        where TCommand : DbCommand
        where TTransaction : DbTransaction
    {
        var param = query.ParameterPrefix + field;
        var setText = excludeParam ? $"{field}={value}" : $"{field}={param}";

        AddSETKeyword(query, setText);

        if (!excludeParam)
        {
            query.AddParameter(param, value!);
        }

        return query;
    }

    private static void AddSETKeyword<TEntity, TConnection, TCommand, TTransaction>(Query<TEntity, TConnection, TCommand, TTransaction> query, string command) where TEntity : IEntityBase, new()
        where TConnection : DbConnection
        where TCommand : DbCommand
        where TTransaction : DbTransaction
    {
        const string keyword = "SET";
        var connector = query.MainCommand.IndexOf(keyword) > 0 ? "," : " " + keyword;
        query.MainCommand = $"{query.MainCommand}{connector} {command}";
    }
}