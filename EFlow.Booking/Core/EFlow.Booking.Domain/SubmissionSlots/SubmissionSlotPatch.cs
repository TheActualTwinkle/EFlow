using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.Teachers;
using FluentPatcher;
using FluentPatcher.Attributes;

namespace EFlow.Booking.Domain.SubmissionSlots;

[PatchFor(typeof(SubmissionSlot))]
public class SubmissionSlotPatch
{
    public Patchable<SubjectId> SubjectId { get; init; }

    public Patchable<TeacherId> TeacherId { get; init; }
    
    public Patchable<DateTime> StartTime { get; init; }
    
    public Patchable<DateTime> EndTime { get; init; }
    
    public Patchable<int> MaxStudents { get; init; }
    
    public Patchable<bool> AllowAllGroups { get; init; }
    
    public Patchable<ICollection<GroupId>> AllowedGroupIds { get; init; }
    
    public Patchable<string?> Location { get; init; }

    public Patchable<string?> Comment { get; init; }
}
