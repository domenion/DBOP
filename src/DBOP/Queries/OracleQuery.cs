using System.Data;
using DBOP.Extensions;
using DBOP.Interfaces;
using Oracle.ManagedDataAccess.Client;

namespace DBOP.Queries;

public class OracleQuery<TEntity> : Query<TEntity, OracleConnection, OracleCommand, OracleTransaction>
    where TEntity : IEntityBase, new()
{
    public OracleQuery(string connectionString) : base(connectionString)
    {
        DbConnection = new(ConnectionString);
        DbCommand = new();
        ParameterPrefix = ":";
        AliasOperator = "";
    }

    public OracleQuery(Query<TEntity, OracleConnection, OracleCommand, OracleTransaction> query) : base(query)
    {
        DbConnection = new(ConnectionString);
        DbCommand = new();
        ParameterPrefix = ":";
        AliasOperator = "";
    }

    public override OracleConnection? DbConnection { get; set; }

    public override OracleCommand? DbCommand { get; set; }

    public override OracleTransaction? DbTransaction { get; set; }

    public OracleQuery<TEntity> ReturnIdentity(string idName = "", string idSeqName = "")
    {
        if (string.IsNullOrEmpty(idName))
        {
            idName = typeof(TEntity).GetPrimaryKeyProperty()?.GetColumnName() ?? "ID";
        }

        if (!string.IsNullOrEmpty(idSeqName))
        {
            var idValue = $"{idSeqName}.NEXTVAL";
            var arr = MainCommand.Split(" ");
            var newC = arr[3].Replace("(", $"({idName},");
            var newV = arr[5].Replace("(", $"({idValue},");
            MainCommand = MainCommand.Replace(arr[3], newC).Replace(arr[5], newV);
        }

        ExtendedCommand += $" RETURNING {idName} INTO {ParameterPrefix}{idName}";
        AddParameter(new OracleParameter(idName, OracleDbType.Int32) { Direction = ParameterDirection.Output });
        return this;
    }

    public int GetReturningIdentity()
    {
        var idName = typeof(TEntity).GetPrimaryKeyProperty()?.GetColumnName() ?? "ID";
        return Convert.ToInt32(DbCommand?.Parameters[idName].Value ?? "-1");
    }

    public int GetReturningIdentity(string idName)
    {
        return Convert.ToInt32(DbCommand?.Parameters[idName].Value ?? "-1");
    }
}
