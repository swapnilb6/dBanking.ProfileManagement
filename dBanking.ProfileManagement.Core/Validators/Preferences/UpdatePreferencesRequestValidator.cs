
// dBanking.ProfileManagement.Core/Validators/Preferences/UpdatePreferencesRequestValidator.cs
using FluentValidation;
using dBanking.ProfileManagement.Core.DTOs;
using static dBanking.ProfileManagement.Core.Validators.CommonValidators;

namespace dBanking.ProfileManagement.Core.Validators.Preferences
{
    public class UpdatePreferencesRequestValidator : AbstractValidator<UpdatePreferencesRequestDto>
    {
        public UpdatePreferencesRequestValidator()
        {
            RuleFor(x => x.CustomerId).MustBeNonEmptyGuid();

            // All preference booleans are optional (PATCH-like). If none supplied, it's a no-op—service can decide.
            When(x => x.Language is not null, () =>
            {
                RuleFor(x => x.Language!).NotEmpty().MaximumLength(10);
            });

            When(x => x.TimeZone is not null, () =>
            {
                RuleFor(x => x.TimeZone!).NotEmpty().OptionalTimeZone();
            });

            RuleFor(x => x.SourceChannel).NotEmpty().MaximumLength(20);
            RuleFor(x => x.CorrelationId).OptionalCorrelationId();
            RuleFor(x => x.Reason).OptionalReason();
        }
    }
}
