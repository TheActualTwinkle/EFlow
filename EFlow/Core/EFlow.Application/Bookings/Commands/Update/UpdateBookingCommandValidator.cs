using FluentValidation;

namespace EFlow.Application.Bookings.Commands.Update;

public class UpdateBookingCommandValidator : AbstractValidator<UpdateBookingCommand>
{
    public UpdateBookingCommandValidator()
    {
        When(
            x => x.StudentId is not null, () =>
            {
                RuleFor(x => x.StudentId)
                    .NotEmpty().WithMessage("Student ID cannot be empty");
            });

        When(
            x => x.SlotId is not null, () =>
            {
                RuleFor(x => x.SlotId)
                    .NotEmpty().WithMessage("Slot ID cannot be empty");
            });
    }
}