using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using StandupTracker.Api.Models;
using StandupTracker.Api.Services;

namespace StandupTracker.Api.Tests;

public class WeeklySummaryTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public WeeklySummaryTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private (HttpClient client, StandupStore store) CreateClientWithStore()
    {
        StandupStore? capturedStore = null;
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Build a temporary provider to grab the singleton store
            });
        });

        var client = factory.CreateClient();
        capturedStore = factory.Services.GetRequiredService<StandupStore>();
        return (client, capturedStore);
    }

    [Fact]
    public async Task WeeklySummary_ReturnsEmptyArray_WhenNoEntries()
    {
        var (client, _) = CreateClientWithStore();

        var response = await client.GetAsync("/api/standups/weekly-summary");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var summaries = await response.Content.ReadFromJsonAsync<List<WeeklySummaryDto>>();
        Assert.NotNull(summaries);
        Assert.Empty(summaries);
    }

    [Fact]
    public async Task WeeklySummary_ReturnsSingleWeek_WhenAllEntriesInSameWeek()
    {
        var (client, store) = CreateClientWithStore();

        // Monday and Wednesday of the same week (March 9-13, 2026)
        store.AddWithDate(new CreateStandupRequest("Did A", "Do B", null), new DateTime(2026, 3, 9, 10, 0, 0));
        store.AddWithDate(new CreateStandupRequest("Did C", "Do D", null), new DateTime(2026, 3, 11, 10, 0, 0));

        var summaries = await client.GetFromJsonAsync<List<WeeklySummaryDto>>("/api/standups/weekly-summary");

        Assert.NotNull(summaries);
        Assert.Single(summaries);
        Assert.Equal(2, summaries[0].StandupCount);
    }

    [Fact]
    public async Task WeeklySummary_GroupsByWeek_NewestFirst()
    {
        var (client, store) = CreateClientWithStore();

        // Week 1: March 2, 2026 (Monday)
        store.AddWithDate(new CreateStandupRequest("Week1", "Today1", null), new DateTime(2026, 3, 2, 10, 0, 0));
        // Week 2: March 9, 2026 (Monday)
        store.AddWithDate(new CreateStandupRequest("Week2", "Today2", null), new DateTime(2026, 3, 9, 10, 0, 0));

        var summaries = await client.GetFromJsonAsync<List<WeeklySummaryDto>>("/api/standups/weekly-summary");

        Assert.NotNull(summaries);
        Assert.Equal(2, summaries.Count);
        // Newest week first
        Assert.True(summaries[0].WeekStart > summaries[1].WeekStart);
        Assert.Equal(new DateTime(2026, 3, 9), summaries[0].WeekStart);
        Assert.Equal(new DateTime(2026, 3, 2), summaries[1].WeekStart);
    }

    [Fact]
    public async Task WeeklySummary_CountsBlockersRaised_OnlyForNonNullNonEmpty()
    {
        var (client, store) = CreateClientWithStore();

        // Same week: March 9-13, 2026
        store.AddWithDate(new CreateStandupRequest("A", "B", "Blocked!"), new DateTime(2026, 3, 9, 10, 0, 0));
        store.AddWithDate(new CreateStandupRequest("C", "D", null), new DateTime(2026, 3, 10, 10, 0, 0));
        store.AddWithDate(new CreateStandupRequest("E", "F", ""), new DateTime(2026, 3, 11, 10, 0, 0));
        store.AddWithDate(new CreateStandupRequest("G", "H", "Another blocker"), new DateTime(2026, 3, 12, 10, 0, 0));

        var summaries = await client.GetFromJsonAsync<List<WeeklySummaryDto>>("/api/standups/weekly-summary");

        Assert.NotNull(summaries);
        Assert.Single(summaries);
        Assert.Equal(2, summaries[0].BlockersRaised);
    }

    [Fact]
    public async Task WeeklySummary_CountsBlockersResolved_OnlyForResolvedTrue()
    {
        var (client, store) = CreateClientWithStore();

        // Same week
        var entry1 = store.AddWithDate(new CreateStandupRequest("A", "B", "Blocked"), new DateTime(2026, 3, 9, 10, 0, 0));
        store.AddWithDate(new CreateStandupRequest("C", "D", "Also blocked"), new DateTime(2026, 3, 10, 10, 0, 0));

        // Resolve one blocker
        store.ResolveBlocker(entry1.Id);

        var summaries = await client.GetFromJsonAsync<List<WeeklySummaryDto>>("/api/standups/weekly-summary");

        Assert.NotNull(summaries);
        Assert.Single(summaries);
        Assert.Equal(2, summaries[0].BlockersRaised);
        Assert.Equal(1, summaries[0].BlockersResolved);
    }

    [Fact]
    public async Task WeeklySummary_ResolutionRate_IsZero_WhenNoBlockersRaised()
    {
        var (client, store) = CreateClientWithStore();

        store.AddWithDate(new CreateStandupRequest("A", "B", null), new DateTime(2026, 3, 9, 10, 0, 0));

        var summaries = await client.GetFromJsonAsync<List<WeeklySummaryDto>>("/api/standups/weekly-summary");

        Assert.NotNull(summaries);
        Assert.Single(summaries);
        Assert.Equal(0, summaries[0].BlockersRaised);
        Assert.Equal(0.0, summaries[0].ResolutionRate);
    }

    [Fact]
    public async Task WeeklySummary_ResolutionRate_CalculatedCorrectly()
    {
        var (client, store) = CreateClientWithStore();

        // 3 blockers raised, 2 resolved => 66.67%
        var e1 = store.AddWithDate(new CreateStandupRequest("A", "B", "B1"), new DateTime(2026, 3, 9, 10, 0, 0));
        var e2 = store.AddWithDate(new CreateStandupRequest("C", "D", "B2"), new DateTime(2026, 3, 10, 10, 0, 0));
        store.AddWithDate(new CreateStandupRequest("E", "F", "B3"), new DateTime(2026, 3, 11, 10, 0, 0));

        store.ResolveBlocker(e1.Id);
        store.ResolveBlocker(e2.Id);

        var summaries = await client.GetFromJsonAsync<List<WeeklySummaryDto>>("/api/standups/weekly-summary");

        Assert.NotNull(summaries);
        Assert.Single(summaries);
        Assert.Equal(3, summaries[0].BlockersRaised);
        Assert.Equal(2, summaries[0].BlockersResolved);
        Assert.Equal(66.67, summaries[0].ResolutionRate);
    }

    [Fact]
    public async Task WeeklySummary_EntriesWithinWeek_OrderedByCreatedAtDescending()
    {
        var (client, store) = CreateClientWithStore();

        // Same week, different days
        store.AddWithDate(new CreateStandupRequest("Monday", "B", null), new DateTime(2026, 3, 9, 10, 0, 0));
        store.AddWithDate(new CreateStandupRequest("Wednesday", "D", null), new DateTime(2026, 3, 11, 10, 0, 0));
        store.AddWithDate(new CreateStandupRequest("Tuesday", "C", null), new DateTime(2026, 3, 10, 10, 0, 0));

        var summaries = await client.GetFromJsonAsync<List<WeeklySummaryDto>>("/api/standups/weekly-summary");

        Assert.NotNull(summaries);
        Assert.Single(summaries);
        var entries = summaries[0].Entries;
        Assert.Equal(3, entries.Count);
        // Newest first: Wednesday, Tuesday, Monday
        Assert.Equal("Wednesday", entries[0].Yesterday);
        Assert.Equal("Tuesday", entries[1].Yesterday);
        Assert.Equal("Monday", entries[2].Yesterday);
    }

    private record StandupEntryDto(
        Guid Id,
        string Yesterday,
        string Today,
        string? Blockers,
        bool BlockerResolved,
        DateTime CreatedAt);

    private record WeeklySummaryDto(
        DateTime WeekStart,
        DateTime WeekEnd,
        int StandupCount,
        int BlockersRaised,
        int BlockersResolved,
        double ResolutionRate,
        List<StandupEntryDto> Entries);
}
