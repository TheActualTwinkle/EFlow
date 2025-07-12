using System.Text.Json.Serialization;
using EFlow.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Application.Subjects.Commands.Update;

public record UpdateSubjectCommand : IRequest<Result>, ITransactionalRequest
{
    [JsonIgnore]
    public Guid Id { get; init; }

    public string? Name { get; init; }

    public Guid? TeacherId { get; init; }
}