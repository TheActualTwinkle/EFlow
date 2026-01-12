using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Models;
using EFlow.Booking.Domain.Repositories;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Commands;

public class CreateBookingRecordCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateBookingRecordCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateBookingRecordCommand request, CancellationToken cancellationToken)
    {
        var booking = new BookingRecord
        {
            Id = Guid.NewGuid(),
            StudentId = request.StudentId,
            SlotId = request.SlotId,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork
            .GetRepository<IBookingRecordRepository>()
            .CreateAsync(booking, cancellationToken);

        return Result.Ok(booking.Id);
    }
}