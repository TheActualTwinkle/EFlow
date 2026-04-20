using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Domain.BookingRecords;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Commands.Update;

public class UpdateBookingRecordCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBookingRecordCommand, Result>
{
    public async Task<Result> Handle(UpdateBookingRecordCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<IBookingRecordRepository>();

        var booking = await repository.GetByIdAsync(new BookingRecordId(request.Id), cancellationToken);

        if (booking is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("BookingRecord not found")
                    .WithId(request.Id));

        // TODO: Update Domain Model
        
        repository.Update(booking);

        return Result.Ok();
    }
}