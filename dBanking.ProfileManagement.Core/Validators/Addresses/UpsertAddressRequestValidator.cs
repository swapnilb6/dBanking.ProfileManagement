
// dBanking.ProfileManagement.Core/Validators/Addresses/UpsertAddressRequestValidator.cs
using FluentValidation;
using dBanking.ProfileManagement.Core.DTOs;
using static dBanking.ProfileManagement.Core.Validators.CommonValidators;

namespace dBanking.ProfileManagement.Core.Validators.Addresses
{
    public class UpsertAddressRequestValidator : AbstractValidator<UpsertAddressRequestDto>
    {
        public UpsertAddressRequestValidator()
        {
            RuleFor(x => x.CustomerId).MustBeNonEmptyGuid();

            RuleFor(x => x.AddressType)
                .NotEmpty()
                .Must(v => v.Equals("Residential", true) || v.Equals("Mailing", true) || v.Equals("Work", true))
                .WithMessage("AddressType must be Residential, Mailing, or Work.");

            RuleFor(x => x.Line1).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Line2).MaximumLength(200);
            RuleFor(x => x.Line3).MaximumLength(200);
            RuleFor(x => x.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.StateProvince).NotEmpty().MaximumLength(100);
            RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
            RuleFor(x => x.CountryCode).MustBeValidCountryCode();

            RuleFor(x => x.EffectiveFrom).MustBeRecentOrFuture();
            RuleFor(x => x.EffectiveTo).OptionalEffectiveToAfterFrom(root => root.EffectiveFrom);

            RuleFor(x => x.SourceChannel).NotEmpty().MaximumLength(20);
            RuleFor(x => x.CorrelationId).OptionalCorrelationId();
            RuleFor(x => x.Reason).OptionalReason();
        }
    }
}
