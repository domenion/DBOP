namespace DBOP.Tests.MySQL;

public class InsertCommandTest
{
    [Fact]
    public void TestInsert()
    {
        var ctx = new DBContext("");
        var query = ctx.CreateMySqlQuery<TestEntity>();
        var te = new TestEntity
        {
            Name = "test_name",
            Value = 100
        };
        query.Insert(te);

        const string expected = "INSERT INTO TEST_TABLE (NAME,VALUE) VALUES (@NAME,@VALUE)";
        Assert.Equal(expected, query.CommandText);
        Assert.Equal("test_name", query.Parameters.First(p => p.Name == "@NAME")?.Value);
        Assert.Equal(100, query.Parameters.First(p => p.Name == "@VALUE")?.Value);
    }
}