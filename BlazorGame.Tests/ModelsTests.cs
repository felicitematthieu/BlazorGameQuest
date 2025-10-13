// Tests unitaires des modèles partagés (valeurs par défaut, types).

using SharedModels;
using Xunit;

public class ModelsTests
{
    [Fact]
    public void Player_Defaults_Are_Initialized()
    {
        var p = new Player();
        Assert.False(string.IsNullOrWhiteSpace(p.Id));
        Assert.Equal("Aventurier", p.Nickname);
        Assert.Equal(0, p.TotalScore);
        Assert.True(p.IsActive);
    }

    [Fact]
    public void Room_Defaults_Are_Initialized()
    {
        var r = new Room();
        Assert.Equal("Salle 1", r.Title);
        Assert.Equal(RoomType.Enemy, r.Type);
        Assert.False(string.IsNullOrWhiteSpace(r.Description));
    }
}
