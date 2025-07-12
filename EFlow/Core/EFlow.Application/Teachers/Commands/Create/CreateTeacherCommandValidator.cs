using EFlow.Application.Common.Validators;
using FluentValidation;

namespace EFlow.Application.Teachers.Commands;

public class CreateTeacherCommandValidator : AbstractValidator<CreateTeacherCommand>
{
    public CreateTeacherCommandValidator()
    {
        RuleFor(x => x.UserName).ValidateUsername();
        RuleFor(x => x.Password).ValidatePassword();

        RuleFor(x => x.FirstName).ValidateFirstName();
        RuleFor(x => x.LastName).ValidateLastName();

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Birth date is required")
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)))
            .WithMessage("Teacher must be at least 18 years old");
    }
}