using EFlow.Booking.Domain.Common.BusinessRules;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.Students.Events;
using EFlow.Booking.Domain.Students.Rules;

namespace EFlow.Common.Domain.Students;

public sealed class Student : Entity, IAggreagateRoot
{
    internal StudentId Id { get; }

    internal GroupId GroupId { get; private set; }

    internal string FirstName { get; private set; }

    internal string LastName { get; private set; }

    public string? MiddleName { get; private set; }

    internal DateOnly BirthDate { get; private set; }

    internal DateTime CreatedAt { get; private set; }

    public Student(
        GroupId groupId,
        string firstName,
        string lastName,
        string? middleName,
        DateOnly birthDate,
        DateTime createdAt,
        DateTime utcNow)
    {
        var trimmedFirstName = firstName.Trim();
        ThrowIfBroken(new StudentFirstNameMustBeProperLengthRule(trimmedFirstName));
        
        var trimmedLastName = lastName.Trim();
        ThrowIfBroken(new StudentLastNameMustBeProperLengthRule(trimmedLastName));
        
        var trimmedMiddleName = middleName?.Trim();
        if (middleName is not null)
            ThrowIfBroken(new StudentMiddleNameMustBeProperLengthRule(trimmedMiddleName!));
        
        ThrowIfBroken(new StudentBirthDateMustBeInPastRule(birthDate, DateOnly.FromDateTime(createdAt)));
        
        ThrowIfBroken(new CreationTimeMustBeInPastRule(createdAt, utcNow));
        
        Id = new StudentId(Guid.CreateVersion7());
        GroupId = groupId;
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
        BirthDate = birthDate;
        CreatedAt = createdAt;
    }
    
    public static Student Create(
        GroupId groupId,
        string firstName,
        string lastName,
        string? middleName,
        DateOnly birthDate,
        DateTime createdAt,
        DateTime now)
    {
        var student = new Student(
            groupId,
            firstName,
            lastName,
            middleName,
            birthDate,
            createdAt,
            now);

        student.AddDomainEvent(
            new StudentCreatedDomainEvent
            {
                StudentId = student.Id,
                CreatedAt = student.CreatedAt
            });

        return student;
    }

    public StudentId Delete()
    {
        AddDomainEvent(
            new StudentDeletedDomainEvent
            {
                StudentId = Id
            });

        return Id;
    }
    
    // public void Update(StudentUpdatePatch patch, DateTime utcNow)
    // {
    //     var updatedStudent = patch.ApplyTo(this).Entity;
    //     
    //     // TODO.
    //     
    //     AddDomainEvent(
    //         new StudentUpdatedDomainEvent
    //         {
    //             StudentId = updatedStudent.Id,
    //             UpdatedAt = utcNow
    //         });
    // }
    
    public void ChangeGroup(GroupId newGroupId, DateTime utcNow)
    {
        ThrowIfBroken(new StudentCannotBeMovedToSameGroupRule(GroupId, newGroupId));
        
        var oldGroupId = GroupId;
        
        GroupId = newGroupId;
        
        AddDomainEvent(
            new StudentChangedGroupDomainEvent
            {
                StudentId = Id,
                OldGroupId = oldGroupId,
                NewGroupId = newGroupId,
                ChangedAt = utcNow
            });
    }
}