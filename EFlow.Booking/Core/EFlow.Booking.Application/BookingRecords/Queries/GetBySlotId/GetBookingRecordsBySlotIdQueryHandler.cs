using EFlow.Booking.Contracts.BookingRecords;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Queries;

public class GetBookingRecordsBySlotIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetBookingRecordsBySlotIdQuery, Result<IEnumerable<BookingRecordView>>>
{
    public async Task<Result<IEnumerable<BookingRecordView>>> Handle(GetBookingRecordsBySlotIdQuery request, CancellationToken cancellationToken)
    {
        var bookings = await unitOfWork
            .GetQueryService<IBookingRecordQueryService>()
            .GetBySlotIdAsync(new SubmissionSlotId(request.SlotId), cancellationToken);

        return Result.Ok(bookings);
    }
}