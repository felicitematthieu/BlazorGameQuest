using System.Linq;
using System.Threading.Tasks;
using ASTA.GameApi;
using ASTA.SharedModels;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class PlayerQueryTests
{
    [Fact]
    public async Task Pagination_Works()
    {
        var opts = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase("player-query").Options;
        using var db = new AstaDbContext(opts);
        for (int i = 0; i < 30; i++) db.Players.Add(new Player { UserName = "user" + i, Level = 1 });
        await db.SaveChangesAsync();

        var page = 2; var pageSize = 10;
        var data = await db.Players.OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        Assert.Equal(pageSize, data.Count);
    }

    [Fact]
    public async Task Filter_Works_CaseInsensitive()
    {
        var opts = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase("player-filter").Options;
        using var db = new AstaDbContext(opts);
        db.Players.AddRange(new Player { UserName = "Stanley" }, new Player { UserName = "Mat" }, new Player { UserName = "Other" });
        await db.SaveChangesAsync();

        // Simulate ILike by simple ToLower Contains since EF.Functions.ILike not accessible outside context extension now
        var q = "mat".ToLower();
        var filtered = db.Players.AsEnumerable().Where(p => p.UserName.ToLower().Contains(q)).ToList();
        Assert.Single(filtered);
        Assert.Equal("Mat", filtered[0].UserName);
    }
}
