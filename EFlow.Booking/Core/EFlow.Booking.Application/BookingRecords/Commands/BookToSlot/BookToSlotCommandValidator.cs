using FluentValidation;

namespace EFlow.Booking.Application.BookingRecords.Commands;

public class BookToSlotCommandValidator : AbstractValidator<BookToSlotCommand>
{
    public BookToSlotCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required");

        RuleFor(x => x.SlotId)
            .NotEmpty().WithMessage("Slot ID is required");
    }
}