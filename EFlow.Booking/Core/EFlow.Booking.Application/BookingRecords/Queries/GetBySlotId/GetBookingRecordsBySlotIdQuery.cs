using EFlow.Booking.Contracts.BookingRecords;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Queries;

public record GetBookingRecordsBySlotIdQuery : IRequest<Result<IEnumerable<BookingRecordView>>>
{
    public required Guid SlotId { get; init; }
}