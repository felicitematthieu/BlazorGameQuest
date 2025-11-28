using ASTA.GameApi;
using ASTA.GameApi.Services;
using ASTA.SharedModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Xunit;

namespace ASTA.Tests;

/// <summary>
/// Tests supplémentaires pour les services métier
/// </summary>
public class ServiceTests
{
    [Fact]
    public async Task PlayerService_CreatePlayer_AddsToDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;
        await using var db = new AstaDbContext(options);
        var service = new PlayerService(db);

        // Act
        var player = await service.CreatePlayerAsync(new Player { UserName = "NewPlayer", Level = 25 });

        // Assert
        Assert.NotNull(player);
        Assert.Equal("NewPlayer", player.UserName);
        Assert.Equal(25, player.Level);
        Assert.True(player.Id > 0);

        var inDb = await db.Players.FindAsync(player.Id);
        Assert.NotNull(inDb);
    }

    [Fact]
    public async Task PlayerService_UpdatePlayer_ModifiesExisting()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;
        await using var db = new AstaDbContext(options);
        var service = new PlayerService(db);

        var player = await service.CreatePlayerAsync(new Player { UserName = "Original", Level = 10 });

        // Act
        var result = await service.UpdatePlayerAsync(player.Id, new Player { UserName = "Modified", Level = 50 });

        // Assert
        Assert.True(result);
        var updated = await db.Players.FindAsync(player.Id);
        Assert.NotNull(updated);
        Assert.Equal("Modified", updated.UserName);
        Assert.Equal(50, updated.Level);
    }

    [Fact]
    public async Task PlayerService_DeletePlayer_RemovesFromDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;
        await using var db = new AstaDbContext(options);
        var service = new PlayerService(db);

        var player = await service.CreatePlayerAsync(new Player { UserName = "ToDelete", Level = 5 });

        // Act
        var result = await service.DeletePlayerAsync(player.Id);

        // Assert
        Assert.True(result);
        var inDb = await db.Players.FindAsync(player.Id);
        Assert.Null(inDb);
    }

    [Fact]
    public async Task DungeonService_CreateDungeon_AddsToDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;
        await using var db = new AstaDbContext(options);
        var service = new DungeonService(db);

        // Act
        var dungeon = await service.CreateDungeonAsync(new Dungeon { Name = "Test Dungeon" });

        // Assert
        Assert.NotNull(dungeon);
        Assert.Equal("Test Dungeon", dungeon.Name);
        Assert.True(dungeon.Id > 0);
    }

    [Fact]
    public async Task DungeonService_AddRoom_AddsRoomToDungeon()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;
        await using var db = new AstaDbContext(options);
        var service = new DungeonService(db);

        var dungeon = await service.CreateDungeonAsync(new Dungeon { Name = "Test" });

        // Act
        var room = await service.AddRoomToDungeonAsync(dungeon.Id, new Room { Name = "New Room" });

        // Assert
        Assert.NotNull(room);
        Assert.Equal("New Room", room.Name);
        Assert.Equal(dungeon.Id, room.DungeonId);

        var reloaded = await db.Dungeons.Include(d => d.Rooms).FirstAsync(d => d.Id == dungeon.Id);
        Assert.Single(reloaded.Rooms);
    }

    [Fact]
    public async Task DungeonService_DeleteDungeon_RemovesWithRooms()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;
        await using var db = new AstaDbContext(options);
        var service = new DungeonService(db);

        var dungeon = await service.CreateDungeonAsync(new Dungeon { Name = "ToDelete" });
        await service.AddRoomToDungeonAsync(dungeon.Id, new Room { Name = "Room1" });
        await service.AddRoomToDungeonAsync(dungeon.Id, new Room { Name = "Room2" });

        // Act
        var result = await service.DeleteDungeonAsync(dungeon.Id);

        // Assert
        Assert.True(result);
        var inDb = await db.Dungeons.Include(d => d.Rooms).FirstOrDefaultAsync(d => d.Id == dungeon.Id);
        Assert.Null(inDb);

        // Rooms aussi supprimées (cascade)
        var rooms = await db.Rooms.Where(r => r.DungeonId == dungeon.Id).ToListAsync();
        Assert.Empty(rooms);
    }

    [Fact]
    public async Task AdventureService_StartNewAdventure_CreatesWithRooms()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;
        await using var db = new AstaDbContext(options);
        var service = new AdventureService(db);

        // Act
        var result = await service.StartNewAdventureAsync(null);

        // Assert - Convertir en JSON pour extraire les valeurs
        var json = JsonSerializer.Serialize(result);
        var doc = JsonDocument.Parse(json);
        var adventureId = doc.RootElement.GetProperty("AdventureId").GetInt32();
        var totalRooms = doc.RootElement.GetProperty("TotalRooms").GetInt32();

        Assert.True(adventureId > 0);
        Assert.InRange(totalRooms, 2, 5);

        var adventure = await db.Adventures.Include(a => a.Rooms).FirstAsync(a => a.Id == adventureId);
        Assert.Equal("InProgress", adventure.Status);
        Assert.Equal(totalRooms, adventure.Rooms.Count);
    }

    [Fact]
    public async Task AdventureService_SubmitChoice_UpdatesScore()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;
        await using var db = new AstaDbContext(options);
        var service = new AdventureService(db);

        var start = await service.StartNewAdventureAsync(null);
        var json = JsonSerializer.Serialize(start);
        var doc = JsonDocument.Parse(json);
        var adventureId = doc.RootElement.GetProperty("AdventureId").GetInt32();

        // Act
        var result = await service.SubmitChoiceAsync(adventureId, "Combattre");

        // Assert
        Assert.NotNull(result);
        // Score a changé
        var adventure = await db.Adventures.FindAsync(adventureId);
        Assert.NotEqual(0, adventure!.TotalScore);
    }

    [Fact]
    public async Task AdventureService_GetAdventureById_ReturnsCompleteAdventure()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AstaDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options;
        await using var db = new AstaDbContext(options);
        var service = new AdventureService(db);

        var start = await service.StartNewAdventureAsync(null);
        var json = JsonSerializer.Serialize(start);
        var doc = JsonDocument.Parse(json);
        var adventureId = doc.RootElement.GetProperty("AdventureId").GetInt32();

        // Act
        var adventure = await service.GetAdventureByIdAsync(adventureId);

        // Assert
        Assert.NotNull(adventure);
        Assert.Equal(adventureId, adventure.Id);
        Assert.NotEmpty(adventure.Rooms);
    }
}
