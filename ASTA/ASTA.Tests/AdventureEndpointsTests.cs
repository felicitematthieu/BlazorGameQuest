using System.Net.Http.Json;
using System.Threading.Tasks;
using ASTA.GameApi;
using ASTA.SharedModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

public class AdventureEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AdventureEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("UseInMemory", "true");
            builder.ConfigureServices(services =>
            {
                // Ensure fresh InMemory database for each test
                var dbName = $"adventure-tests-{Guid.NewGuid()}";
                var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<AstaDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                
                services.AddDbContext<AstaDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));
            });
        });
    }

    [Fact]
    public async Task POST_Adventures_StartsNewAdventure()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - playerId null est supporté
        var response = await client.PostAsync("/api/adventures", null);

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AdventureStartDto>();
        
        Assert.NotNull(result);
        Assert.True(result.AdventureId > 0);
        Assert.InRange(result.TotalRooms, 2, 5);
        Assert.NotNull(result.CurrentRoom);
        Assert.NotEmpty(result.CurrentRoom.RoomTitle);
        Assert.Contains(result.CurrentRoom.RoomType, new[] { "Enemy", "Treasure", "Trap" });
    }

    [Fact]
    public async Task POST_Choices_AdvancesAdventure()
    {
        // Arrange
        var client = _factory.CreateClient();
        var startResponse = await client.PostAsync("/api/adventures", null);
        var adventure = await startResponse.Content.ReadFromJsonAsync<AdventureStartDto>();
        Assert.NotNull(adventure);

        // Act
        var choiceResponse = await client.PostAsJsonAsync(
            $"/api/adventures/{adventure.AdventureId}/choices",
            new { Choice = "Combattre" }
        );

        // Assert
        choiceResponse.EnsureSuccessStatusCode();
        var result = await choiceResponse.Content.ReadFromJsonAsync<ChoiceResultDto>();
        
        Assert.NotNull(result);
        Assert.True(result.RoomIndex >= 0);
        
        // Score should change (can be positive or negative depending on random outcome)
        if (!result.IsDead)
        {
            Assert.True(result.NewScore != 0 || result.IsComplete);
        }
    }

    [Fact]
    public async Task GET_Adventure_ReturnsCompleteAdventure()
    {
        // Arrange
        var client = _factory.CreateClient();
        var startResponse = await client.PostAsync("/api/adventures", null);
        var adventure = await startResponse.Content.ReadFromJsonAsync<AdventureStartDto>();
        Assert.NotNull(adventure);

        // Act
        var response = await client.GetAsync($"/api/adventures/{adventure.AdventureId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Adventure>();
        
        Assert.NotNull(result);
        Assert.Equal(adventure.AdventureId, result.Id);
        // PlayerId peut être null (généré sans playerId) ou 1 (seed)
        Assert.True(result.PlayerId == null || result.PlayerId == 1);
        Assert.Equal("InProgress", result.Status);
        Assert.NotEmpty(result.Rooms);
    }

    [Fact]
    public async Task GET_Adventures_ByPlayer_ReturnsPlayerAdventures()
    {
        // Arrange
        var client = _factory.CreateClient();
        // Test avec playerId non existant (doit renvoyer liste vide)
        var response = await client.GetAsync("/api/adventures/player/999");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Adventure[]>();
        
        Assert.NotNull(result);
        // Liste vide attendue car playerId=999 n'existe pas
        Assert.Empty(result);
    }

    // DTOs matching API responses
    private class AdventureStartDto
    {
        public int AdventureId { get; set; }
        public int TotalRooms { get; set; }
        public RoomDto CurrentRoom { get; set; } = new();
    }

    private class RoomDto
    {
        public string RoomTitle { get; set; } = "";
        public string RoomType { get; set; } = "";
        public string Description { get; set; } = "";
    }

    private class ChoiceResultDto
    {
        public int NewScore { get; set; }
        public int RoomIndex { get; set; }
        public bool IsComplete { get; set; }
        public bool IsDead { get; set; }
        public RoomDto? NextRoom { get; set; }
    }
}
