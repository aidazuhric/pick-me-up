using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class TaxiCompanyUpdateValidator : AbstractValidator<TaxiCompanyUpdateRequest>
    {
        public TaxiCompanyUpdateValidator()
        {
            RuleFor(x => x.Name).NotEmpty()
                .When(x => x.Name != null)
                .WithMessage("Company name cannot be empty.")
                .MaximumLength(150)
                .WithMessage("Company name must be at most 150 characters.");

            RuleFor(x => x.HqAddress).NotEmpty()
                .When(x => x.HqAddress != null)
                .WithMessage("Headquarters address cannot be empty.")
                .MaximumLength(250)
                .WithMessage("Headquarters address must be at most 250 characters.");

            RuleFor(x => x.CityId!.Value).GreaterThan(0)
                .When(x => x.CityId.HasValue)
                .WithMessage("Select a city from the list.");

            RuleFor(x => x.ContactEmail).EmailAddress()
                .When(x => x.ContactEmail != null)
                .WithMessage("Enter a valid email address, for example name@company.com.")
                .MaximumLength(100)
                .WithMessage("Contact email must be at most 100 characters.");

            RuleFor(x => x.ContactPhone).Matches(ValidationPatterns.Phone)
                .When(x => x.ContactPhone != null)
                .WithMessage(ValidationPatterns.PhoneMessage)
                .MaximumLength(30)
                .WithMessage("Contact phone must be at most 30 characters.");

            RuleFor(x => x.OwnerName).NotEmpty()
                .When(x => x.OwnerName != null)
                .WithMessage("Owner name cannot be empty.")
                .MaximumLength(150)
                .WithMessage("Owner name must be at most 150 characters.");

            RuleFor(x => x.StartingPrice!.Value).GreaterThanOrEqualTo(0)
                .When(x => x.StartingPrice.HasValue)
                .WithMessage("Starting price cannot be negative.");

            RuleFor(x => x.PricePerKm!.Value).GreaterThanOrEqualTo(0)
                .When(x => x.PricePerKm.HasValue)
                .WithMessage("Price per kilometre cannot be negative.");
        }
    }
}
