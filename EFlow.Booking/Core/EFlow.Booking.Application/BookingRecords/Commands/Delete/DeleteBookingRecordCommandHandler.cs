using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Repositories;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Commands;

public class DeleteBookingRecordCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteBookingRecordCommand, Result>
{
    public async Task<Result> Handle(DeleteBookingRecordCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork
            .GetRepository<IBookingRecordRepository>()
            .DeleteAsync(request.Id, cancellationToken);

        return Result.Ok();
    }
}