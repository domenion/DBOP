using DBOP.Interfaces;
using MySql.Data.MySqlClient;

namespace DBOP.Queries;

public class MySqlQuery<TEntity> : Query<TEntity, MySqlConnection, MySqlCommand, MySqlTransaction>
    where TEntity : IEntityBase, new()
{
    public MySqlQuery(string connectionString) : base(connectionString)
    {
        DbConnection = new(ConnectionString);
        DbCommand = new();
        ParameterPrefix = "@";
    }

    public MySqlQuery(Query<TEntity, MySqlConnection, MySqlCommand, MySqlTransaction> query) : base(query)
    {
        ParameterPrefix = "@";
    }

    public override MySqlConnection? DbConnection { get; set; }

    public override MySqlCommand? DbCommand { get; set; }

    public override MySqlTransaction? DbTransaction { get; set; }
}