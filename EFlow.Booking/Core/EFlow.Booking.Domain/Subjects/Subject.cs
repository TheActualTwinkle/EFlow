using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Teachers;
using EFlow.Booking.Subjects.Events;
using EFlow.Booking.Subjects.Rules;
using EFlow.Common.Domain;

namespace EFlow.Booking.Subjects;

public sealed class Subject : Entity, IAggreagateRoot
{
    internal SubjectId Id { get; }

    internal string Name { get; private set; }

    internal TeacherId TeacherId { get; private set; }
    
    internal ICollection<GroupId> GroupIds { get; private set; }
    
    private Subject(
        string name,
        TeacherId teacherId,
        ICollection<GroupId> groupIds)
    {
        var trimmedName = name.Trim();
        
        ThrowIfBroken(new SubjectNameMustBeProperLengthRule(trimmedName));
        
        ThrowIfBroken(new GroupIdsMustNotDuplicateRule(groupIds));

        Id = new SubjectId(Guid.CreateVersion7());
        Name = trimmedName;
        TeacherId = teacherId;
        GroupIds = groupIds;
    }

    public static Subject Create(
        string name,
        TeacherId teacherId,
        ICollection<GroupId> groupIds)
    {
        var subject = new Subject(name, teacherId, groupIds);
        
        subject.AddDomainEvent(new SubjectCreatedDomainEvent
        {
            SubjectId = subject.Id,
            TeacherId = teacherId,
            GroupIds = groupIds
        });

        return subject;
    }
    
    public SubjectId Delete()
    {
        AddDomainEvent(new SubjectDeletedDomainEvent
        {
            SubjectId = Id
        });

        return Id;
    }
    
    // public void Update(SubjectUpdatePatch patch)
    // {
    //     var result = patch.ApplyTo(this);
    //     
    //     // TODO
    //
    //     AddDomainEvent(new SubjectUpdatedDomainEvent
    //     {
    //         SubjectId = Id,
    //         UpdatedAt = DateTime.UtcNow
    //     });
    // }
}