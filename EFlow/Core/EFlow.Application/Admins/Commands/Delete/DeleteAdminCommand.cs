using EFlow.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Application.Admins.Commands;

public record DeleteAdminCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid Id { get; init; }
}