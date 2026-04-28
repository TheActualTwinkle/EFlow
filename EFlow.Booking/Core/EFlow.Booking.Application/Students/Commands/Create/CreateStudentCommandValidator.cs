using EFlow.Booking.Application.Common.Validation;
using EFlow.Common.Infrastructure;
using FluentValidation;

namespace EFlow.Booking.Application.Students.Commands;

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator(ISystemClock systemClock)
    {
        RuleFor(x => x.UserName).ValidateUsername();
        RuleFor(x => x.Password).ValidatePassword();
        RuleFor(x => x.Email).ValidateEmail();

        RuleFor(x => x.GroupId)
            .NotEmpty().WithMessage("Group ID is required");

        RuleFor(x => x.FirstName).ValidateFirstName();
        RuleFor(x => x.LastName).ValidateLastName();

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Birth date is required")
            .Must(birthDate => birthDate < DateOnly.FromDateTime(systemClock.UtcNow.AddYears(-14)))
            .WithMessage("Student must be at least 14 years old");
    }
}
