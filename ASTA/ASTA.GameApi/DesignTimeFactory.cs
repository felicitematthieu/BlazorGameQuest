using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ASTA.GameApi;


public class DesignTimeFactory : IDesignTimeDbContextFactory<AstaDbContext>
{
    public AstaDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AstaDbContext>()
            .UseNpgsql("Host=localhost;Port=5434;Database=asta_game;Username=postgres;Password=postgres")
            .Options;

        return new AstaDbContext(options);
    }
}
