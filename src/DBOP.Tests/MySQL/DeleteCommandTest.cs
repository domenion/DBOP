namespace DBOP.Tests.MySQL;

public class DeleteCommandTest
{
    [Fact]
    public void TestDelete()
    {
        var ctx = new DBContext("");
        var query = ctx.CreateMySqlQuery<TestEntity>();
        var te = new TestEntity
        {
            ID = 1
        };
        query.Delete(te);

        const string expected = "DELETE FROM TEST_TABLE WHERE ID = @ID0";
        Assert.Equal(expected, query.CommandText);
        Assert.Equal(1, query.Parameters.First().Value);
    }

    [Fact]
    public void TestDeleteByIncorrectData()
    {
        var ctx = new DBContext("");
        var query = ctx.CreateMySqlQuery<TestEntity>();
        var te = new TestEntity();
        query.Delete(te);

        const string expected = "DELETE FROM TEST_TABLE WHERE ID = @ID0";
        Assert.Equal(expected, query.CommandText);
        Assert.Equal(0, query.Parameters.First().Value);
    }
}