using System;
using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class OrderInsertValidator : AbstractValidator<OrderInsertRequest>
    {
        private const double CoordinateEpsilon = 0.0001;

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

            RuleFor(x => x)
                .Must(x => Math.Abs(x.PickupLat) > CoordinateEpsilon || Math.Abs(x.PickupLng) > CoordinateEpsilon)
                .WithName(nameof(OrderInsertRequest.PickupLat))
                .WithMessage("Pick your pick-up point on the map first.");

            RuleFor(x => x.DestinationAddress).NotEmpty()
                .WithMessage("Destination address is required. Choose the point on the map.")
                .MaximumLength(250)
                .WithMessage("Destination address must be at most 250 characters.");

            RuleFor(x => x.DestinationLat).NotNull()
                .WithMessage("Pick your destination on the map first.");

            RuleFor(x => x.DestinationLng).NotNull()
                .WithMessage("Pick your destination on the map first.");

            RuleFor(x => x.DestinationLat!.Value).InclusiveBetween(-90, 90)
                .When(x => x.DestinationLat.HasValue)
                .WithMessage("Destination latitude must be between -90 and 90.");

            RuleFor(x => x.DestinationLng!.Value).InclusiveBetween(-180, 180)
                .When(x => x.DestinationLng.HasValue)
                .WithMessage("Destination longitude must be between -180 and 180.");

            RuleFor(x => x)
                .Must(x => Math.Abs(x.DestinationLat!.Value) > CoordinateEpsilon
                    || Math.Abs(x.DestinationLng!.Value) > CoordinateEpsilon)
                .When(x => x.DestinationLat.HasValue && x.DestinationLng.HasValue)
                .WithName(nameof(OrderInsertRequest.DestinationLat))
                .WithMessage("Pick your destination on the map first.");
        }
    }
}
