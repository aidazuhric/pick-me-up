using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class PaymentStatusUpdateValidator : AbstractValidator<PaymentStatusUpdateRequest>
    {
        public PaymentStatusUpdateValidator()
        {
            RuleFor(x => x.Name).NotEmpty()
                .When(x => x.Name != null)
                .WithMessage("Name cannot be empty.")
                .MaximumLength(50)
                .WithMessage("Name must be at most 50 characters.");

            RuleFor(x => x.Description).MaximumLength(300)
                .When(x => x.Description != null)
                .WithMessage("Description must be at most 300 characters.");

            RuleFor(x => x.SortOrder!.Value).GreaterThanOrEqualTo(0)
                .When(x => x.SortOrder.HasValue)
                .WithMessage("SortOrder must be zero or greater.");
        }
    }
}
