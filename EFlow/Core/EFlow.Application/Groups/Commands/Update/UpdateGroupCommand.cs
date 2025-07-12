using System.Text.Json.Serialization;
using EFlow.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Application.Groups.Commands.Update;

public record UpdateGroupCommand : IRequest<Result>, ITransactionalRequest
{
    [JsonIgnore]
    public Guid Id { get; init; }

    public string? Name { get; init; }
}