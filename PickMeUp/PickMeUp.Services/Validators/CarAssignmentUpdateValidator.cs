using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class CarAssignmentUpdateValidator : AbstractValidator<CarAssignmentUpdateRequest>
    {
        public CarAssignmentUpdateValidator()
        {
            RuleFor(x => x.CarId!.Value).GreaterThan(0)
                .When(x => x.CarId.HasValue)
                .WithMessage("Select a vehicle from the list.");

            RuleFor(x => x.DriverId!.Value).GreaterThan(0)
                .When(x => x.DriverId.HasValue)
                .WithMessage("Select a driver from the list.");

            RuleFor(x => x.AssignedTo!.Value).GreaterThan(x => x.AssignedFrom!.Value)
                .When(x => x.AssignedTo.HasValue && x.AssignedFrom.HasValue)
                .WithMessage("End of the assignment must be later than its start.");
        }
    }
}
