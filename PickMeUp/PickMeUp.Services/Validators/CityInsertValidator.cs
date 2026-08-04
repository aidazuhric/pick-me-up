using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class CityInsertValidator : AbstractValidator<CityInsertRequest>
    {
        public CityInsertValidator()
        {
            RuleFor(x => x.Name).NotEmpty()
                .WithMessage("Name is required.")
                .MaximumLength(100)
                .WithMessage("Name must be at most 100 characters.");

            RuleFor(x => x.PostalCode).MaximumLength(20)
                .WithMessage("PostalCode must be at most 20 characters.");
        }
    }
}
