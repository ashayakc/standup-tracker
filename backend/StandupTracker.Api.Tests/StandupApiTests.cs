using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace StandupTracker.Api.Tests;

public class StandupApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public StandupApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetStandups_ReturnsEmptyList_Initially()
    {
        var response = await _client.GetAsync("/api/standups");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var entries = await response.Content.ReadFromJsonAsync<List<StandupEntryDto>>();
        Assert.NotNull(entries);
        Assert.Empty(entries);
    }

    [Fact]
    public async Task PostStandup_CreatesEntry_Returns201()
    {
        var request = new { yesterday = "Fixed bugs", today = "Write tests", blockers = "Waiting on API keys" };

        var response = await _client.PostAsJsonAsync("/api/standups", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var entry = await response.Content.ReadFromJsonAsync<StandupEntryDto>();
        Assert.NotNull(entry);
        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal("Fixed bugs", entry.Yesterday);
        Assert.Equal("Write tests", entry.Today);
        Assert.Equal("Waiting on API keys", entry.Blockers);
        Assert.False(entry.BlockerResolved);
    }

    [Fact]
    public async Task PostStandup_MissingFields_Returns400()
    {
        var request = new { yesterday = "", today = "Write tests", blockers = (string?)null };

        var response = await _client.PostAsJsonAsync("/api/standups", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetStandups_ReturnsNewestFirst()
    {
        var client = new WebApplicationFactory<Program>().CreateClient();

        await client.PostAsJsonAsync("/api/standups", new { yesterday = "First", today = "First today" });
        await client.PostAsJsonAsync("/api/standups", new { yesterday = "Second", today = "Second today" });

        var entries = await client.GetFromJsonAsync<List<StandupEntryDto>>("/api/standups");

        Assert.NotNull(entries);
        Assert.True(entries.Count >= 2);
        Assert.Equal("Second", entries[0].Yesterday);
        Assert.Equal("First", entries[1].Yesterday);
    }

    [Fact]
    public async Task PatchResolve_ResolvesBlocker_Returns200()
    {
        var client = new WebApplicationFactory<Program>().CreateClient();

        var postResponse = await client.PostAsJsonAsync("/api/standups",
            new { yesterday = "Work", today = "More work", blockers = "Blocked!" });
        var created = await postResponse.Content.ReadFromJsonAsync<StandupEntryDto>();

        var patchResponse = await client.PatchAsync($"/api/standups/{created!.Id}/resolve", null);

        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
        var updated = await patchResponse.Content.ReadFromJsonAsync<StandupEntryDto>();
        Assert.NotNull(updated);
        Assert.True(updated.BlockerResolved);
    }

    [Fact]
    public async Task PatchResolve_NonExistentId_Returns404()
    {
        var response = await _client.PatchAsync($"/api/standups/{Guid.NewGuid()}/resolve", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private record StandupEntryDto(
        Guid Id,
        string Yesterday,
        string Today,
        string? Blockers,
        bool BlockerResolved,
        DateTime CreatedAt);
}
