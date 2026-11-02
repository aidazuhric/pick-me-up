using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public static class PasswordRules
    {
        public const int MinLength = 6;

        public const int MaxLength = 100;

        public const string TooShortMessage = "Password must be at least 6 characters.";

        public const string TooLongMessage = "Password cannot exceed 100 characters.";

        public static IRuleBuilderOptions<T, string> PasswordStrength<T>(this IRuleBuilder<T, string> rule) =>
            rule.MinimumLength(MinLength)
                .WithMessage(TooShortMessage)
                .MaximumLength(MaxLength)
                .WithMessage(TooLongMessage);
    }
}
