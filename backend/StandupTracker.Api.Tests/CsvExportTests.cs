using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace StandupTracker.Api.Tests;

public class CsvExportTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CsvExportTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Export_ReturnsCsvContentType()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/standups/export");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task Export_ReturnsHeaderRow_WhenNoEntries()
    {
        var client = new WebApplicationFactory<Program>().CreateClient();

        var response = await client.GetAsync("/api/standups/export");
        var csv = await response.Content.ReadAsStringAsync();

        var lines = csv.TrimEnd().Split('\n');
        Assert.Single(lines);
        Assert.Equal("Id,Yesterday,Today,Blockers,BlockerResolved,CreatedAt", lines[0].TrimEnd('\r'));
    }

    [Fact]
    public async Task Export_ReturnsAllEntries_AsCsv()
    {
        var client = new WebApplicationFactory<Program>().CreateClient();

        await client.PostAsJsonAsync("/api/standups",
            new { yesterday = "Did work", today = "More work", blockers = "None" });
        await client.PostAsJsonAsync("/api/standups",
            new { yesterday = "Fixed bugs", today = "Write tests", blockers = (string?)null });

        var response = await client.GetAsync("/api/standups/export");
        var csv = await response.Content.ReadAsStringAsync();

        var lines = csv.TrimEnd().Split('\n');
        Assert.Equal(3, lines.Length);
        Assert.StartsWith("Id,Yesterday,Today,Blockers,BlockerResolved,CreatedAt", lines[0]);
    }

    [Fact]
    public async Task Export_EscapesCommasInFields()
    {
        var client = new WebApplicationFactory<Program>().CreateClient();

        await client.PostAsJsonAsync("/api/standups",
            new { yesterday = "Did A, B, and C", today = "Plan D", blockers = (string?)null });

        var response = await client.GetAsync("/api/standups/export");
        var csv = await response.Content.ReadAsStringAsync();

        var lines = csv.TrimEnd().Split('\n');
        Assert.Equal(2, lines.Length);
        // The yesterday field with commas should be wrapped in double quotes
        Assert.Contains("\"Did A, B, and C\"", lines[1]);
    }

    [Fact]
    public async Task Export_ContentDispositionIncludesDate()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/standups/export");

        var disposition = response.Content.Headers.ContentDisposition;
        Assert.NotNull(disposition);
        Assert.Equal("attachment", disposition.DispositionType);

        var filename = disposition.FileNameStar ?? disposition.FileName;
        Assert.NotNull(filename);
        // Filename should match pattern standups-yyyy-MM-dd.csv
        var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
        Assert.Contains($"standups-{date}.csv", filename);
    }
}
