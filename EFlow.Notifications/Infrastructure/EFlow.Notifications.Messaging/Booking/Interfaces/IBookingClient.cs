using EFlow.Notifications.Messaging.Booking.Models;

namespace EFlow.Notifications.Messaging.Booking.Interfaces;

public interface IBookingClient
{
    public Task<SubmissionSlotReminderSnapshot[]> GetReminderSnapshotAsync(CancellationToken cancellationToken);
}