using Microsoft.AspNetCore.Http;

namespace dBanking.ProfileManagement.API.Filters
{
    public sealed record IdempotencyEntry(
        int StatusCode,
        string ContentType,
        string Body,
        DateTimeOffset CreatedAt
    );

    public interface IIdempotencyStore
    {
        Task<IdempotencyEntry?> GetAsync(string actorId, string key, CancellationToken ct);
        Task SetAsync(string actorId, string key, IdempotencyEntry entry, TimeSpan ttl, CancellationToken ct);
    }
}
