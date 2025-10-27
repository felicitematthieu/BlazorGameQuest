using ASTA.GameApi;
using ASTA.SharedModels;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class PlayerTests
{
    [Fact]
    public async Task Can_Create_Player()
    {
        var opts = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase("t").Options;
        using var db = new AstaDbContext(opts);
        db.Players.Add(new Player { UserName = "test", Level = 3 });
        await db.SaveChangesAsync();
        Assert.Single(db.Players);
    }
    [Fact]
public async Task Can_Read_Player_After_Create()
{
    var opts = new DbContextOptionsBuilder<AstaDbContext>()
        .UseInMemoryDatabase("t-read").Options;
    using var db = new AstaDbContext(opts);
    var p = new Player { UserName = "reader", Level = 4 };
    db.Players.Add(p);
    await db.SaveChangesAsync();

    var found = await db.Players.FindAsync(p.Id);
    Assert.NotNull(found);
    Assert.Equal("reader", found!.UserName);
}

}
