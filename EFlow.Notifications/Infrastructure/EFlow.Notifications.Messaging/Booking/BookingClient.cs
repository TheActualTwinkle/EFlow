using System.Net.Http.Json;
using EFlow.Notifications.Messaging.Booking.Interfaces;
using EFlow.Notifications.Messaging.Booking.Models;

namespace EFlow.Notifications.Messaging.Booking;

public sealed class BookingClient(HttpClient httpClient) : IBookingClient
{
    public async Task<IEnumerable<SubmissionSlotReminderSnapshot>> GetReminderSnapshotAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync("api/internal/submission-slots/reminder-snapshot", cancellationToken);
        
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<IEnumerable<SubmissionSlotReminderSnapshot>>(cancellationToken) ?? 
               throw new InvalidOperationException("Unable to get reminder snapshot.");
    }
}
