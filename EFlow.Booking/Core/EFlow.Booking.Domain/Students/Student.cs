using EFlow.Booking.Domain.Common.BusinessRules;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Students.Events;
using EFlow.Booking.Domain.Students.Rules;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Students;

public sealed class Student : Entity, IAggreagateRoot
{
    public StudentId Id { get; }

    internal GroupId GroupId { get; private set; }

    internal string FirstName { get; private set; }

    internal string LastName { get; private set; }

    public string? MiddleName { get; private set; }

    internal DateOnly BirthDate { get; private set; }

    internal DateTime CreatedAt { get; private set; }

    private Student() { }
    
    public Student(
        StudentId id,
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
        
        Id = id;
        GroupId = groupId;
        FirstName = trimmedFirstName;
        LastName = trimmedLastName;
        MiddleName = trimmedMiddleName;
        BirthDate = birthDate;
        CreatedAt = createdAt;
    }
    
    public static Student Create(
        StudentId id,
        GroupId groupId,
        string firstName,
        string lastName,
        string? middleName,
        DateOnly birthDate,
        DateTime createdAt,
        DateTime now)
    {
        var student = new Student(
            id,
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

    public string GetFullName() =>
        string.Join(
            ' ',
            new[] { LastName, FirstName, MiddleName }.Where(name => !string.IsNullOrWhiteSpace(name)));

    public StudentId Delete()
    {
        AddDomainEvent(
            new StudentDeletedDomainEvent
            {
                StudentId = Id
            });

        return Id;
    }
    
    public void Update(StudentUpdatePatch patch, DateTime utcNow)
    {
        patch.ApplyInto(this);
        
        _ = new Student(
            Id,
            GroupId,
            FirstName,
            LastName,
            MiddleName,
            BirthDate,
            CreatedAt,
            utcNow);
        
        AddDomainEvent(
            new StudentUpdatedDomainEvent
            {
                StudentId = Id,
                UpdatedAt = utcNow
            });
    }
}
