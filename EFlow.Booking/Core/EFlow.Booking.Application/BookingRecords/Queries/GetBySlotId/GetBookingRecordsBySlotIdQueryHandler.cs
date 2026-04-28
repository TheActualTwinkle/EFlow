using EFlow.Booking.Domain.BookingRecords;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.Infrastructure;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Queries;

public class GetBookingRecordsBySlotIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetBookingRecordsBySlotIdQuery, Result<IEnumerable<BookingRecordDto>>>
{
    public async Task<Result<IEnumerable<BookingRecordDto>>> Handle(GetBookingRecordsBySlotIdQuery request, CancellationToken cancellationToken)
    {
        var bookings = (await unitOfWork
                .GetRepository<IBookingRecordRepository>()
                .GetBySlotIdAsync(new SubmissionSlotId(request.SlotId), cancellationToken))
            .Adapt<IEnumerable<BookingRecordDto>>();

        return Result.Ok(bookings);
    }
}