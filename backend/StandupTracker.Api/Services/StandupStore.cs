using System.Collections.Concurrent;
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
}
