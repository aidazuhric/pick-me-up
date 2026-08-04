namespace PickMeUp.Services.Validators
{
    public static class ValidationPatterns
    {
        public const string Phone = @"^\+?[0-9\s\-]{6,20}$";

        public const string PhoneMessage =
            "Enter a valid phone number, digits only with an optional leading +, for example +38736111222.";
    }
}
