namespace EFlow.Booking.WebApi.Contracts.Students;

public record UpdateStudentRequest
{
    public Guid? GroupId { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public string? MiddleName { get; init; }

    public DateOnly? BirthDate { get; init; }
}
