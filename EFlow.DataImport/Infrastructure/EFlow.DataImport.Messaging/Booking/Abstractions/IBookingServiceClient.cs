using EFlow.DataImport.Messaging.Booking.Models;

namespace EFlow.DataImport.Messaging.Booking.Abstractions;

public interface IBookingServiceClient
{
    Task<CurrentUserResult> GetCurrentUserAsync(
        string? authorizationHeader,
        string? cookieHeader,
        CancellationToken cancellationToken);
}
