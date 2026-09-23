using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class DriverInsertValidator : AbstractValidator<DriverInsertRequest>
    {
        public DriverInsertValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty()
                .WithMessage("First name is required.")
                .MaximumLength(50)
                .WithMessage("First name must be at most 50 characters.");

            RuleFor(x => x.LastName).NotEmpty()
                .WithMessage("Last name is required.")
                .MaximumLength(50)
                .WithMessage("Last name must be at most 50 characters.");

            RuleFor(x => x.Email).NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Enter a valid email address, for example name@company.com.")
                .MaximumLength(100)
                .WithMessage("Email must be at most 100 characters.");

            RuleFor(x => x.PhoneNumber).Matches(ValidationPatterns.Phone)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage(ValidationPatterns.PhoneMessage);

            RuleFor(x => x.HireDate).NotEmpty()
                .WithMessage("Hire date is required.")
                .LessThanOrEqualTo(_ => DateTime.UtcNow.AddDays(1))
                .WithMessage("Hire date cannot be in the future.");

            RuleFor(x => x.LoginUsername).NotEmpty()
                .WithMessage("Login username for the driver account is required.")
                .MaximumLength(100)
                .WithMessage("Login username must be at most 100 characters.");

            RuleFor(x => x.LoginPassword).NotEmpty()
                .WithMessage("Set an initial password for the driver account.")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters.")
                .MaximumLength(100)
                .WithMessage("Password must be at most 100 characters.");
        }
    }
}
