using FluentValidation;

namespace EFlow.Booking.Application.BookingRecords.Commands.Update;

public class UpdateBookingRecordCommandValidator : AbstractValidator<UpdateBookingRecordCommand>
{
    public UpdateBookingRecordCommandValidator()
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