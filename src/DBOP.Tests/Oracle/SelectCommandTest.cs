namespace DBOP.Tests.Oracle;

public class SelectCommandTest
{
    [Fact]
    public void TestSelectStar()
    {
        var ctx = new DBContext("");
        var query = ctx.CreateOracleQuery<TestEntity>();
        query.Select();

        const string expected = "SELECT * FROM TEST_TABLE T0";
        Assert.Equal(expected, query.CommandText);
    }

    [Fact]
    public void TestSelectWithObject()
    {
        var ctx = new DBContext("");
        var query = ctx.CreateOracleQuery<TestEntity>();
        query.Select(new TestEntity()
        {
            Name = "test"
        });

        const string expected = "SELECT * FROM TEST_TABLE T0 WHERE T0.NAME = :NAME0";
        Assert.Equal(expected, query.CommandText);
    }

    [Fact]
    public void TestSelectBySpecificColumn()
    {
        var ctx = new DBContext("");
        var query = ctx.CreateOracleQuery<TestEntity>();
        query.Select("NAME");

        const string expected = "SELECT T0.NAME FROM TEST_TABLE T0";
        Assert.Equal(expected, query.CommandText);
    }

    [Fact]
    public void TestSelectWithWhereCondition()
    {
        var ctx = new DBContext("");
        var query = ctx.CreateOracleQuery<TestEntity>();
        var createDate = DateTime.Now;
        query.Select();
        query.Where("FLAG", true);
        query.Where("CREATE_DATE", createDate, Constants.Signs.LessThan);

        const string expected = "SELECT * FROM TEST_TABLE T0 WHERE T0.FLAG = :FLAG0 AND T0.CREATE_DATE < :CREATE_DATE0";
        Assert.Equal(expected, query.CommandText);
        Assert.Equal(true, query.Parameters.First(p => p.Name == ":FLAG0")?.Value);
        Assert.Equal(createDate, query.Parameters.First(p => p.Name == ":CREATE_DATE0")?.Value);
    }

    [Fact]
    public void TestSelectWithLeftJoin()
    {
        var ctx = new DBContext("");
        var query = ctx.CreateOracleQuery<TestEntity>();
        var refItem = new TestEntity
        {
            ID = 123,
            Name = "test_name"
        };
        query.Select(refItem);
        query.Join(new SubTestEntity(), direction: Constants.JoinDirection.LEFT);

        const string expected = "SELECT * FROM TEST_TABLE T0 LEFT JOIN SUB_TEST_TABLE JT0 ON JT0.ID = T0.ID WHERE T0.ID = :ID0 AND T0.NAME = :NAME0";
        Assert.Equal(expected, query.CommandText);
        Assert.Equal(refItem.ID, query.Parameters.First(p => p.Name == ":ID0")?.Value);
        Assert.Equal(refItem.Name, query.Parameters.First(p => p.Name == ":NAME0")?.Value);
    }

    [Fact]
    public void TestSelectWithLeftOuterJoin()
    {
        var ctx = new DBContext("");
        var query = ctx.CreateOracleQuery<TestEntity>();
        var te = new TestEntity
        {
            ID = 123,
            Name = "test_name"
        };
        query.Select(te);
        query.Join(new SubTestEntity(), direction: Constants.JoinDirection.LEFT, outer: true);

        const string expected = "SELECT * FROM TEST_TABLE T0 LEFT OUTER JOIN SUB_TEST_TABLE JT0 ON JT0.ID = T0.ID WHERE T0.ID = :ID0 AND T0.NAME = :NAME0";
        Assert.Equal(expected, query.CommandText);
        Assert.Equal(te.ID, query.Parameters.First(p => p.Name == ":ID0")?.Value);
        Assert.Equal(te.Name, query.Parameters.First(p => p.Name == ":NAME0")?.Value);
    }
}