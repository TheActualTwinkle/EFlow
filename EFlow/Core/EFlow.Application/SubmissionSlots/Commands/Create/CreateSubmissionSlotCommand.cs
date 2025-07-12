using EFlow.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Application.SubmissionSlots.Commands;

public record CreateSubmissionSlotCommand : IRequest<Result<Guid>>, ITransactionalRequest
{
    public required Guid SubjectId { get; init; }

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required int MaxStudents { get; init; }

    public string? Location { get; init; }
}