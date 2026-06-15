using EFlow.DataImport.Messaging.Booking.Models;

namespace EFlow.DataImport.Messaging.Booking.Contracts;

internal sealed record ImportStudentsRequest
{
    public required IReadOnlyList<BookingImportedStudent> Students { get; init; }
}
