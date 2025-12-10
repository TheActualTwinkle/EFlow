using EFlow.Booking.Application.Common.Validators;
using FluentValidation;

namespace EFlow.Booking.Application.Students.Commands.Update;

public class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
{
    public UpdateStudentCommandValidator()
    {
        When(
            x => x.FirstName is not null,
            () => { RuleFor(x => x.FirstName)!.ValidateFirstName(); });

        When(
            x => x.LastName is not null,
            () => { RuleFor(x => x.LastName)!.ValidateLastName(); });

        When(
            x => x.BirthDate is not null,
            () =>
            {
                RuleFor(x => x.BirthDate)
                    .LessThan(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-14)))
                    .WithMessage("Student must be at least 14 years old");
            });
    }
}