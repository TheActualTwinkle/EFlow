using EFlow.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Application.Students.Commands;

public record CreateStudentCommand : IRequest<Result<Guid>>, ITransactionalRequest
{
    public required string UserName { get; init; }

    public required string Password { get; init; }

    public required Guid GroupId { get; init; }

    public required string FirstName { get; init; }

    public string? MiddleName { get; init; }

    public required string LastName { get; init; }

    public required DateOnly BirthDate { get; init; }
}