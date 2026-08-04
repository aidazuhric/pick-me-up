using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class DriverUpdateValidator : AbstractValidator<DriverUpdateRequest>
    {
        public DriverUpdateValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty()
                .When(x => x.FirstName != null)
                .WithMessage("First name cannot be empty.")
                .MaximumLength(50)
                .WithMessage("First name must be at most 50 characters.");

            RuleFor(x => x.LastName).NotEmpty()
                .When(x => x.LastName != null)
                .WithMessage("Last name cannot be empty.")
                .MaximumLength(50)
                .WithMessage("Last name must be at most 50 characters.");

            RuleFor(x => x.PhoneNumber).Matches(ValidationPatterns.Phone)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage(ValidationPatterns.PhoneMessage);

            RuleFor(x => x.HireDate!.Value).LessThanOrEqualTo(_ => DateTime.UtcNow.AddDays(1))
                .When(x => x.HireDate.HasValue)
                .WithMessage("Hire date cannot be in the future.");
        }
    }
}
