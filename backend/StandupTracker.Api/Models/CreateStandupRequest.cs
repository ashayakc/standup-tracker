namespace StandupTracker.Api.Models;

public record CreateStandupRequest(
    string Yesterday,
    string Today,
    string? Blockers);
