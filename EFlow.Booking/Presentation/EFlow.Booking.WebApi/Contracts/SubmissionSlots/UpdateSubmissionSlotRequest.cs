using FluentPatcher;

namespace EFlow.Booking.WebApi.Contracts.SubmissionSlots;

public record UpdateSubmissionSlotRequest
{
    public Patchable<Guid> SubjectId { get; init; }
    
    public Patchable<Guid> TeacherId { get; init; }

    public Patchable<DateTime> StartTime { get; init; }

    public Patchable<DateTime> EndTime { get; init; }

    public Patchable<int> MaxStudents { get; init; }

    public Patchable<bool> AllowAllGroups { get; init; }

    public Patchable<ICollection<Guid>> AllowedGroupIds { get; init; }

    public Patchable<string?> Location { get; init; }

    public Patchable<string?> Comment { get; init; }
}
