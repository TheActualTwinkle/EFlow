using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Teachers;

public sealed class Teacher : Entity, IAggreagateRoot
{
    public TeacherId Id { get; private set; }

    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string? MiddleName { get; private set; }

    public DateOnly BirthDate { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Identity? Identity { get; private set; }

    private Teacher(
        string firstName,
        string lastName,
        string? middleName,
        DateOnly birthDate,
        DateTime createdAt,
        DateTime now)
    {
    }

    public static Teacher Create(
        string firstName,
        string lastName,
        string? middleName,
        DateOnly birthDate,
        DateTime createdAt,
        DateTime now) =>
        new(firstName, lastName, middleName, birthDate, createdAt, now);
}