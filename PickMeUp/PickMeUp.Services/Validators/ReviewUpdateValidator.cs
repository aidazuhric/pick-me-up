using PickMeUp.Model.Constants;
using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class ReviewUpdateValidator : AbstractValidator<ReviewUpdateRequest>
    {
        public ReviewUpdateValidator()
        {
            RuleFor(x => x.Rating!.Value)
                .InclusiveBetween(ReviewLimits.MinRating, ReviewLimits.MaxRating)
                .When(x => x.Rating.HasValue)
                .WithMessage($"Rating must be between {ReviewLimits.MinRating} and {ReviewLimits.MaxRating}.");

            RuleFor(x => x.Comment).MaximumLength(ReviewLimits.MaxCommentLength)
                .WithMessage($"Comment must be at most {ReviewLimits.MaxCommentLength} characters.");
        }
    }
}
