using EFlow.Booking.Domain;
using EFlow.Booking.Domain.Groups;

namespace EFlow.Common.Domain.Students;

public sealed class Student : Entity
{
    public required Guid Id { get; init; }

    public required Guid GroupId { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public string? MiddleName { get; init; }

    public required DateOnly BirthDate { get; init; }

    public required DateTime CreatedAt { get; init; }

    public Identity? Identity { get; init; }

    public Group? Group { get; init; }
}