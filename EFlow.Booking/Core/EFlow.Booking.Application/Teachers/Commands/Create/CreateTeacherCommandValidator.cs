using EFlow.Booking.Application.Common.Validation;
using EFlow.Common.Infrastructure;
using FluentValidation;

namespace EFlow.Booking.Application.Teachers.Commands;

public class CreateTeacherCommandValidator : AbstractValidator<CreateTeacherCommand>
{
    public CreateTeacherCommandValidator(ISystemClock systemClock)
    {
        RuleFor(x => x.UserName).ValidateUsername();
        RuleFor(x => x.Password).ValidatePassword();
        RuleFor(x => x.Email).ValidateEmail();

        RuleFor(x => x.FirstName).ValidateFirstName();
        RuleFor(x => x.LastName).ValidateLastName();

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Birth date is required")
            .Must(birthDate => birthDate < DateOnly.FromDateTime(systemClock.UtcNow.AddYears(-18)))
            .WithMessage("Teacher must be at least 18 years old");
    }
}
