using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class CarInsertValidator : AbstractValidator<CarInsertRequest>
    {
        public CarInsertValidator()
        {
            RuleFor(x => x.Make).NotEmpty()
                .WithMessage("Make is required.")
                .MaximumLength(60)
                .WithMessage("Make must be at most 60 characters.");

            RuleFor(x => x.Model).NotEmpty()
                .WithMessage("Model is required.")
                .MaximumLength(60)
                .WithMessage("Model must be at most 60 characters.");

            RuleFor(x => x.Plate).NotEmpty()
                .WithMessage("Registration plate is required.")
                .MaximumLength(20)
                .WithMessage("Registration plate must be at most 20 characters.");

            RuleFor(x => x.Year)
                .InclusiveBetween(CarLimits.EarliestYear, DateTime.UtcNow.Year + 1)
                .WithMessage($"Production year must be between {CarLimits.EarliestYear} and {DateTime.UtcNow.Year + 1}.");

            RuleFor(x => x.Seats)
                .InclusiveBetween(CarLimits.MinSeats, CarLimits.MaxSeats)
                .WithMessage($"Seat count must be between {CarLimits.MinSeats} and {CarLimits.MaxSeats}.");

            RuleFor(x => x.VehicleCategoryId).GreaterThan(0)
                .WithMessage("Select a vehicle category from the list.");
        }
    }
}
