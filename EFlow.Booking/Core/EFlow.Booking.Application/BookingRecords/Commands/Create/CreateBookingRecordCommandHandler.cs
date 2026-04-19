using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Domain.BookingRecords;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Commands;

public class CreateBookingRecordCommandHandler(
    IUnitOfWork unitOfWork,
    ISystemClock systemClock)
    : IRequestHandler<CreateBookingRecordCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateBookingRecordCommand request, CancellationToken cancellationToken)
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
        
        var bookingRecord = slot.BookToSlot(student.Id, systemClock.UtcNow);
        
        await unitOfWork.GetRepository<IBookingRecordRepository>().CreateAsync(bookingRecord, cancellationToken);
        
        return Result.Ok(bookingRecord.Id.Value);
    }
}