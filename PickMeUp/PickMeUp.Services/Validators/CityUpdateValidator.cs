using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class CityUpdateValidator : AbstractValidator<CityUpdateRequest>
    {
        public CityUpdateValidator()
        {
            RuleFor(x => x.Name).NotEmpty()
                .When(x => x.Name != null)
                .WithMessage("Name cannot be empty.")
                .MaximumLength(100)
                .WithMessage("Name must be at most 100 characters.");

            RuleFor(x => x.PostalCode).MaximumLength(20)
                .When(x => x.PostalCode != null)
                .WithMessage("PostalCode must be at most 20 characters.");
        }
    }
}
