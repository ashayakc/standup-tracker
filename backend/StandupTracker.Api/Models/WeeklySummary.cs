namespace StandupTracker.Api.Models;

public record WeeklySummary(
    DateTime WeekStart,
    DateTime WeekEnd,
    int StandupCount,
    int BlockersRaised,
    int BlockersResolved,
    double ResolutionRate,
    IReadOnlyList<StandupEntry> Entries);
