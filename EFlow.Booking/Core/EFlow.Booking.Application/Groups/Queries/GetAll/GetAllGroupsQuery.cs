using EFlow.Booking.Contracts.Groups;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Groups.Queries;

public record GetAllGroupsQuery : IRequest<Result<IEnumerable<GroupView>>>;