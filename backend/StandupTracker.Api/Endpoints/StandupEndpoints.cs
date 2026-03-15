using System.Text;
using StandupTracker.Api.Models;
using StandupTracker.Api.Services;

namespace StandupTracker.Api.Endpoints;

public static class StandupEndpoints
{
    public static IEndpointRouteBuilder MapStandupEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/standups").WithTags("Standups");

        group.MapGet("/", (StandupStore store) =>
            TypedResults.Ok(store.GetAll()));

        group.MapPost("/", (CreateStandupRequest request, StandupStore store) =>
        {
            if (string.IsNullOrWhiteSpace(request.Yesterday) || string.IsNullOrWhiteSpace(request.Today))
                return Results.BadRequest("Yesterday and Today are required.");

            var entry = store.Add(request);
            return Results.Created($"/api/standups/{entry.Id}", entry);
        });

        group.MapGet("/weekly-summary", (StandupStore store) =>
            TypedResults.Ok(store.GetWeeklySummary()));

        group.MapGet("/export", (StandupStore store, HttpContext context) =>
        {
            var entries = store.GetAll();
            var sb = new StringBuilder();
            sb.AppendLine("Id,Date,Yesterday,Today,Blockers,Blocker Resolved");

            foreach (var entry in entries)
            {
                sb.AppendLine(string.Join(",",
                    CsvEscape(entry.Id.ToString()),
                    CsvEscape(entry.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")),
                    CsvEscape(entry.Yesterday),
                    CsvEscape(entry.Today),
                    CsvEscape(entry.Blockers),
                    CsvEscape(entry.BlockerResolved ? "Yes" : "No")));
            }

            context.Response.Headers["Content-Disposition"] = "attachment; filename=\"standups.csv\"";
            return Results.Text(sb.ToString(), "text/csv");
        });

        group.MapPatch("/{id:guid}/resolve", (Guid id, StandupStore store) =>
        {
            var updated = store.ResolveBlocker(id);
            return updated is not null
                ? Results.Ok(updated)
                : Results.NotFound();
        });

        return app;
    }

    internal static string CsvEscape(string? value)
    {
        if (value is null)
            return "";

        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
