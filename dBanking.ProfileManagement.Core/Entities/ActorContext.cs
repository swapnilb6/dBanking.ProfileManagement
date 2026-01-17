
namespace dBanking.ProfileManagement.Core.Entities
{
    using dBanking.ProfileManagement.Core.Entities;

    public sealed record ActorContext(
        string ActorId,
        ActorRole ActorRole,
        SourceChannel SourceChannel,
        string? IpAddress = null,
        string? UserAgent = null,
        string? CorrelationId = null,
        string? Reason = null);

}
