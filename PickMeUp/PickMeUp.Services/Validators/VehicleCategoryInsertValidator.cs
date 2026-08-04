using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class VehicleCategoryInsertValidator : AbstractValidator<VehicleCategoryInsertRequest>
    {
        public VehicleCategoryInsertValidator()
        {
            RuleFor(x => x.Name).NotEmpty()
                .WithMessage("Name is required.")
                .MaximumLength(100)
                .WithMessage("Name must be at most 100 characters.");

            RuleFor(x => x.Description).MaximumLength(300)
                .WithMessage("Description must be at most 300 characters.");
        }
    }
}
