using ASTA.GameApi;
using ASTA.SharedModels;
using Xunit;

namespace ASTA.Tests;

/// <summary>
/// Tests pour la validation des modèles via DataAnnotations
/// </summary>
public class ValidationTests
{
    [Fact]
    public void Validate_ValidPlayer_ReturnsTrue()
    {
        // Arrange
        var player = new Player { UserName = "TestUser", Level = 50 };

        // Act
        var (isValid, errors) = ValidationUtil.Validate(player);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_PlayerWithEmptyUserName_ReturnsFalse()
    {
        // Arrange
        var player = new Player { UserName = "", Level = 10 };

        // Act
        var (isValid, errors) = ValidationUtil.Validate(player);

        // Assert
        Assert.False(isValid);
        Assert.Contains("UserName", errors.Keys);
    }

    [Fact]
    public void Validate_PlayerWithInvalidLevel_ReturnsFalse()
    {
        // Arrange
        var player = new Player { UserName = "Test", Level = 150 }; // Max 100

        // Act
        var (isValid, errors) = ValidationUtil.Validate(player);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Level", errors.Keys);
    }

    [Fact]
    public void Validate_PlayerWithZeroLevel_ReturnsFalse()
    {
        // Arrange
        var player = new Player { UserName = "Test", Level = 0 }; // Min 1

        // Act
        var (isValid, errors) = ValidationUtil.Validate(player);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Level", errors.Keys);
    }

    [Fact]
    public void Validate_ValidAdmin_ReturnsTrue()
    {
        // Arrange
        var admin = new Admin { Login = "admin@test.com" };

        // Act
        var (isValid, errors) = ValidationUtil.Validate(admin);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_AdminWithEmptyLogin_ReturnsFalse()
    {
        // Arrange
        var admin = new Admin { Login = "" };

        // Act
        var (isValid, errors) = ValidationUtil.Validate(admin);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Login", errors.Keys);
    }

    [Fact]
    public void Validate_ValidDungeon_ReturnsTrue()
    {
        // Arrange
        var dungeon = new Dungeon { Name = "Test Dungeon" };

        // Act
        var (isValid, errors) = ValidationUtil.Validate(dungeon);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_DungeonWithEmptyName_ReturnsFalse()
    {
        // Arrange
        var dungeon = new Dungeon { Name = "" };

        // Act
        var (isValid, errors) = ValidationUtil.Validate(dungeon);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Name", errors.Keys);
    }

    [Fact]
    public void Validate_ValidRoom_ReturnsTrue()
    {
        // Arrange
        var room = new Room { Name = "Test Room", DungeonId = 1 };

        // Act
        var (isValid, errors) = ValidationUtil.Validate(room);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_RoomWithEmptyName_ReturnsFalse()
    {
        // Arrange
        var room = new Room { Name = "", DungeonId = 1 };

        // Act
        var (isValid, errors) = ValidationUtil.Validate(room);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Name", errors.Keys);
    }
}
