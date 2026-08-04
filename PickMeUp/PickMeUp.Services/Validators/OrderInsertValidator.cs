using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class OrderInsertValidator : AbstractValidator<OrderInsertRequest>
    {
        public OrderInsertValidator()
        {
            RuleFor(x => x.PickupAddress).NotEmpty()
                .WithMessage("Pick-up address is required. Choose the point on the map.")
                .MaximumLength(250)
                .WithMessage("Pick-up address must be at most 250 characters.");

            RuleFor(x => x.PickupLat).InclusiveBetween(-90, 90)
                .WithMessage("Pick-up latitude must be between -90 and 90.");

            RuleFor(x => x.PickupLng).InclusiveBetween(-180, 180)
                .WithMessage("Pick-up longitude must be between -180 and 180.");

            RuleFor(x => x.DestinationLat!.Value).InclusiveBetween(-90, 90)
                .When(x => x.DestinationLat.HasValue)
                .WithMessage("Destination latitude must be between -90 and 90.");

            RuleFor(x => x.DestinationLng!.Value).InclusiveBetween(-180, 180)
                .When(x => x.DestinationLng.HasValue)
                .WithMessage("Destination longitude must be between -180 and 180.");

            RuleFor(x => x.DestinationAddress).MaximumLength(250)
                .WithMessage("Destination address must be at most 250 characters.");

            RuleFor(x => x.DistanceKm!.Value).GreaterThanOrEqualTo(0)
                .When(x => x.DistanceKm.HasValue)
                .WithMessage("Distance cannot be negative.");
        }
    }
}
