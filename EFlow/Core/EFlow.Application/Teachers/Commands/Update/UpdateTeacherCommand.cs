using System.Text.Json.Serialization;
using EFlow.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Application.Teachers.Commands;

public record UpdateTeacherCommand : IRequest<Result>, ITransactionalRequest
{
    [JsonIgnore]
    public Guid IdentityId { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public string? MiddleName { get; init; }

    public DateOnly? BirthDate { get; init; }
}