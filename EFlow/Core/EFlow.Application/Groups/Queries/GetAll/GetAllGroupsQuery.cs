using FluentResults;
using MediatR;

namespace EFlow.Application.Groups.Queries;

public record GetAllGroupsQuery : IRequest<Result<IEnumerable<GroupDto>>>;