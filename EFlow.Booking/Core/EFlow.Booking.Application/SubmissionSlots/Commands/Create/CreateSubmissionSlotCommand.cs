using EFlow.Booking.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Commands;

public record CreateSubmissionSlotCommand : IRequest<Result<Guid>>, ITransactionalRequest
{
    public required Guid SubjectId { get; init; }
    
    public required Guid TeacherId { get; init; }

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required int MaxStudents { get; init; }

    public required bool AllowAllGroups { get; init; }

    public ICollection<Guid>? AllowedGroupIds { get; init; }

    public string? Location { get; init; }

    public string? Comment { get; init; }
}
