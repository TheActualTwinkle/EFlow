using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Groups.Queries;

public record GetGroupByIdQuery : IRequest<Result<GroupDto>>
{
    public required Guid Id { get; init; }
}