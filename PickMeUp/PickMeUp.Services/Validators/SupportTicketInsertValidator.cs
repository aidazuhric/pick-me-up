using PickMeUp.Model.Constants;
using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class SupportTicketInsertValidator : AbstractValidator<SupportTicketInsertRequest>
    {
        public SupportTicketInsertValidator()
        {
            RuleFor(x => x.Subject).NotEmpty()
                .WithMessage("Subject is required.")
                .MinimumLength(SupportTicketLimits.MinSubjectLength)
                .WithMessage($"Subject must be at least {SupportTicketLimits.MinSubjectLength} characters.")
                .MaximumLength(SupportTicketLimits.MaxSubjectLength)
                .WithMessage($"Subject must be at most {SupportTicketLimits.MaxSubjectLength} characters.");

            RuleFor(x => x.Body).NotEmpty()
                .WithMessage("Describe the problem so we can help.")
                .MinimumLength(SupportTicketLimits.MinBodyLength)
                .WithMessage($"Description must be at least {SupportTicketLimits.MinBodyLength} characters.")
                .MaximumLength(SupportTicketLimits.MaxBodyLength)
                .WithMessage($"Description must be at most {SupportTicketLimits.MaxBodyLength} characters.");
        }
    }
}
