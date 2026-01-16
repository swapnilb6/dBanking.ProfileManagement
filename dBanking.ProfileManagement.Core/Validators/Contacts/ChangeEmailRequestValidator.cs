using dBanking.ProfileManagement.Core.DTOs;
using FluentValidation;
using static dBanking.ProfileManagement.Core.Validators.CommonValidators;

namespace dBanking.ProfileManagement.Core.Validators.Contacts
{
    public class ChangeEmailRequestValidator : AbstractValidator<ChangeEmailRequestDto>
    {
        public ChangeEmailRequestValidator()
        {
            RuleFor(x => x.CustomerId).MustBeNonEmptyGuid();
            RuleFor(x => x.NewEmail).MustBeValidEmail();
            RuleFor(x => x.SourceChannel).NotEmpty().MaximumLength(20);
            RuleFor(x => x.CorrelationId).OptionalCorrelationId();
            RuleFor(x => x.Reason).OptionalReason();
        }
    }
}
