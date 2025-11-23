using System.Threading.Tasks;
using ASTA.GameApi;
using ASTA.SharedModels;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class DungeonTests
{
    [Fact]
    public async Task Creating_Dungeon_With_Rooms_Persists_Cascade()
    {
        var opts = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase("dungeon-test").Options;
        using var db = new AstaDbContext(opts);

        var dungeon = new Dungeon { Name = "Test Dungeon" };
        dungeon.Rooms.Add(new Room { Name = "Room A" });
        dungeon.Rooms.Add(new Room { Name = "Room B" });
        db.Dungeons.Add(dungeon);
        await db.SaveChangesAsync();

        var loaded = await db.Dungeons.Include(d => d.Rooms).FirstAsync();
        Assert.Equal(2, loaded.Rooms.Count);
        Assert.All(loaded.Rooms, r => Assert.Equal(loaded.Id, r.DungeonId));
    }
}
