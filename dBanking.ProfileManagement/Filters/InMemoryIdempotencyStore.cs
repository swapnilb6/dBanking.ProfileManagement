using System.Collections.Concurrent;

namespace dBanking.ProfileManagement.API.Filters
{
    public sealed class InMemoryIdempotencyStore : IIdempotencyStore
    {
        private readonly ConcurrentDictionary<string, (IdempotencyEntry Entry, DateTimeOffset Expires)> _store = new();

        private static string K(string actorId, string key) => $"{actorId}:{key}";

        public Task<IdempotencyEntry?> GetAsync(string actorId, string key, CancellationToken ct)
        {
            var id = K(actorId, key);
            if (_store.TryGetValue(id, out var v))
            {
                if (v.Expires > DateTimeOffset.UtcNow) return Task.FromResult<IdempotencyEntry?>(v.Entry);
                _store.TryRemove(id, out _);
            }
            return Task.FromResult<IdempotencyEntry?>(null);
        }

        public Task SetAsync(string actorId, string key, IdempotencyEntry entry, TimeSpan ttl, CancellationToken ct)
        {
            _store[K(actorId, key)] = (entry, DateTimeOffset.UtcNow.Add(ttl));
            return Task.CompletedTask;
        }
    }
}

