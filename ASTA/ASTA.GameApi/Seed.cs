using ASTA.SharedModels;
using Microsoft.EntityFrameworkCore;

namespace ASTA.GameApi;

public static class Seed
{
    public static async Task EnsureAsync(AstaDbContext db, ILogger logger)
    {
        await db.Database.EnsureCreatedAsync();

        if (!await db.Players.AnyAsync())
        {
            db.Players.AddRange(
                new Player { UserName = "Stanley", Level = 7 },
                new Player { UserName = "Mat",     Level = 3 }
            );
        }

        if (!await db.Dungeons.AnyAsync())
        {
            var d1 = new Dungeon { Name = "Donjon de Cracovie" };
            d1.Rooms.AddRange(new [] { new Room{ Name="Salle 1"}, new Room{ Name="Boss" } });
            var d2 = new Dungeon { Name = "Forteresse EFREI" };
            db.Dungeons.AddRange(d1, d2);
        }

        await db.SaveChangesAsync();
        logger.LogInformation("✅ Seed done: {players} players, {dungeons} dungeons",
            await db.Players.CountAsync(), await db.Dungeons.CountAsync());
    }
}
