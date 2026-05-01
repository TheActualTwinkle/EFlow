using EFlow.Booking.Contracts.BookingRecords;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Queries;

public class GetAllBookingRecordsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllBookingRecordsQuery, Result<IEnumerable<BookingRecordView>>>
{
    public async Task<Result<IEnumerable<BookingRecordView>>> Handle(GetAllBookingRecordsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await unitOfWork
            .GetQueryService<IBookingRecordQueryService>()
            .GetAllAsync(cancellationToken);

        return Result.Ok(bookings);
    }
}