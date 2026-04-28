using EFlow.Booking.Domain.BookingRecords;
using EFlow.Common.Infrastructure;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Queries;

public class GetAllBookingRecordsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllBookingRecordsQuery, Result<IEnumerable<BookingRecordDto>>>
{
    public async Task<Result<IEnumerable<BookingRecordDto>>> Handle(GetAllBookingRecordsQuery request, CancellationToken cancellationToken)
    {
        var bookings = (await unitOfWork
                .GetRepository<IBookingRecordRepository>()
                .GetAllAsync(cancellationToken))
            .Adapt<IEnumerable<BookingRecordDto>>();

        return Result.Ok(bookings);
    }
}