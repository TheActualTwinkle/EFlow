using EFlow.Booking.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Commands.Update;

public record UpdateSubmissionSlotCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid Id { get; init; }
    
    public Guid? TeacherId { get; init; }

    public Guid? SubjectId { get; init; }

    public DateTime? StartTime { get; init; }

    public DateTime? EndTime { get; init; }

    public int? MaxStudents { get; init; }

    public bool? AllowAllGroups { get; init; }

    public ICollection<Guid>? AllowedGroupIds { get; init; }

    public string? Location { get; init; }

    public string? Comment { get; init; }
}
