using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace ASTA.WorldApi;
public class DesignTimeFactory : IDesignTimeDbContextFactory<AstaDbContext>
{
    public AstaDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<AstaDbContext>()
            .UseNpgsql("Host=localhost;Port=5434;Database=asta_world;Username=postgres;Password=postgres")
            .Options);
}
