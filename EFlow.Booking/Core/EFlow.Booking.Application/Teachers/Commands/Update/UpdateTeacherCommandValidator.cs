using EFlow.Booking.Application.Common.Validators;
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
            x => x.FirstName is not null,
            () => { RuleFor(x => x.FirstName)!.ValidateFirstName(); });

        When(
            x => x.LastName is not null,
            () => { RuleFor(x => x.LastName)!.ValidateLastName(); });

        When(
            x => x.BirthDate is not null,
            () => RuleFor(x => x.BirthDate)
                .Must(birthDate => birthDate.HasValue && birthDate.Value < DateOnly.FromDateTime(systemClock.UtcNow.AddYears(-18)))
                .WithMessage("Teacher must be at least 18 years old"));
    }
}