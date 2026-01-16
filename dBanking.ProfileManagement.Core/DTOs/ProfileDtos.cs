
namespace dBanking.ProfileManagement.Core.DTOs
{
    using System;

    // ----------------------------
    // Contacts
    // ----------------------------

    public record ContactViewDto
    {
        public string? Email { get; init; }
        public string EmailStatus { get; init; } = "Unknown";
        public string? PhoneE164 { get; init; }
        public string PhoneStatus { get; init; } = "Unknown";

        public string? PendingEmail { get; init; }
        public string? PendingPhoneE164 { get; init; }
    }

    public record ChangeEmailRequestDto
    {
        public Guid CustomerId { get; init; }
        public string NewEmail { get; init; } = string.Empty;
        public string? CorrelationId { get; init; }
        public string SourceChannel { get; init; } = "Web"; // MobileApp|Web|CSA|System
        public string? Reason { get; init; }
    }

    public record VerifyEmailRequestDto
    {
        public Guid CustomerId { get; init; }
        public string VerificationToken { get; init; } = string.Empty; // raw token from link
        public string? CorrelationId { get; init; }
    }

    public record ChangePhoneRequestDto
    {
        public Guid CustomerId { get; init; }
        public string NewPhoneE164 { get; init; } = string.Empty; // +91...
        public string? CorrelationId { get; init; }
        public string SourceChannel { get; init; } = "Web";
        public string? Reason { get; init; }
    }

    public record VerifyPhoneRequestDto
    {
        public Guid CustomerId { get; init; }
        public string OtpCode { get; init; } = string.Empty;
        public string? CorrelationId { get; init; }
    }

    public record ContactChangeResultDto
    {
        public bool Success { get; init; }
        public string Status { get; init; } = "PendingVerification"; // or Verified/Rejected
        public string Message { get; init; } = string.Empty;
    }

    // ----------------------------
    // Addresses
    // ----------------------------

    public record AddressDto
    {
        public Guid AddressId { get; init; }
        public string AddressType { get; init; } = "Residential";
        public string Line1 { get; init; } = string.Empty;
        public string? Line2 { get; init; }
        public string? Line3 { get; init; }
        public string City { get; init; } = string.Empty;
        public string StateProvince { get; init; } = string.Empty;
        public string PostalCode { get; init; } = string.Empty;
        public string CountryCode { get; init; } = "IN";
        public bool IsPrimary { get; init; } = true;
        public DateTimeOffset EffectiveFrom { get; init; }
        public DateTimeOffset? EffectiveTo { get; init; }
    }

    public record UpdateAddressRequestDto
    {
        public Guid CustomerId { get; init; }
        public Guid AddressId { get; init; }
        public string AddressType { get; init; } = "Residential";
        public string Line1 { get; init; } = string.Empty;
        public string? Line2 { get; init; }
        public string? Line3 { get; init; }
        public string City { get; init; } = string.Empty;
        public string StateProvince { get; init; } = string.Empty;
        public string PostalCode { get; init; } = string.Empty;
        public string CountryCode { get; init; } = "IN";
        public bool IsPrimary { get; init; } = true;
        public DateTimeOffset EffectiveFrom { get; init; }
        public DateTimeOffset? EffectiveTo { get; init; }
        public string SourceChannel { get; init; } = "Web";
        public string? CorrelationId { get; init; }
        public string? Reason { get; init; }
    }

    // Optional convenience for upsert without AddressId
    public record UpsertAddressRequestDto
    {
        public Guid CustomerId { get; init; }
        public string AddressType { get; init; } = "Residential";
        public string Line1 { get; init; } = string.Empty;
        public string? Line2 { get; init; }
        public string? Line3 { get; init; }
        public string City { get; init; } = string.Empty;
        public string StateProvince { get; init; } = string.Empty;
        public string PostalCode { get; init; } = string.Empty;
        public string CountryCode { get; init; } = "IN";
        public bool IsPrimary { get; init; } = true;
        public DateTimeOffset EffectiveFrom { get; init; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? EffectiveTo { get; init; }
        public string SourceChannel { get; init; } = "Web";
        public string? CorrelationId { get; init; }
        public string? Reason { get; init; }
    }

    // ----------------------------
    // Preferences
    // ----------------------------

    public record PreferencesDto
    {
        public bool SmsEnabled { get; init; }
        public bool EmailEnabled { get; init; }
        public bool PushEnabled { get; init; }
        public bool RegulatoryConsentGiven { get; init; }
        public string? Language { get; init; }
        public string? TimeZone { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
    }

    public record UpdatePreferencesRequestDto
    {
        public Guid CustomerId { get; init; }
        public bool? SmsEnabled { get; init; }
        public bool? EmailEnabled { get; init; }
        public bool? PushEnabled { get; init; }
        public bool? RegulatoryConsentGiven { get; init; }
        public string? Language { get; init; }
        public string? TimeZone { get; init; }

        public string SourceChannel { get; init; } = "Web";
        public string? CorrelationId { get; init; }
        public string? Reason { get; init; }
    }

    // ----------------------------
    // Audit (read model)
    // ----------------------------

    public record AuditEntryDto
    {
        public Guid AuditId { get; init; }
        public Guid CustomerId { get; init; }
        public string Entity { get; init; } = string.Empty;
        public string EntityId { get; init; } = string.Empty;
        public string Operation { get; init; } = string.Empty;
        public string OldValueJson { get; init; } = "{}";
        public string NewValueJson { get; init; } = "{}";
        public string ChangedFieldsCsv { get; init; } = string.Empty;
        public string ActorId { get; init; } = string.Empty;
        public string ActorRole { get; init; } = "Customer";
        public string SourceChannel { get; init; } = "Web";
        public string? ReasonCode { get; init; }
        public string? CorrelationId { get; init; }
        public DateTimeOffset Timestamp { get; init; }
    }

    // ----------------------------
    // Generic responses
    // ----------------------------

    public record OperationResultDto
    {
        public bool Success { get; init; }
        public string Code { get; init; } = "OK";       // e.g., OK, PENDING_VERIFICATION, VALIDATION_ERROR
        public string Message { get; init; } = string.Empty;
    }
}
