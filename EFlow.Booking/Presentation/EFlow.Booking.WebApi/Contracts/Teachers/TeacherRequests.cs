namespace EFlow.Booking.WebApi.Contracts.Teachers;

public record CreateTeacherRequest
{
    public required string UserName { get; init; }

    public required string Password { get; init; }

    public required string FirstName { get; init; }

    public string? MiddleName { get; init; }

    public required string LastName { get; init; }

    public required DateOnly BirthDate { get; init; }
}
