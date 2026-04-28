using FluentValidation;

namespace EFlow.Booking.Application.Common.Validation;

public static class NameValidationRules
{
    extension<T>(IRuleBuilder<T, string> ruleBuilder)
    {
        public IRuleBuilderOptions<T, string> ValidateFirstName() =>
            ruleBuilder
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(31).WithMessage("First name must not exceed 31 characters");

        public IRuleBuilderOptions<T, string> ValidateLastName() =>
            ruleBuilder
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(31).WithMessage("Last name must not exceed 31 characters");
    }
}