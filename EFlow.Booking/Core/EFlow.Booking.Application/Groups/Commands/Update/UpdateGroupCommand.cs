using EFlow.Booking.Application.Common.Markers;
using EFlow.Booking.Domain.Groups;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Groups.Commands.Update;

public record UpdateGroupCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid Id { get; init; }

    public required GroupUpdatePatch Patch { get; init; }
}
