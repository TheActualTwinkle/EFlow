using EFlow.Application.Common.Errors.Abstractions;
using FluentResults;

namespace EFlow.Application.Common.Errors.Teachers;

public record TeacherNotFoundError : NotFoundError
{
    public override string? Message { get; init; }

    public override Dictionary<string, object>? Metadata { get; init; }

    public override List<IError>? Reasons { get; init; }
}