using System.Text;
using System.Text.RegularExpressions;
using DBOP.Interfaces;
using DBOP.Models;
using DBOP.Queries;

namespace DBOP;

public class DBContext
{
    public DBContext(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public DBContext(ConnectionOptions options)
    {
        var toSentenceCase = options.Source == Constants.DbSource.Oracle;
        ConnectionString = GenerateConnectionString(options, toSentenceCase);
    }

    public string ConnectionString { get; set; }

    public static string GenerateConnectionString(ConnectionOptions options, bool toSentenceCase = false)
    {
        if (options == null)
        {
            return "";
        }

        var sb = new StringBuilder();
        foreach (var prop in options.GetType().GetProperties())
        {
            var value = prop.GetValue(options, null);
            if (value != null)
            {
                if (prop.Name == "Source")
                {
                    continue;
                }

                var name = prop.Name;
                if (name == "Username" && options.Source == Constants.DbSource.Oracle)
                {
                    name = "UserId";
                }

                name = toSentenceCase ? ToSentenceCase(name) : name;
                sb.Append(name);
                sb.Append('=');
                sb.Append(value);
                sb.Append(';');
            }
        }

        return sb.ToString();
    }

    private static string ToSentenceCase(string str)
    {
        return Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + " " + m.Value[1]);
    }

    public OracleQuery<TEntity> CreateOracleQuery<TEntity>()
        where TEntity : IEntityBase, new()
    {
        return new OracleQuery<TEntity>(ConnectionString);
    }

    public static OracleQuery<TEntity> CreateOracleQuery<TEntity>(string connectionString)
        where TEntity : IEntityBase, new()
    {
        return new OracleQuery<TEntity>(connectionString);
    }

    public MySqlQuery<TEntity> CreateMySqlQuery<TEntity>()
        where TEntity : IEntityBase, new()
    {
        return new MySqlQuery<TEntity>(ConnectionString);
    }

    public static MySqlQuery<TEntity> CreateMySqlQuery<TEntity>(string connectionString)
        where TEntity : IEntityBase, new()
    {
        return new MySqlQuery<TEntity>(connectionString);
    }
}