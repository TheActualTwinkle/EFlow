using FluentValidation;

namespace EFlow.Booking.Application.Common.Validation;

public static class AuthValidationRules
{
    extension<T>(IRuleBuilder<T, string> ruleBuilder)
    {
        public IRuleBuilderOptions<T, string> ValidateUsername() =>
            ruleBuilder
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters")
                .MaximumLength(31).WithMessage("Username must not exceed 31 characters");

        public IRuleBuilderOptions<T, string> ValidatePassword() =>
            ruleBuilder
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .MaximumLength(63).WithMessage("Password must not exceed 63 characters");

        public IRuleBuilderOptions<T, string> ValidateEmail() =>
            ruleBuilder
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be valid")
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters");
    }
}
