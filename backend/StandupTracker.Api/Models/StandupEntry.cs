namespace StandupTracker.Api.Models;

public record StandupEntry(
    Guid Id,
    string Yesterday,
    string Today,
    string? Blockers,
    bool BlockerResolved,
    DateTime CreatedAt);
