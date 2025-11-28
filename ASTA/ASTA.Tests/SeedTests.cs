using ASTA.GameApi;
using ASTA.SharedModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ASTA.Tests;

/// <summary>
/// Tests pour la logique de seed de données
/// </summary>
public class SeedTests
{
    [Fact]
    public async Task EnsureAsync_EmptyDatabase_CreatesPlayersAndDungeons()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase($"seed-test-{Guid.NewGuid()}")
            .Options;
        await using var db = new AstaDbContext(options);
        var logger = NullLogger.Instance;

        // Act
        await Seed.EnsureAsync(db, logger);

        // Assert
        var players = await db.Players.ToListAsync();
        var dungeons = await db.Dungeons.Include(d => d.Rooms).ToListAsync();

        Assert.Equal(2, players.Count);
        Assert.Contains(players, p => p.UserName == "Stanley" && p.Level == 7);
        Assert.Contains(players, p => p.UserName == "Mat" && p.Level == 3);

        Assert.Equal(2, dungeons.Count);
        Assert.Contains(dungeons, d => d.Name == "Donjon de Cracovie");
        Assert.Contains(dungeons, d => d.Name == "Forteresse EFREI");

        var cracovie = dungeons.First(d => d.Name == "Donjon de Cracovie");
        Assert.Equal(2, cracovie.Rooms.Count);
    }

    [Fact]
    public async Task EnsureAsync_DatabaseAlreadySeeded_DoesNotDuplicate()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase($"seed-test-{Guid.NewGuid()}")
            .Options;
        await using var db = new AstaDbContext(options);
        var logger = NullLogger.Instance;

        // Premier seed
        await Seed.EnsureAsync(db, logger);

        // Act - deuxième appel
        await Seed.EnsureAsync(db, logger);

        // Assert - pas de duplication
        var players = await db.Players.ToListAsync();
        var dungeons = await db.Dungeons.ToListAsync();

        Assert.Equal(2, players.Count);
        Assert.Equal(2, dungeons.Count);
    }

    [Fact]
    public async Task EnsureAsync_DatabaseWithPlayersButNoDungeons_OnlyAddsDungeons()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase($"seed-test-{Guid.NewGuid()}")
            .Options;
        await using var db = new AstaDbContext(options);
        var logger = NullLogger.Instance;

        // Ajouter manuellement des joueurs
        db.Players.Add(new Player { UserName = "ExistingPlayer", Level = 5 });
        await db.SaveChangesAsync();

        // Act
        await Seed.EnsureAsync(db, logger);

        // Assert
        var players = await db.Players.ToListAsync();
        var dungeons = await db.Dungeons.ToListAsync();

        // Un seul joueur (pas de seed)
        Assert.Single(players);
        Assert.Equal("ExistingPlayer", players[0].UserName);

        // Mais les donjons ont été créés
        Assert.Equal(2, dungeons.Count);
    }
}
