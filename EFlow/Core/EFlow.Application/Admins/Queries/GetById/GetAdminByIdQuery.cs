using FluentResults;
using MediatR;

namespace EFlow.Application.Admins.Queries;

public record GetAdminByIdQuery : IRequest<Result<AdminDto>>
{
    public required Guid Id { get; init; }
}