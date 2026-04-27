using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Queries;

public record GetBookingRecordsBySlotIdQuery : IRequest<Result<IEnumerable<BookingRecordDto>>>
{
    public required Guid SlotId { get; init; }
}