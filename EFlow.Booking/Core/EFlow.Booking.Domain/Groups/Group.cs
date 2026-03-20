using EFlow.Booking.Domain.Groups.Events;
using EFlow.Booking.Domain.Groups.Rules;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Subjects;
using EFlow.Common.Domain;
using EFlow.Common.Domain.Students;

namespace EFlow.Booking.Domain.Groups;

public sealed class Group : Entity, IAggreagateRoot
{
    internal GroupId Id { get; }

    internal string Name { get; private set; }

    internal ICollection<Student> Students { get; private set; } = [];

    internal ICollection<Subject> Subjects { get; private set; } = [];

    private Group(string name)
    {
        var trimmedName = name.Trim();
        
        ThrowIfBroken(new GroupNameMustBeProperLengthRule(trimmedName));

        Id = new GroupId(Guid.CreateVersion7());
        Name = trimmedName;
    }
    
    public static Group Create(string name)
    {
        var group = new Group(name);

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