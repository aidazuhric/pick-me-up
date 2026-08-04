using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class VehicleCategoryUpdateValidator : AbstractValidator<VehicleCategoryUpdateRequest>
    {
        public VehicleCategoryUpdateValidator()
        {
            RuleFor(x => x.Name).NotEmpty()
                .When(x => x.Name != null)
                .WithMessage("Name cannot be empty.")
                .MaximumLength(100)
                .WithMessage("Name must be at most 100 characters.");

            RuleFor(x => x.Description).MaximumLength(300)
                .When(x => x.Description != null)
                .WithMessage("Description must be at most 300 characters.");
        }
    }
}
