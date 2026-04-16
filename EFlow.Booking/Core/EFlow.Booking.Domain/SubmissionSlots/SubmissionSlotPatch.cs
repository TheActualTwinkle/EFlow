using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Subjects;
using FluentPatcher;
using FluentPatcher.Attributes;

namespace EFlow.Booking.Domain.SubmissionSlots;

[PatchFor(typeof(SubmissionSlot))]
public class SubmissionSlotPatch
{
    public Patchable<SubjectId> SubjectId { get; }
    
    public Patchable<DateTime> StartTime { get; }
    
    public Patchable<DateTime> EndTime { get; }
    
    public Patchable<int> MaxStudents { get; }
    
    public Patchable<bool> AllowAllGroups { get; }
    
    public Patchable<ICollection<GroupId>> AllowedGroupIds { get; }
    
    public Patchable<string?> Location { get; }
}