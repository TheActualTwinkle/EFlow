using EFlow.DataImport.Messaging.Booking.Models;

namespace EFlow.DataImport.Messaging.Booking.Abstractions;

public interface IBookingCurrentUserClient
{
    Task<CurrentUserResult> GetCurrentUserAsync(
        string? authorizationHeader,
        string? cookieHeader,
        CancellationToken cancellationToken);
}
