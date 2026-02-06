using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Teachers;

public sealed class Teacher : Entity
{
    public required Guid Id { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public string? MiddleName { get; init; }

    public required DateOnly BirthDate { get; init; }

    public required DateTime CreatedAt { get; init; }

    public Identity? Identity { get; init; }
}