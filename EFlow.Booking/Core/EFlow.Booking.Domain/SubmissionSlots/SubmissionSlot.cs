using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.SubmissionSlots.Events;
using EFlow.Booking.Domain.SubmissionSlots.Rules;
using EFlow.Booking.Subjects;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots;

public sealed class SubmissionSlot : Entity
{
    internal SubmissionSlotId Id { get; private set; }

    internal SubjectId SubjectId { get; private set; }

    internal DateTime StartTime { get; private set; }

    internal DateTime EndTime { get; private set; }

    internal int MaxStudents { get; private set; }

    internal bool AllowAllGroups { get; private set; }

    internal ICollection<GroupId> AllowedGroupIds { get; private set; }

    internal string? Location { get; private set; }
    
    private SubmissionSlot(
        SubjectId subjectId,
        DateTime startTime,
        DateTime endTime,
        int maxStudents,
        bool allowAllGroups,
        ICollection<GroupId>? allowedGroupIds = null,
        string? location = null)
    {
        allowedGroupIds ??= []; 
        
        ThrowIfBroken(new StartTimeMustBeBeforeEndTimeRule(startTime, endTime));
        
        ThrowIfBroken(new MaxStudentsMustBePositiveRule(maxStudents));
        
        ThrowIfBroken(new AllowedGroupIdsMustNotBeEmptyWhenAllowAllGroupsIsFalseRule(allowAllGroups, allowedGroupIds));
        
        ThrowIfBroken(new AllowedGroupIdsMustBeEmptyWhenAllowAllGroupsIsTrueRule(allowAllGroups, allowedGroupIds));
        
        ThrowIfBroken(new AllowedGroupIdsMustNotContainDuplicatesRule(allowedGroupIds));
        
        Id = new SubmissionSlotId(Guid.CreateVersion7());
        SubjectId = subjectId;
        StartTime = startTime;
        EndTime = endTime;
        MaxStudents = maxStudents;
        AllowAllGroups = allowAllGroups;
        AllowedGroupIds = allowedGroupIds;
        Location = location;
    }
    
    public static SubmissionSlot Create(
        SubjectId subjectId,
        DateTime startTime,
        DateTime endTime,
        int maxStudents,
        bool allowAllGroups,
        DateTime nowUtc,
        ICollection<GroupId>? allowedGroupIds = null,
        string? location = null)
    {
        var slot = new SubmissionSlot(subjectId, startTime, endTime, maxStudents, allowAllGroups, allowedGroupIds, location);
        
        slot.AddDomainEvent(new SubmissionSlotCreatedDomainEvent
        {
            SlotId = slot.Id,
            CreatedAt = nowUtc
        });
        
        return slot;
    }
    
    // TODO:
}