using EFlow.Booking.Contracts.Groups;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Groups.Queries;

public record GetGroupByIdQuery : IRequest<Result<GroupView>>
{
    public required Guid Id { get; init; }
}