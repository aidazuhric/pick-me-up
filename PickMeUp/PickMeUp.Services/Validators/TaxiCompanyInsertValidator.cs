using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class TaxiCompanyInsertValidator : AbstractValidator<TaxiCompanyInsertRequest>
    {
        public TaxiCompanyInsertValidator()
        {
            RuleFor(x => x.Name).NotEmpty()
                .WithMessage("Company name is required.")
                .MaximumLength(150)
                .WithMessage("Company name must be at most 150 characters.");

            RuleFor(x => x.HqAddress).NotEmpty()
                .WithMessage("Headquarters address is required.")
                .MaximumLength(250)
                .WithMessage("Headquarters address must be at most 250 characters.");

            RuleFor(x => x.CityId).GreaterThan(0)
                .WithMessage("Select a city from the list.");

            RuleFor(x => x.ContactEmail).NotEmpty()
                .WithMessage("Contact email is required.")
                .EmailAddress()
                .WithMessage("Enter a valid email address, for example name@company.com.")
                .MaximumLength(100)
                .WithMessage("Contact email must be at most 100 characters.");

            RuleFor(x => x.ContactPhone).NotEmpty()
                .WithMessage("Contact phone is required.")
                .Matches(ValidationPatterns.Phone)
                .WithMessage(ValidationPatterns.PhoneMessage)
                .MaximumLength(30)
                .WithMessage("Contact phone must be at most 30 characters.");

            RuleFor(x => x.OwnerName).NotEmpty()
                .WithMessage("Owner name is required.")
                .MaximumLength(150)
                .WithMessage("Owner name must be at most 150 characters.");

            RuleFor(x => x.StartingPrice).GreaterThanOrEqualTo(0)
                .WithMessage("Starting price cannot be negative.");

            RuleFor(x => x.PricePerKm).GreaterThanOrEqualTo(0)
                .WithMessage("Price per kilometre cannot be negative.");

            RuleFor(x => x.LoginUsername).NotEmpty()
                .WithMessage("Login username for the company account is required.")
                .MaximumLength(100)
                .WithMessage("Login username must be at most 100 characters.");

            RuleFor(x => x.LoginEmail).EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.LoginEmail))
                .WithMessage("Enter a valid email address, for example name@company.com.");

            RuleFor(x => x.LoginPassword).NotEmpty()
                .WithMessage("Set an initial password for the company account.")
                .PasswordStrength();
        }
    }
}
