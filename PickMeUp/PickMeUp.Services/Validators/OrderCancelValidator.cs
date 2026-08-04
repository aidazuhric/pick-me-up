using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class OrderCancelValidator : AbstractValidator<OrderCancelRequest>
    {
        public OrderCancelValidator()
        {
            RuleFor(x => x.Reason).NotEmpty()
                .WithMessage("A reason is required when cancelling an order.")
                .MaximumLength(500)
                .WithMessage("Reason must be at most 500 characters.");
        }
    }
}
