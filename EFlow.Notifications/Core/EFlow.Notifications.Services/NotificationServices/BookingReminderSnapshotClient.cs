using System.Net.Http.Json;
using EFlow.Notifications.Services.NotificationServices.Models;

namespace EFlow.Notifications.Services.NotificationServices;

public sealed class BookingReminderSnapshotClient(HttpClient httpClient)
{
    public async Task<SubmissionSlotReminderSnapshot[]> GetReminderSnapshotAsync(CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync("api/submission-slots/reminder-snapshot", cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SubmissionSlotReminderSnapshot[]>(cancellationToken)
            ?? [];
    }
}
