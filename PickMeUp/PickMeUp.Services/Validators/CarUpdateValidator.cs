using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class CarUpdateValidator : AbstractValidator<CarUpdateRequest>
    {
        public CarUpdateValidator()
        {
            RuleFor(x => x.Make).NotEmpty()
                .When(x => x.Make != null)
                .WithMessage("Make cannot be empty.")
                .MaximumLength(60)
                .WithMessage("Make must be at most 60 characters.");

            RuleFor(x => x.Model).NotEmpty()
                .When(x => x.Model != null)
                .WithMessage("Model cannot be empty.")
                .MaximumLength(60)
                .WithMessage("Model must be at most 60 characters.");

            RuleFor(x => x.Plate).NotEmpty()
                .When(x => x.Plate != null)
                .WithMessage("Registration plate cannot be empty.")
                .MaximumLength(20)
                .WithMessage("Registration plate must be at most 20 characters.");

            RuleFor(x => x.Year!.Value)
                .InclusiveBetween(CarLimits.EarliestYear, DateTime.UtcNow.Year + 1)
                .When(x => x.Year.HasValue)
                .WithMessage($"Production year must be between {CarLimits.EarliestYear} and {DateTime.UtcNow.Year + 1}.");

            RuleFor(x => x.Seats!.Value)
                .InclusiveBetween(CarLimits.MinSeats, CarLimits.MaxSeats)
                .When(x => x.Seats.HasValue)
                .WithMessage($"Seat count must be between {CarLimits.MinSeats} and {CarLimits.MaxSeats}.");

            RuleFor(x => x.VehicleCategoryId!.Value).GreaterThan(0)
                .When(x => x.VehicleCategoryId.HasValue)
                .WithMessage("Select a vehicle category from the list.");
        }
    }
}
