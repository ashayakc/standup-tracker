using System.Collections.Concurrent;
using System.Globalization;
using StandupTracker.Api.Models;

namespace StandupTracker.Api.Services;

public class StandupStore
{
    private readonly ConcurrentDictionary<Guid, StandupEntry> _entries = new();

    public IReadOnlyList<StandupEntry> GetAll()
    {
        return _entries.Values
            .OrderByDescending(e => e.CreatedAt)
            .ToList();
    }

    public StandupEntry? GetById(Guid id)
    {
        return _entries.GetValueOrDefault(id);
    }

    public StandupEntry Add(CreateStandupRequest request)
    {
        var entry = new StandupEntry(
            Guid.NewGuid(),
            request.Yesterday,
            request.Today,
            request.Blockers,
            false,
            DateTime.UtcNow);

        _entries[entry.Id] = entry;
        return entry;
    }

    internal StandupEntry AddWithDate(CreateStandupRequest request, DateTime createdAt)
    {
        var entry = new StandupEntry(
            Guid.NewGuid(),
            request.Yesterday,
            request.Today,
            request.Blockers,
            false,
            createdAt);

        _entries[entry.Id] = entry;
        return entry;
    }

    public StandupEntry? ResolveBlocker(Guid id)
    {
        if (!_entries.TryGetValue(id, out var entry))
            return null;

        if (string.IsNullOrWhiteSpace(entry.Blockers))
            return null;

        var updated = entry with { BlockerResolved = true };
        _entries[id] = updated;
        return updated;
    }

    public List<WeeklySummary> GetWeeklySummary()
    {
        var entries = _entries.Values.ToList();

        var grouped = entries
            .GroupBy(e =>
            {
                var year = ISOWeek.GetYear(e.CreatedAt);
                var week = ISOWeek.GetWeekOfYear(e.CreatedAt);
                return (year, week);
            })
            .Select(g =>
            {
                var weekStart = ISOWeek.ToDateTime(g.Key.year, g.Key.week, DayOfWeek.Monday);
                var weekEnd = weekStart.AddDays(6).AddHours(23).AddMinutes(59).AddSeconds(59);
                var weekEntries = g.OrderByDescending(e => e.CreatedAt).ToList();
                var blockersRaised = weekEntries.Count(e => !string.IsNullOrEmpty(e.Blockers));
                var blockersResolved = weekEntries.Count(e => e.BlockerResolved);
                var resolutionRate = blockersRaised > 0
                    ? Math.Round((double)blockersResolved / blockersRaised * 100, 2)
                    : 0;

                return new WeeklySummary(
                    weekStart,
                    weekEnd,
                    weekEntries.Count,
                    blockersRaised,
                    blockersResolved,
                    resolutionRate,
                    weekEntries);
            })
            .OrderByDescending(s => s.WeekStart)
            .ToList();

        return grouped;
    }
}
