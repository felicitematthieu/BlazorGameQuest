using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ASTA.SharedModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class ApiEndpointsTests
{
    [Fact]
    public async Task GameApi_Players_List_Returns_SeededOrEmpty()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseSetting("UseInMemory", "true"));
        using var client = factory.CreateClient();
        var resp = await client.GetAsync("/api/players?page=1&pageSize=10");
        Assert.True(resp.IsSuccessStatusCode, "Expected success status code");
        var payload = await resp.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(payload);
    }

    [Fact]
    public async Task WorldApi_Dungeons_List_Returns_Data()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseSetting("UseInMemory", "true"));
        using var client = factory.CreateClient();
        var resp = await client.GetAsync("/api/dungeons");
        Assert.True(resp.IsSuccessStatusCode, "Expected success status code");
        var dungeons = await resp.Content.ReadFromJsonAsync<Dungeon[]>();
        Assert.NotNull(dungeons);
    }
}
