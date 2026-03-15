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

        group.MapPatch("/{id:guid}/resolve", (Guid id, StandupStore store) =>
        {
            var updated = store.ResolveBlocker(id);
            return updated is not null
                ? Results.Ok(updated)
                : Results.NotFound();
        });

        return app;
    }
}
