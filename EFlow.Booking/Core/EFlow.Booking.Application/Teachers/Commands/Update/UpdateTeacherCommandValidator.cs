using EFlow.Booking.Application.Common.Validation;
using EFlow.Common.Infrastructure;
using FluentValidation;

namespace EFlow.Booking.Application.Teachers.Commands.Update;

public class UpdateTeacherCommandValidator : AbstractValidator<UpdateTeacherCommand>
{
    public UpdateTeacherCommandValidator(ISystemClock systemClock)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Teacher ID is required");

        When(
            x => x.Patch.FirstName.HasValue,
            () => { RuleFor(x => x.Patch.FirstName.Value!).ValidateFirstName(); });

        When(
            x => x.Patch.LastName.HasValue,
            () => { RuleFor(x => x.Patch.LastName.Value!).ValidateLastName(); });

        When(
            x => x.Patch.BirthDate.HasValue,
            () => RuleFor(x => x.Patch.BirthDate.Value)
                .Must(birthDate => birthDate < DateOnly.FromDateTime(systemClock.UtcNow.AddYears(-18)))
                .WithMessage("Teacher must be at least 18 years old"));
    }
}
