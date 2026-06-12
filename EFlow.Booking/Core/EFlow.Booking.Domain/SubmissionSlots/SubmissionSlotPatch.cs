using EFlow.Booking.Domain.Groups;
using FluentPatcher;
using FluentPatcher.Attributes;

namespace EFlow.Booking.Domain.SubmissionSlots;

[PatchFor(typeof(SubmissionSlot))]
public class SubmissionSlotPatch
{
    public Patchable<DateTime> StartTime { get; init; }
    
    public Patchable<DateTime> EndTime { get; init; }
    
    public Patchable<int> MaxStudents { get; init; }
    
    public Patchable<bool> AllowAllGroups { get; init; }
    
    public Patchable<List<GroupId>> AllowedGroupIds { get; init; }
    
    public Patchable<string?> Location { get; init; }

    public Patchable<string?> Comment { get; init; }
}
