using PickMeUp.Model.Constants;
using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class ReviewInsertValidator : AbstractValidator<ReviewInsertRequest>
    {
        public ReviewInsertValidator()
        {
            RuleFor(x => x.OrderId).GreaterThan(0)
                .WithMessage("Choose the ride you are reviewing.");

            RuleFor(x => x.Rating)
                .InclusiveBetween(ReviewLimits.MinRating, ReviewLimits.MaxRating)
                .WithMessage($"Rating must be between {ReviewLimits.MinRating} and {ReviewLimits.MaxRating}.");

            RuleFor(x => x.Comment).MaximumLength(ReviewLimits.MaxCommentLength)
                .WithMessage($"Comment must be at most {ReviewLimits.MaxCommentLength} characters.");

            RuleFor(x => x).Must(x => x.ReviewCompany || x.ReviewDriver)
                .WithMessage("Choose whether you are reviewing the company, the driver, or both.");
        }
    }
}
