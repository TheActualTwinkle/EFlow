using EFlow.Booking.Application.Common.Validators;
using FluentValidation;

namespace EFlow.Booking.Application.Teachers.Commands.Update;

public class UpdateTeacherCommandValidator : AbstractValidator<UpdateTeacherCommand>
{
    public UpdateTeacherCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Teacher ID is required");

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
                    .LessThan(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)))
                    .WithMessage("Teacher must be at least 18 years old");
            });
    }
}