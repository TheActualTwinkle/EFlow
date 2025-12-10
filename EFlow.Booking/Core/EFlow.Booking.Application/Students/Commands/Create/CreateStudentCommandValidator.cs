using EFlow.Booking.Application.Common.Validators;
using FluentValidation;

namespace EFlow.Booking.Application.Students.Commands;

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.UserName).ValidateUsername();
        RuleFor(x => x.Password).ValidatePassword();

        RuleFor(x => x.GroupId)
            .NotEmpty().WithMessage("Group ID is required");

        RuleFor(x => x.FirstName).ValidateFirstName();
        RuleFor(x => x.LastName).ValidateLastName();

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Birth date is required")
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-14)))
            .WithMessage("Student must be at least 14 years old");
    }
}