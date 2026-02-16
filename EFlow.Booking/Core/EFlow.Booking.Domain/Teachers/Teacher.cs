using EFlow.Booking.Domain.Common.BusinessRules;
using EFlow.Booking.Domain.Teachers.Events;
using EFlow.Booking.Domain.Teachers.Rules;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Teachers;

public sealed class Teacher : Entity, IAggreagateRoot
{
    internal TeacherId Id { get; }

    internal string FirstName { get; private set; }

    internal string LastName { get; private set; }

    internal string? MiddleName { get; private set; }

    internal DateOnly BirthDate { get; private set; }

    internal DateTime CreatedAt { get; private set; }

    internal Identity? Identity { get; private set; }

    private Teacher(
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

        Id = new TeacherId(Guid.CreateVersion7());
        FirstName = trimmedFirstName;
        LastName = trimmedLastName;
        MiddleName = trimmedMiddleName;
        BirthDate = birthDate;
        CreatedAt = createdAt;

        AddDomainEvent(
            new TeacherCreatedDomainEvent
            {
                TeacherId = Id,
                CreatedAt = createdAt
            });
    }

    public static Teacher Create(
        string firstName,
        string lastName,
        string? middleName,
        DateOnly birthDate,
        DateTime createdAt,
        DateTime now) =>
        new(firstName, lastName, middleName, birthDate, createdAt, now);

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
        if (patch.NewFirstName is not null)
        {
            var trimmedFirstName = patch.NewFirstName.Trim();
            ThrowIfBroken(new TeacherFirstNameMustBeProperLengthRule(trimmedFirstName));

            FirstName = trimmedFirstName;
        }
        
        if (patch.NewLastName is not null)
        {
            var trimmedLastName = patch.NewLastName.Trim();
            ThrowIfBroken(new TeacherLastNameMustBeProperLengthRule(trimmedLastName));

            LastName = trimmedLastName;
        }
        
        if (patch.NewMiddleName is not null)
        {
            var trimmedMiddleName = patch.NewMiddleName.Trim();
            ThrowIfBroken(new TeacherMiddleNameMustBeProperLengthRule(trimmedMiddleName));

            MiddleName = trimmedMiddleName;
        }
        
        if (patch.NewBirthDate is not null)
        {
            ThrowIfBroken(new TeacherBirthDateMustBeInPastRule(patch.NewBirthDate.Value, DateOnly.FromDateTime(utcNow)));

            BirthDate = patch.NewBirthDate.Value;
        }

        AddDomainEvent(
            new TeacherUpdatedDomainEvent
            {
                TeacherId = Id,
                UpdatedAt = utcNow
            });
    }
}