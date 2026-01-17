namespace dBanking.ProfileManagement.Core.Events
{
    using System;
    public sealed record ProfileEmailChangeRequested(
        Guid CustomerId,
        string NewEmailMasked,
        string CorrelationId,
        DateTimeOffset RequestedAt);

    public sealed record ProfileEmailVerified(
        Guid CustomerId,
        string EmailMasked,
        string CorrelationId,
        DateTimeOffset VerifiedAt);

    public sealed record ProfilePhoneVerified(
        Guid CustomerId,
        string PhoneMasked,
        string CorrelationId,
        DateTimeOffset VerifiedAt);

    public sealed record ProfileAddressUpdated(
        Guid CustomerId,
        Guid AddressId,
        string AddressType,
        string CorrelationId,
        DateTimeOffset EffectiveFrom);

    public sealed record ProfilePreferencesUpdated(
        Guid CustomerId,
        bool? SmsEnabled,
        bool? EmailEnabled,
        bool? PushEnabled,
        bool? RegulatoryConsentGiven,
        string? Language,
        string? TimeZone,
        string CorrelationId,
        DateTimeOffset UpdatedAt);
}
