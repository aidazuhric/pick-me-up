using PickMeUp.Model.Requests;
using FluentValidation;

namespace PickMeUp.Services.Validators
{
    public class UserPasswordChangeValidator : AbstractValidator<UserPasswordChangeRequest>
    {
        public UserPasswordChangeValidator()
        {
            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Password is required.")
                .PasswordStrength();
        }
    }
}
