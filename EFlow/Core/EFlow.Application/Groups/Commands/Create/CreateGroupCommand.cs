using EFlow.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Application.Groups.Commands;

public record CreateGroupCommand : IRequest<Result<Guid>>, ITransactionalRequest
{
    public required string Name { get; init; }
}