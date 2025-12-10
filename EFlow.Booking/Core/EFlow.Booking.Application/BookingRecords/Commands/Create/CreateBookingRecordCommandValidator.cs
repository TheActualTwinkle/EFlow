using FluentValidation;

namespace EFlow.Booking.Application.BookingRecords.Commands;

public class CreateBookingRecordCommandValidator : AbstractValidator<CreateBookingRecordCommand>
{
    public CreateBookingRecordCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required");

        RuleFor(x => x.SlotId)
            .NotEmpty().WithMessage("Slot ID is required");
    }
}