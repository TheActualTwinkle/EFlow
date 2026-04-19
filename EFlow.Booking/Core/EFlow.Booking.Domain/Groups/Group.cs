using EFlow.Booking.Domain.Groups.Events;
using EFlow.Booking.Domain.Groups.Rules;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.Subjects;
using EFlow.Common.Domain;
using EFlow.Common.Domain.Students;

namespace EFlow.Booking.Domain.Groups;

public sealed class Group : Entity, IAggreagateRoot
{
    public GroupId Id { get; }

    internal string Name { get; private set; }

    internal ICollection<Student> Students { get; private set; } = [];

    internal ICollection<Subject> Subjects { get; private set; } = []; // TODO: Check private set usage.

    private Group(string name, IEnumerable<string> existingGroupNames)
    {
        var trimmedName = name.Trim();
        
        ThrowIfBroken(new GroupNameMustBeProperLengthRule(trimmedName));
        ThrowIfBroken(new GroupNameMustUniqueRule(trimmedName, existingGroupNames));

        Id = new GroupId(Guid.CreateVersion7());
        Name = trimmedName;
    }
    
    private Group() { }
    
    public static Group Create(string name, IEnumerable<string> existingGroupNames)
    {
        var group = new Group(name, existingGroupNames);

        group.AddDomainEvent(new GroupCreatedDomainEvent
        {
            GroupId = group.Id
        });
        
        return group;
    }

    public GroupId Delete()
    {
        AddDomainEvent(new GroupDeletedDomainEvent
        {
            GroupId = Id
        });

        return Id;
    }
    
    // public void Update(GroupUpdatePatch patch)
    // {
    //     var result = patch.ApplyTo(this);
    //     
    //     // TODO
    //
    //     AddDomainEvent(new GroupUpdatedDomainEvent
    //     {
    //         GroupId = Id,
    //         UpdatedAt = DateTime.UtcNow
    //     });
    // }
    
    public void AddStudent(Student student)
    {
        ThrowIfBroken(new StudentMustNotBeAddedTwiceRule(Students, student));
        
        Students.Add(student);
        
        AddDomainEvent(new StudentAddedToGroupDomainEvent
        {
            GroupId = Id,
            StudentId = student.Id
        });
    }
    
    public void RemoveStudent(StudentId studentId)
    {
        var student = Students.FirstOrDefault(s => s.Id == studentId);
        
        if (student is null)
            return;
        
        Students.Remove(student);
        
        AddDomainEvent(new StudentRemovedFromGroupDomainEvent
        {
            GroupId = Id,
            StudentId = studentId
        });
    }
    
    public void AddSubject(Subject subject)
    {
        ThrowIfBroken(new SubjectMustNotBeAddedTwiceRule(Subjects, subject));
        
        Subjects.Add(subject);
        
        AddDomainEvent(new SubjectAddedToGroupDomainEvent
        {
            GroupId = Id,
            SubjectId = subject.Id
        });
    }
    
    public void RemoveSubject(SubjectId subjectId)
    {
        var subject = Subjects.FirstOrDefault(s => s.Id == subjectId);
        
        if (subject is null)
            return;
        
        Subjects.Remove(subject);
        
        AddDomainEvent(new SubjectRemovedFromGroupDomainEvent
        {
            GroupId = Id,
            SubjectId = subjectId
        });
    }
}