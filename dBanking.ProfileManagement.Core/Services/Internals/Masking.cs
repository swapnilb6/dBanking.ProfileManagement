
// dBanking.ProfileManagement.Core/Services/Internals/Masking.cs
using System;

namespace dBanking.ProfileManagement.Core.Services.Internals
{
    internal static class Masking
    {
        public static string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return string.Empty;
            var parts = email.Split('@');
            if (parts.Length != 2) return "***";
            var user = parts[0];
            var domain = parts[1];
            var visible = Math.Min(2, user.Length);
            return $"{user.Substring(0, visible)}***@{domain}";
        }

        public static string MaskPhone(string phoneE164)
        {
            if (string.IsNullOrWhiteSpace(phoneE164)) return string.Empty;
            // Keep country code + last 2 digits visible
            if (phoneE164.Length <= 5) return phoneE164;
            var cc = phoneE164.Substring(0, 3); // e.g., +91
            var tail = phoneE164[^2..];
            return $"{cc}****{tail}";
        }
    }
}
