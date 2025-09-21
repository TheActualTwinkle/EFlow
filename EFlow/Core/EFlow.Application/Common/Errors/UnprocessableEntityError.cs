using EFlow.Application.Common.Errors.Abstractions;
using FluentResults;

namespace EFlow.Application.Common.Errors;

public record UnprocessableEntityError : ApplicationError
{
    public override string? Message { get; init; }

    public override Dictionary<string, object>? Metadata { get; init; }

    public override List<IError>? Reasons { get; init; }
}