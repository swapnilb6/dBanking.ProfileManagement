using FluentValidation;
using dBanking.ProfileManagement.Core.DTOs;
using static dBanking.ProfileManagement.Core.Validators.CommonValidators;

namespace dBanking.ProfileManagement.Core.Validators.Contacts
{
    public class VerifyPhoneRequestValidator : AbstractValidator<VerifyPhoneRequestDto>
    {
        public VerifyPhoneRequestValidator()
        {
            RuleFor(x => x.CustomerId).MustBeNonEmptyGuid();
            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("OTP code is required.")
                .Length(4, 10).WithMessage("OTP length must be between 4 and 10.");
            RuleFor(x => x.CorrelationId).OptionalCorrelationId();
        }
    }
}
