namespace DBOP.Tests.MySQL;

public class SelectCommandTest
{
    [Fact]
    public void TestSelectStar()
    {
        var ctx = new DBContext("");
        var query = ctx.CreateMySqlQuery<TestEntity>();
        query.Select();

        const string expected = "SELECT * FROM TEST_TABLE AS T0";
        Assert.Equal(expected, query.CommandText);
    }

    [Fact]
    public void TestSelectBySpecificColumn()
    {
        var ctx = new DBContext("");
        var query = ctx.CreateMySqlQuery<TestEntity>();
        query.Select("NAME");

        const string expected = "SELECT T0.NAME FROM TEST_TABLE AS T0";
        Assert.Equal(expected, query.CommandText);
    }

    [Fact]
    public void TestSelectWithWhereCondition()
    {
        var ctx = new DBContext("");
        var query = ctx.CreateMySqlQuery<TestEntity>();
        var createDate = DateTime.Now;
        query.Select();
        query.Where("FLAG", true);
        query.Where("CREATE_DATE", createDate, Constants.Signs.LessThan);

        const string expected = "SELECT * FROM TEST_TABLE AS T0 WHERE T0.FLAG = @FLAG0 AND T0.CREATE_DATE < @CREATE_DATE0";
        Assert.Equal(expected, query.CommandText);
        Assert.Equal(true, query.Parameters.First(p => p.Name == "@FLAG0")?.Value);
        Assert.Equal(createDate, query.Parameters.First(p => p.Name == "@CREATE_DATE0")?.Value);
    }

    [Fact]
    public void TestSelectWithLeftJoin()
    {
        var ctx = new DBContext("");
        var query = ctx.CreateMySqlQuery<TestEntity>();
        var te = new TestEntity
        {
            ID = 123,
            Name = "test_name"
        };
        query.Select(te);
        query.LeftJoin(new SubTestEntity());

        const string expected = "SELECT * FROM TEST_TABLE AS T0 LEFT JOIN SUB_TEST_TABLE AS JT0 ON JT0.ID = T0.ID WHERE T0.ID = @ID0 AND T0.NAME = @NAME0";
        Assert.Equal(expected, query.CommandText);
        Assert.Equal(123, query.Parameters.First(p => p.Name == "@ID0")?.Value);
        Assert.Equal("test_name", query.Parameters.First(p => p.Name == "@NAME0")?.Value);
    }
}