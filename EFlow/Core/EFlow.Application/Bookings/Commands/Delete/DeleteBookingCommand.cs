using EFlow.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Application.Bookings.Commands;

public record DeleteBookingCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid Id { get; init; }
}