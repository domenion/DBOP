namespace DBOP.Tests.Oracle;

public class InsertCommandTest
{
    [Fact]
    public void TestInsert()
    {
        var ctx = new DBContext("");
        var query = ctx.CreateOracleQuery<TestEntity>();
        var te = new TestEntity
        {
            Name = "test_name",
            Value = 100
        };
        query.Insert(te);

        const string expected = "INSERT INTO TEST_TABLE (NAME,VALUE) VALUES (:NAME,:VALUE)";
        Assert.Equal(expected, query.CommandText);
        Assert.Equal(te.Name, query.Parameters.First(p => p.Name == ":NAME")?.Value);
        Assert.Equal(te.Value, query.Parameters.First(p => p.Name == ":VALUE")?.Value);
    }
}
