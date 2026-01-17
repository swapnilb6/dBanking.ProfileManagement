namespace dBanking.ProfileManagement.Core.ServiceContracts
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IVerificationService
    {
        // Generates and persists verification (email link token or phone OTP)
        Task<(Guid VerificationId, string RawTokenOrOtp)> CreateAsync(
            Guid customerId, string channelValue, string type, string? correlationId, int ttlMinutes, CancellationToken ct);

        // Validates token/OTP, updates token status, returns success
        Task<bool> VerifyAsync(Guid customerId, Guid verificationId, string presentedSecret, CancellationToken ct);
    }
}