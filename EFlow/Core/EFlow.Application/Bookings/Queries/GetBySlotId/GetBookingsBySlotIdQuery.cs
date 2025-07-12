using FluentResults;
using MediatR;

namespace EFlow.Application.Bookings.Queries;

public record GetBookingsBySlotIdQuery : IRequest<Result<IEnumerable<BookingDto>>>
{
    public required Guid SlotId { get; init; }
}