using EFlow.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Application.Admins.Commands;

public record CreateAdminCommand : IRequest<Result<Guid>>, ITransactionalRequest
{
    public required string UserName { get; init; }

    public required string Password { get; init; }

    public DateTime? CreatedAt { get; init; }
}