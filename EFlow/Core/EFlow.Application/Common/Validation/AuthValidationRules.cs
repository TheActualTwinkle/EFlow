using FluentValidation;

namespace EFlow.Application.Common.Validators;

public static class AuthValidationRules
{
    public static IRuleBuilderOptions<T, string> ValidateUsername<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(31).WithMessage("Username must not exceed 31 characters");

    public static IRuleBuilderOptions<T, string> ValidatePassword<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(63).WithMessage("Password must not exceed 63 characters");
}