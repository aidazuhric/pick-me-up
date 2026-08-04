using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class OrderStatusInsertValidator : AbstractValidator<OrderStatusInsertRequest>
    {
        public OrderStatusInsertValidator()
        {
            RuleFor(x => x.Name).NotEmpty()
                .WithMessage("Name is required.")
                .MaximumLength(50)
                .WithMessage("Name must be at most 50 characters.");

            RuleFor(x => x.Description).MaximumLength(300)
                .WithMessage("Description must be at most 300 characters.");

            RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0)
                .WithMessage("SortOrder must be zero or greater.");
        }
    }
}
