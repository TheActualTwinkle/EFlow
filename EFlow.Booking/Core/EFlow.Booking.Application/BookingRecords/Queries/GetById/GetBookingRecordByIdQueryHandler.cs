using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Contracts.BookingRecords;
using EFlow.Booking.Domain.BookingRecords;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Queries;

public class GetBookingRecordByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetBookingRecordByIdQuery, Result<BookingRecordView>>
{
    public async Task<Result<BookingRecordView>> Handle(GetBookingRecordByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await unitOfWork
            .GetQueryService<IBookingRecordQueryService>()
            .GetByIdAsync(new BookingRecordId(request.Id), cancellationToken);

        if (booking is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("BookingRecord not found")
                    .WithId(request.Id));

        return Result.Ok(booking); 
    }
}