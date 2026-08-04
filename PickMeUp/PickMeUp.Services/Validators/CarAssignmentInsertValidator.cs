using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class CarAssignmentInsertValidator : AbstractValidator<CarAssignmentInsertRequest>
    {
        public CarAssignmentInsertValidator()
        {
            RuleFor(x => x.CarId).GreaterThan(0)
                .WithMessage("Select a vehicle from the list.");

            RuleFor(x => x.DriverId).GreaterThan(0)
                .WithMessage("Select a driver from the list.");

            RuleFor(x => x.AssignedFrom).NotEmpty()
                .WithMessage("Start of the assignment is required.");

            RuleFor(x => x.AssignedTo!.Value).GreaterThan(x => x.AssignedFrom)
                .When(x => x.AssignedTo.HasValue)
                .WithMessage("End of the assignment must be later than its start.");
        }
    }
}
