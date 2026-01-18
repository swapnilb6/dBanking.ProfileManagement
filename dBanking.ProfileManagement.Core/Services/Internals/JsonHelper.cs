// dBanking.ProfileManagement.Core/Services/Internals/JsonHelper.cs
using System.Text.Json;

namespace dBanking.ProfileManagement.Core.Services.Internals
{
    internal static class JsonHelper
    {
        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        public static string ToJson(object? value)
            => JsonSerializer.Serialize(value ?? new { }, _opts);
    }
}