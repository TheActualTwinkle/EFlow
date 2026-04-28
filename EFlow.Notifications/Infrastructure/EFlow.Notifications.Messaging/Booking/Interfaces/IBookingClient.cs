using EFlow.Notifications.Messaging.Booking.Models;

namespace EFlow.Notifications.Messaging.Booking.Interfaces;

public interface IBookingClient
{
    public Task<IEnumerable<SubmissionSlotReminderSnapshot>> GetReminderSnapshotAsync(CancellationToken cancellationToken);
}