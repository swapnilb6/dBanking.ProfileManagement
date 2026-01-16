using FluentValidation;
using System;
using System.Text.RegularExpressions;

namespace dBanking.ProfileManagement.Core.Validators
{
    public static class CommonValidators
    {
        // Simplified E.164: +<1-15 digits>, first digit 1-9 (no leading zero after +)
        private static readonly Regex E164Regex = new(@"^\+[1-9]\d{6,14}$", RegexOptions.Compiled);
        private static readonly Regex IsoCountryRegex = new(@"^[A-Z]{2}$", RegexOptions.Compiled);
        private static readonly Regex TimeZoneGuess = new(@"^[A-Za-z/_+\-0-9]+$", RegexOptions.Compiled);

        public static IRuleBuilderOptions<T, Guid> MustBeNonEmptyGuid<T>(this IRuleBuilder<T, Guid> ruleBuilder) =>
            ruleBuilder.NotEmpty().WithMessage("Id must be a non-empty GUID.");

        public static IRuleBuilderOptions<T, string?> MustBeValidEmail<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
            ruleBuilder
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email format is invalid.")
                .MaximumLength(254);

        public static IRuleBuilderOptions<T, string?> MustBeValidPhoneE164<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
            ruleBuilder
                .NotEmpty().WithMessage("Phone is required.")
                .Matches(E164Regex).WithMessage("Phone must be E.164 formatted (e.g., +911234567890).");

        public static IRuleBuilderOptions<T, string?> MustBeValidCountryCode<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
            ruleBuilder
                .NotEmpty().WithMessage("CountryCode is required.")
                .Matches(IsoCountryRegex).WithMessage("CountryCode must be ISO 3166-1 alpha-2 (e.g., IN).");

        public static IRuleBuilderOptions<T, string?> OptionalCountryCode<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
            ruleBuilder
                .Must(x => x == null || IsoCountryRegex.IsMatch(x))
                .WithMessage("CountryCode must be ISO 3166-1 alpha-2 (e.g., IN).");

        public static IRuleBuilderOptions<T, string?> OptionalCorrelationId<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
            ruleBuilder.MaximumLength(100);

        public static IRuleBuilderOptions<T, string?> OptionalReason<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
            ruleBuilder.MaximumLength(200);

        public static IRuleBuilderOptions<T, string?> OptionalLanguage<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
            ruleBuilder.MaximumLength(10);

        public static IRuleBuilderOptions<T, string?> OptionalTimeZone<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
            ruleBuilder
                .Must(x => x == null || TimeZoneGuess.IsMatch(x))
                .WithMessage("TimeZone value looks invalid.");

        public static IRuleBuilderOptions<T, DateTimeOffset> MustBeRecentOrFuture<T>(this IRuleBuilder<T, DateTimeOffset> ruleBuilder) =>
            ruleBuilder
                .LessThanOrEqualTo(DateTimeOffset.UtcNow.AddYears(10))
                .GreaterThan(DateTimeOffset.UtcNow.AddYears(-50));

        public static IRuleBuilderOptions<T, DateTimeOffset?> OptionalEffectiveToAfterFrom<T>(
            this IRuleBuilder<T, DateTimeOffset?> ruleBuilder, Func<T, DateTimeOffset> fromSelector) =>
            ruleBuilder
                .Must((root, to) => to == null || to > fromSelector(root))
                .WithMessage("EffectiveTo must be greater than EffectiveFrom.");
    }
}


