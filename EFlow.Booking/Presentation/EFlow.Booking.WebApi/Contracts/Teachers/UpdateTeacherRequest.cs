namespace EFlow.Booking.WebApi.Contracts.Teachers;

public record UpdateTeacherRequest
{
    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public string? MiddleName { get; init; }

    public DateOnly? BirthDate { get; init; }
}
