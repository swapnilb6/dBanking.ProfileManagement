
using FluentValidation;
using dBanking.ProfileManagement.Core.DTOs;
using static dBanking.ProfileManagement.Core.Validators.CommonValidators;

namespace dBanking.ProfileManagement.Core.Validators.Contacts
{
    public class ChangePhoneRequestValidator : AbstractValidator<ChangePhoneRequestDto>
    {
        public ChangePhoneRequestValidator()
        {
            RuleFor(x => x.CustomerId).MustBeNonEmptyGuid();
            RuleFor(x => x.NewPhoneE164).MustBeValidPhoneE164();
            RuleFor(x => x.SourceChannel).NotEmpty().MaximumLength(20);
            RuleFor(x => x.CorrelationId).OptionalCorrelationId();
            RuleFor(x => x.Reason).OptionalReason();
        }
    }
}
