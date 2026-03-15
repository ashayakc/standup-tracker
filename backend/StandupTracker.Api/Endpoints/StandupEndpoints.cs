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

        group.MapGet("/export", (StandupStore store) =>
        {
            var entries = store.GetAll();
            var sb = new StringBuilder();
            sb.AppendLine("Id,Yesterday,Today,Blockers,BlockerResolved,CreatedAt");

            foreach (var entry in entries)
            {
                sb.Append(entry.Id);
                sb.Append(',');
                sb.Append(EscapeCsvField(entry.Yesterday));
                sb.Append(',');
                sb.Append(EscapeCsvField(entry.Today));
                sb.Append(',');
                sb.Append(EscapeCsvField(entry.Blockers ?? ""));
                sb.Append(',');
                sb.Append(entry.BlockerResolved.ToString().ToLower());
                sb.Append(',');
                sb.AppendLine(entry.CreatedAt.ToString("o"));
            }

            var csv = sb.ToString();
            var date = DateTime.UtcNow.ToString("yyyy-MM-dd");

            return Results.File(
                Encoding.UTF8.GetBytes(csv),
                contentType: "text/csv",
                fileDownloadName: $"standups-{date}.csv");
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

    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
