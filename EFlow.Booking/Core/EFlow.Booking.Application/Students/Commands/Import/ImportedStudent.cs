namespace EFlow.Booking.Application.Students.Commands.Import;

public sealed record ImportedStudent
{
    public required int RowNumber { get; init; }

    public required string UserName { get; init; }

    public required string Password { get; init; }

    public required string Email { get; init; }

    public required string FirstName { get; init; }

    public string? MiddleName { get; init; }

    public required string LastName { get; init; }

    public required DateOnly BirthDate { get; init; }
}
