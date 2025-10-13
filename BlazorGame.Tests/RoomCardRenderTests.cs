using Bunit;
using Xunit;
using SharedModels;
using BlazorGame.Client.Pages.Components; // Namespace du composant

public class RoomCardRenderTests
{
    [Fact]
    public void RoomCard_Renders_Title_And_Buttons()
    {
        using var ctx = new TestContext();
        var room = new Room { Title = "Salle Test", Type = RoomType.Treasure, Description = "Coffre mystérieux" };

        var cut = ctx.RenderComponent<RoomCard>(parameters => parameters
            .Add(p => p.Room, room)
        );

        var html = cut.Markup;

        Assert.Contains("Salle Test", html);
        Assert.Contains("Combattre", html);
        Assert.Contains("Fuir", html);
        Assert.Contains("Fouiller", html);
    }
}
