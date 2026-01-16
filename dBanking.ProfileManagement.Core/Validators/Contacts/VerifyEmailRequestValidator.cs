using FluentValidation;
using dBanking.ProfileManagement.Core.DTOs;
using static dBanking.ProfileManagement.Core.Validators.CommonValidators;

namespace dBanking.ProfileManagement.Core.Validators.Contacts
{
    public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequestDto>
    {
        public VerifyEmailRequestValidator()
        {
            RuleFor(x => x.CustomerId).MustBeNonEmptyGuid();
            RuleFor(x => x.VerificationToken)
                .NotEmpty().WithMessage("Verification token is required.")
                .MaximumLength(256);
            RuleFor(x => x.CorrelationId).OptionalCorrelationId();
        }
    }
}
