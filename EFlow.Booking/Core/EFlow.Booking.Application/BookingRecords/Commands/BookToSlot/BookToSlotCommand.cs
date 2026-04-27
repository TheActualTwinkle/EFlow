using EFlow.Booking.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Commands;

public record BookToSlotCommand : IRequest<Result<Guid>>, ITransactionalRequest
{
    public required Guid StudentId { get; init; }

    public required Guid SlotId { get; init; }
}