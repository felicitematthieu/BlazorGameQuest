using ASTA.SharedModels;
using Microsoft.EntityFrameworkCore;

namespace ASTA.WorldApi;

public class AstaDbContext(DbContextOptions<AstaDbContext> options) : DbContext(options)
{
    public DbSet<Dungeon> Dungeons => Set<Dungeon>();
    public DbSet<Room> Rooms => Set<Room>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Dungeon>()
         .HasMany(d => d.Rooms)
         .WithOne()
         .HasForeignKey(r => r.DungeonId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
