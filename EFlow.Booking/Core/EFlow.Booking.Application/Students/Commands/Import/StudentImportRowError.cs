namespace EFlow.Booking.Application.Students.Commands.Import;

public sealed record StudentImportRowError
{
    public required int RowNumber { get; init; }

    public required string Message { get; init; }
}
