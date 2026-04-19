using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Domain.BookingRecords;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Commands;

public class DeleteBookingRecordCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteBookingRecordCommand, Result>
{
    public async Task<Result> Handle(DeleteBookingRecordCommand request, CancellationToken cancellationToken)
    {
        var bookingRecordRepository = unitOfWork.GetRepository<IBookingRecordRepository>();

        var bookingRecord = await bookingRecordRepository.GetByIdAsync(request.Id, cancellationToken);

        if (bookingRecord is null)
            return Result.Ok();

        var slot = await unitOfWork
            .GetRepository<ISubmissionSlotRepository>()
            .GetByIdAsync(bookingRecord.GetSlotId().Value, cancellationToken);
        
        if (slot is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Submission Slot not found")
                    .WithId(bookingRecord.GetSlotId().Value));
        
        slot.CancelBooking(bookingRecord);
        
        await bookingRecordRepository.DeleteAsync(bookingRecord);

        return Result.Ok();
    }
}