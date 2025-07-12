using EFlow.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Application.Bookings.Commands;

public record CreateBookingCommand : IRequest<Result<Guid>>, ITransactionalRequest
{
    public required Guid StudentId { get; init; }

    public required Guid SlotId { get; init; }
}