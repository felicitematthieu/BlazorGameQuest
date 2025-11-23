using System.Linq;
using ASTA.GameApi;
using ASTA.SharedModels;
using Xunit;

public class AdventureGeneratorTests
{
    [Fact]
    public void GenerateAdventure_ProducesValidAdventure()
    {
        var adventure = AdventureGenerator.GenerateAdventure();
        
        Assert.NotNull(adventure);
        Assert.InRange(adventure.Rooms.Count, 2, 5);
        Assert.Equal("InProgress", adventure.Status);
        Assert.Equal(0, adventure.TotalScore);
        Assert.All(adventure.Rooms, room => Assert.False(string.IsNullOrWhiteSpace(room.RoomTitle)));
    }

    [Fact]
    public void CalculateScoreDelta_Enemy_Combattre_ReturnsValidScore()
    {
        var score = AdventureGenerator.CalculateScoreDelta("Enemy", "Combattre");
        Assert.True(score == 10 || score == -5, "Enemy/Combattre should return 10 or -5");
    }

    [Fact]
    public void CalculateScoreDelta_Enemy_Fuir_ReturnsPositive()
    {
        var score = AdventureGenerator.CalculateScoreDelta("Enemy", "Fuir");
        Assert.Equal(2, score);
    }

    [Fact]
    public void CalculateScoreDelta_Treasure_Ouvrir_ReturnsValidScore()
    {
        var score = AdventureGenerator.CalculateScoreDelta("Treasure", "Ouvrir");
        Assert.True(score == 15 || score == -10, "Treasure/Ouvrir should return 15 or -10");
    }
}
