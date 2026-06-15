using EFlow.Booking.Application.Students.Commands.Import;

namespace EFlow.Booking.WebApi.Contracts.Students;

public sealed record ImportStudentsRequest
{
    public required IReadOnlyList<ImportedStudent> Students { get; init; }
}
