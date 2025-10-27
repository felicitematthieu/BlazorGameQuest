using ASTA.SharedModels;
using Microsoft.EntityFrameworkCore;

namespace ASTA.GameApi;

public class AstaDbContext(DbContextOptions<AstaDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Dungeon> Dungeons => Set<Dungeon>();
    public DbSet<Room> Rooms => Set<Room>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Player>().HasIndex(p => p.UserName).IsUnique();
        b.Entity<Dungeon>()
            .HasMany(d => d.Rooms)
            .WithOne()
            .HasForeignKey(r => r.DungeonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
