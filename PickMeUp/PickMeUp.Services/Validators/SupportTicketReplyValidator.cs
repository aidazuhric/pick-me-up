using PickMeUp.Model.Constants;
using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class SupportTicketReplyValidator : AbstractValidator<SupportTicketReplyRequest>
    {
        public SupportTicketReplyValidator()
        {
            RuleFor(x => x.Body).NotEmpty()
                .WithMessage("A reply cannot be empty.")
                .MaximumLength(SupportTicketLimits.MaxReplyLength)
                .WithMessage($"A reply must be at most {SupportTicketLimits.MaxReplyLength} characters.");
        }
    }
}
