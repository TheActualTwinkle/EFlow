using EFlow.Booking.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Groups.Commands.Update;

public record UpdateGroupCommand : IRequest<Result>, ITransactionalRequest
{
    public Guid Id { get; init; }

    public string? Name { get; init; }
}