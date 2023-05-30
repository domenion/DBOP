using DBOP.Models;

namespace DBOP.Tests;

public class DBContextTest
{
    [Fact]
    public void TestConnectToMySQLByConnectionString()
    {
        const string cs = "Database=db;Data Source=localhost;Port=3306;Username=test;Password=test;";
        var ctx = new DBContext(cs);
        var query = ctx.CreateMySqlQuery<TestEntity>();
        Assert.NotNull(query);
        Assert.True(query.TestConnection());
    }

    [Fact]
    public void TestConnectToMySQLByConnectionOptions()
    {
        var op = new ConnectionOptions
        {
            Source = Constants.DbSource.MySQL,
            Database = "db",
            DataSource = "localhost",
            Port = 3306,
            Username = "test",
            Password = "test",
        };
        var ctx = new DBContext(op);
        var query = ctx.CreateMySqlQuery<TestEntity>();
        Assert.NotNull(query);
        Assert.True(query.TestConnection());
    }

    [Fact]
    public void TestGenerateConnectionStringForOracle()
    {
        var op = new ConnectionOptions
        {
            Source = Constants.DbSource.Oracle,
            Database = "db",
            DataSource = "localhost",
            Port = 1234,
            Username = "test",
            Password = "test",
        };
        var ctx = new DBContext(op);

        const string expected = "Database=db;Data Source=localhost;Port=1234;User Id=test;Password=test;";
        Assert.Equal(expected, ctx.ConnectionString);
    }
}