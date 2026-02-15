
namespace dBanking.ProfileManagement.Core.Entities
{
    using dBanking.ProfileManagement.Core.Entities;
    public sealed record ActorContext(

        string ActorId,                  // user or app/client-id (goes to audit.actor)
        ActorRole ActorRole,             // Customer|Employee|Service (not persisted; can be projected into actor format if needed)
        SourceChannel SourceChannel,     // Canonical channel (goes to audit.channel)
        string? IpAddress = null,
        string? UserAgent = null,
        Guid? CorrelationId = null,      // use Guid? (maps to audit.correlation_id)
        string? Reason = null            // service-level; not persisted unless you add a column
    );
}
