using ASTA.GameApi;
using ASTA.GameApi.Services;
using ASTA.SharedModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Xunit;

namespace ASTA.Tests;

/// <summary>
/// Tests pour le service d'administration
/// </summary>
public class AdminServiceTests
{
    [Fact]
    public async Task GetLeaderboard_ReturnsTopPlayersByScore()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;
        await using var db = new AstaDbContext(options);

        var player1 = new Player { UserName = "Player1", Level = 10 };
        var player2 = new Player { UserName = "Player2", Level = 15 };
        db.Players.AddRange(player1, player2);
        await db.SaveChangesAsync();

        db.Adventures.AddRange(
            new Adventure { PlayerId = player1.Id, TotalScore = 50, Status = "Completed" },
            new Adventure { PlayerId = player1.Id, TotalScore = 30, Status = "Completed" },
            new Adventure { PlayerId = player2.Id, TotalScore = 100, Status = "Completed" }
        );
        await db.SaveChangesAsync();

        var service = new AdminService(db);

        // Act
        var leaderboard = await service.GetLeaderboardAsync(10);

        // Assert
        Assert.Equal(2, leaderboard.Count);
        
        // Vérifier que Player2 est premier (score total: 100)
        var json = JsonSerializer.Serialize(leaderboard[0]);
        var doc = JsonDocument.Parse(json);
        Assert.Equal(player2.Id, doc.RootElement.GetProperty("PlayerId").GetInt32());
        Assert.Equal(100, doc.RootElement.GetProperty("TotalScore").GetInt32());
    }

    [Fact]
    public async Task GetPlayerHistory_ReturnsPlayerAdventures()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;
        await using var db = new AstaDbContext(options);

        var player = new Player { UserName = "TestPlayer", Level = 5 };
        db.Players.Add(player);
        await db.SaveChangesAsync();

        db.Adventures.AddRange(
            new Adventure { PlayerId = player.Id, TotalScore = 20, Status = "Completed" },
            new Adventure { PlayerId = player.Id, TotalScore = -5, Status = "Dead" }
        );
        await db.SaveChangesAsync();

        var service = new AdminService(db);

        // Act
        var history = await service.GetPlayerHistoryAsync(player.Id);

        // Assert
        Assert.Equal(2, history.Count);
        Assert.All(history, adv => Assert.Equal(player.Id, adv.PlayerId));
    }

    [Fact]
    public async Task SetPlayerActiveStatus_UpdatesPlayerStatus()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;
        await using var db = new AstaDbContext(options);

        var player = new Player { UserName = "ActivePlayer", Level = 10, IsActive = true };
        db.Players.Add(player);
        await db.SaveChangesAsync();

        var service = new AdminService(db);

        // Act
        var result = await service.SetPlayerActiveStatusAsync(player.Id, false);

        // Assert
        Assert.True(result);
        var updated = await db.Players.FindAsync(player.Id);
        Assert.NotNull(updated);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task ExportPlayersToCsv_GeneratesValidCsv()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;
        await using var db = new AstaDbContext(options);

        db.Players.AddRange(
            new Player { UserName = "Player1", Level = 5, IsActive = true },
            new Player { UserName = "Player2", Level = 10, IsActive = false }
        );
        await db.SaveChangesAsync();

        var service = new AdminService(db);

        // Act
        var csv = await service.ExportPlayersToCsvAsync();

        // Assert
        Assert.NotNull(csv);
        Assert.Contains("Id,UserName,Level,IsActive,TotalScore,AdventureCount", csv);
        Assert.Contains("Player1", csv);
        Assert.Contains("Player2", csv);
        
        // Vérifier le format (au moins 3 lignes: header + 2 players)
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length >= 3);
    }

    [Fact]
    public async Task GetAllAdventures_FiltersCorrectly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;
        await using var db = new AstaDbContext(options);

        var player = new Player { UserName = "TestPlayer", Level = 5 };
        db.Players.Add(player);
        await db.SaveChangesAsync();

        db.Adventures.AddRange(
            new Adventure { PlayerId = player.Id, TotalScore = 20, Status = "Completed" },
            new Adventure { PlayerId = player.Id, TotalScore = 30, Status = "InProgress" },
            new Adventure { PlayerId = null, TotalScore = 10, Status = "Completed" }
        );
        await db.SaveChangesAsync();

        var service = new AdminService(db);

        // Act - filtrer par joueur
        var resultByPlayer = await service.GetAllAdventuresAsync(playerId: player.Id);
        var json = JsonSerializer.Serialize(resultByPlayer);
        var doc = JsonDocument.Parse(json);
        var total = doc.RootElement.GetProperty("total").GetInt32();

        // Assert
        Assert.Equal(2, total); // 2 aventures pour ce joueur

        // Act - filtrer par statut
        var resultByStatus = await service.GetAllAdventuresAsync(status: "Completed");
        json = JsonSerializer.Serialize(resultByStatus);
        doc = JsonDocument.Parse(json);
        total = doc.RootElement.GetProperty("total").GetInt32();

        // Assert
        Assert.Equal(2, total); // 2 aventures "Completed"
    }
}
