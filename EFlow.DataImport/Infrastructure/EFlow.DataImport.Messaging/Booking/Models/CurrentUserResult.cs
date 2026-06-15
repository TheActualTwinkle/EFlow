using System.Net;

namespace EFlow.DataImport.Messaging.Booking.Models;

public sealed record CurrentUserResult
{
    public required HttpStatusCode StatusCode { get; init; }

    public string? Body { get; init; }

    public CurrentUserView? User { get; init; }
}
