using System.Net;

namespace EFlow.DataImport.Messaging.Booking.Models;

public sealed record BookingStudentImportResult
{
    public required HttpStatusCode StatusCode { get; init; }

    public string? Body { get; init; }

    public string? ContentType { get; init; }
}
