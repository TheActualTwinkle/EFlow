namespace EFlow.Booking.Contracts.Teachers;

public sealed record TeacherView
{
    public required Guid Id { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public string? MiddleName { get; init; }

    public required DateOnly BirthDate { get; init; }

    public DateTime? CreatedAt { get; init; }
}
