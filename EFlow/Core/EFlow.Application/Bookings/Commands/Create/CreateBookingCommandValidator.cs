using FluentValidation;

namespace EFlow.Application.Bookings.Commands;

public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required");

        RuleFor(x => x.SlotId)
            .NotEmpty().WithMessage("Slot ID is required");
    }
}