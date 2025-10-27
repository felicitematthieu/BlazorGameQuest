using ASTA.SharedModels;
using Microsoft.EntityFrameworkCore;

namespace ASTA.WorldApi;

public static class Seed
{
    public static async Task EnsureAsync(AstaDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        if (!await db.Dungeons.AnyAsync())
        {
            var d = new Dungeon { Name = "Donjon World API" };
            d.Rooms.AddRange(new[] { new Room { Name = "Entrée" }, new Room { Name = "Boss" } });
            db.Dungeons.Add(d);
            await db.SaveChangesAsync();
        }
    }
}
