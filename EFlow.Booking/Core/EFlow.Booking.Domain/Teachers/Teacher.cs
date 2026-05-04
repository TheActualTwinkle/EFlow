using EFlow.Booking.Domain.Common.BusinessRules;
using EFlow.Booking.Domain.Teachers.Events;
using EFlow.Booking.Domain.Teachers.Rules;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Teachers;

public sealed class Teacher : Entity, IAggreagateRoot
{
    public TeacherId Id { get; }

    internal string FirstName { get; private set; }

    internal string LastName { get; private set; }

    internal string? MiddleName { get; private set; }

    internal DateOnly BirthDate { get; private set; }

    internal DateTime CreatedAt { get; private set; }

    private Teacher() { }
    
    private Teacher(
        TeacherId id,
        string firstName,
        string lastName,
        string? middleName,
        DateOnly birthDate,
        DateTime createdAt,
        DateTime utcNow)
    {
        var trimmedFirstName = firstName.Trim();
        ThrowIfBroken(new TeacherFirstNameMustBeProperLengthRule(trimmedFirstName));
        
        var trimmedLastName = lastName.Trim();
        ThrowIfBroken(new TeacherLastNameMustBeProperLengthRule(trimmedLastName));

        var trimmedMiddleName = middleName?.Trim();
        if (middleName is not null)
            ThrowIfBroken(new TeacherMiddleNameMustBeProperLengthRule(trimmedMiddleName!));

        ThrowIfBroken(new TeacherBirthDateMustBeInPastRule(birthDate, DateOnly.FromDateTime(utcNow)));

        ThrowIfBroken(new CreationTimeMustBeInPastRule(createdAt, utcNow));

        Id = id;
        FirstName = trimmedFirstName;
        LastName = trimmedLastName;
        MiddleName = trimmedMiddleName;
        BirthDate = birthDate;
        CreatedAt = createdAt;
    }

    public static Teacher Create(
        TeacherId id,
        string firstName,
        string lastName,
        string? middleName,
        DateOnly birthDate,
        DateTime createdAt,
        DateTime now)
    {
        var teacher = new Teacher(
            id,
            firstName,
            lastName,
            middleName,
            birthDate,
            createdAt,
            now);

        teacher.AddDomainEvent(
            new TeacherCreatedDomainEvent
            {
                TeacherId = teacher.Id,
                CreatedAt = teacher.CreatedAt
            });

        return teacher;
    }

    public string GetFullName() =>
        string.Join(
            ' ',
            new[] { LastName, FirstName, MiddleName }.Where(name => !string.IsNullOrWhiteSpace(name)));

    public TeacherId Delete()
    {
        AddDomainEvent(
            new TeacherDeletedDomainEvent
            {
                TeacherId = Id
            });

        return Id;
    }

    public void Update(
        TeacherUpdatePatch patch,
        DateTime utcNow)
    {
        patch.ApplyInto(this);

        _ = new Teacher(
            Id,
            FirstName,
            LastName,
            MiddleName,
            BirthDate,
            CreatedAt,
            utcNow);

        AddDomainEvent(
            new TeacherUpdatedDomainEvent
            {
                TeacherId = Id,
                UpdatedAt = utcNow
            });
    }
}
