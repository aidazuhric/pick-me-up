using FluentValidation;
using PickMeUp.Model.Requests;

namespace PickMeUp.Services.Validators
{
    public class PaymentRefundValidator : AbstractValidator<PaymentRefundRequest>
    {
        public PaymentRefundValidator()
        {
            RuleFor(x => x.IdempotencyKey)
                .Must(key => key.HasValue && key.Value != Guid.Empty)
                .WithMessage("A refund key is required so the same refund is never sent twice.");
        }
    }
}
