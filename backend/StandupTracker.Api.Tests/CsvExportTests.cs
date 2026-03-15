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
    public async Task ExportCsv_ReturnsHeaderOnly_WhenNoEntries()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/standups/export");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Single(lines);
        Assert.Equal("Id,Date,Yesterday,Today,Blockers,Blocker Resolved", lines[0].TrimEnd('\r'));
    }

    [Fact]
    public async Task ExportCsv_ReturnsCorrectContentType()
    {
        var client = new WebApplicationFactory<Program>().CreateClient();

        var response = await client.GetAsync("/api/standups/export");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task ExportCsv_ReturnsAllEntries()
    {
        var client = new WebApplicationFactory<Program>().CreateClient();

        await client.PostAsJsonAsync("/api/standups",
            new { yesterday = "Did code review", today = "Write docs", blockers = "None" });
        await client.PostAsJsonAsync("/api/standups",
            new { yesterday = "Wrote docs", today = "Deploy", blockers = "CI broken" });

        var response = await client.GetAsync("/api/standups/export");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        // Header + 2 data rows
        Assert.Equal(3, lines.Length);
        Assert.Contains("Did code review", content);
        Assert.Contains("Wrote docs", content);
    }

    [Fact]
    public async Task ExportCsv_HandlesCommasInFields()
    {
        var client = new WebApplicationFactory<Program>().CreateClient();

        await client.PostAsJsonAsync("/api/standups",
            new { yesterday = "Fixed bugs, refactored code", today = "Write tests", blockers = (string?)null });

        var response = await client.GetAsync("/api/standups/export");

        var content = await response.Content.ReadAsStringAsync();
        // The yesterday field contains a comma, so it should be wrapped in double quotes
        Assert.Contains("\"Fixed bugs, refactored code\"", content);
    }

    [Fact]
    public async Task ExportCsv_HandlesNullBlockers()
    {
        var client = new WebApplicationFactory<Program>().CreateClient();

        await client.PostAsJsonAsync("/api/standups",
            new { yesterday = "Worked on feature", today = "Continue feature" });

        var response = await client.GetAsync("/api/standups/export");

        var content = await response.Content.ReadAsStringAsync();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        // Find the data row (not the header)
        var dataLine = lines[^1].TrimEnd('\r');
        // Null blockers should result in an empty field: ...,, (two consecutive commas before "No")
        Assert.Contains(",No", dataLine);
        // Verify the blockers field is empty by checking the pattern: ,<empty>,No
        var parts = dataLine.Split(',');
        // Blockers is the 5th column (index 4)
        Assert.Equal("", parts[4]);
    }
}
