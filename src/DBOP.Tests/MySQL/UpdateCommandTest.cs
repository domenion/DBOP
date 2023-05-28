namespace DBOP.Tests.MySQL;
public class UpdateCommandTest
{
    [Fact]
    public void TestUpdateWithObject()
    {
        var ctx = new DBContext("");
        var query = ctx.CreateMySqlQuery<TestEntity>();
        var te = new TestEntity
        {
            Name = "test_name",
            ModifiedDate = DateTime.Now,
        };
        query.Update(te);
        query.Where("ID", 1);

        const string expected = "UPDATE TEST_TABLE SET NAME=@NAME, MODIFIED_DATE=@MODIFIED_DATE WHERE ID = @ID0";
        Assert.Equal(expected, query.CommandText);
        Assert.Equal("test_name", query.Parameters.First(p => p.Name == "@NAME")?.Value);
        Assert.Equal(1, query.Parameters.First(p => p.Name == "@ID0")?.Value);
    }

    [Fact]
    public void TestUpdateWthSetKeyword()
    {
        var ctx = new DBContext("");
        var query = ctx.CreateMySqlQuery<TestEntity>();
        query.Update();
        query.Set("NAME", "test_name");
        query.Where("ID", 1);

        const string expected = "UPDATE TEST_TABLE SET NAME=@NAME WHERE ID = @ID0";
        Assert.Equal(expected, query.CommandText);
        Assert.Equal("test_name", query.Parameters.First(p => p.Name == "@NAME")?.Value);
        Assert.Equal(1, query.Parameters.First(p => p.Name == "@ID0")?.Value);
    }
}