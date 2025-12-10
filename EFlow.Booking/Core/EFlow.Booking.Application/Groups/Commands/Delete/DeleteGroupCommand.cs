using EFlow.Booking.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Groups.Commands;

public record DeleteGroupCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid Id { get; init; }
}