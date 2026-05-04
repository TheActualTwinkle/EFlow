using EFlow.Booking.Application.Common.Validation;
using EFlow.Common.Infrastructure;
using FluentValidation;

namespace EFlow.Booking.Application.Students.Commands.Update;

public class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
{
    public UpdateStudentCommandValidator(ISystemClock systemClock)
    {
        When(
            x => x.Patch.FirstName.HasValue,
            () => { RuleFor(x => x.Patch.FirstName.Value!).ValidateFirstName(); });

        When(
            x => x.Patch.LastName.HasValue,
            () => { RuleFor(x => x.Patch.LastName.Value!).ValidateLastName(); });

        When(
            x => x.Patch.BirthDate.HasValue,
            () =>
            {
                RuleFor(x => x.Patch.BirthDate.Value)
                    .Must(birthDate => birthDate < DateOnly.FromDateTime(systemClock.UtcNow.AddYears(-14)))
                    .WithMessage("Student must be at least 14 years old");
            });
    }
}
