using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Domain.BookingRecords;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Commands;

public class BookToSlotCommandHandler(
    IUnitOfWork unitOfWork,
    ISystemClock systemClock)
    : IRequestHandler<BookToSlotCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(BookToSlotCommand request, CancellationToken cancellationToken)
    {
        var slot = await unitOfWork
            .GetRepository<ISubmissionSlotRepository>()
            .GetByIdAsync(new SubmissionSlotId(request.SlotId), cancellationToken);
        
        if (slot is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Submission Slot not found")
                    .WithId(request.SlotId));
        
        var student = await unitOfWork
            .GetRepository<IStudentRepository>()
            .GetByIdAsync(new StudentId(request.StudentId), cancellationToken);

        if (student is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Student not found")
                    .WithId(request.StudentId));

        var bookingRepository = unitOfWork.GetRepository<IBookingRecordRepository>();
        var existingBookings = (await bookingRepository.GetBySlotIdAsync(slot.Id, cancellationToken)).ToArray();

        var bookingRecord = slot.BookToSlot(student, existingBookings, systemClock.UtcNow);
        
        await bookingRepository.CreateAsync(bookingRecord, cancellationToken);
        
        return Result.Ok(bookingRecord.Id.Value);
    }
}
