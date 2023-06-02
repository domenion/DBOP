using System.Data.Common;
using DBOP.Constants;
using DBOP.Extensions;
using DBOP.Interfaces;
using DBOP.Queries;

namespace DBOP;

public static class ExtJoinCommand
{
    public static Query<TMainEntity, TConnection, TCommand, TTransaction> Join<TMainEntity, TJoinEntity, TConnection, TCommand, TTransaction>(
        this Query<TMainEntity, TConnection, TCommand, TTransaction> query,
        TJoinEntity joinEntity,
        string joinAliases = "",
        Func<TJoinEntity, string>? joinKeyGetter = null,
        Func<TMainEntity, string>? mainKeyGetter = null,
        JoinDirection direction = JoinDirection.INNER,
        bool outer = false
    )
        where TMainEntity : IEntityBase, new()
        where TJoinEntity : IEntityBase, new()
        where TConnection : DbConnection
        where TCommand : DbCommand
        where TTransaction : DbTransaction
    {
        var joiningTbName = joinEntity.GetType().GetTableName();
        var joinKeyName = joinKeyGetter?.Invoke(joinEntity);
        var mainKeyName = mainKeyGetter?.Invoke(query.Entities.FirstOrDefault()!);

        if (string.IsNullOrWhiteSpace(joinAliases))
        {
            joinAliases = $"JT{query.JoiningCounters}";
        }

        if (string.IsNullOrWhiteSpace(joinKeyName))
        {
            joinKeyName = typeof(TJoinEntity).GetPrimaryKeyProperty()?.GetColumnName() ?? "ID";
        }

        if (string.IsNullOrWhiteSpace(mainKeyName))
        {
            mainKeyName = typeof(TMainEntity).GetPrimaryKeyProperty()?.GetColumnName() ?? "ID";
        }

        var directionTxt = direction.ToString();
        var outerTxt = outer ? " OUTER " : " ";
        var leftJoinCmd = $"{directionTxt}{outerTxt}JOIN {joiningTbName} {query.AliasOperator} {joinAliases} ON {joinAliases}.{joinKeyName} = {query.Aliases}.{mainKeyName}";
        query.JoinCommand = leftJoinCmd.Replace("  ", " ");
        query.JoiningCounters++;
        return query;
    }
}
