using FluentValidation;

namespace EFlow.Booking.Application.Common.Validation;

public static class NameValidationRules
{
    public static IRuleBuilderOptions<T, string> ValidateFirstName<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(31).WithMessage("First name must not exceed 31 characters");

    public static IRuleBuilderOptions<T, string> ValidateLastName<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(31).WithMessage("Last name must not exceed 31 characters");
}