using EFlow.Common.IntegrationEvents.Booking.Models;

namespace EFlow.Notifications.Templates.Notifications.Interfaces;

public interface IBookingNotificationTemplateService
{
    Task<(string Subject, string Body)> CreateBookingCreatedAsync(BookingRecordModel bookingRecord, CancellationToken cancellationToken = new());

    Task<(string Subject, string Body)> CreateBookingCancelledAsync(BookingRecordModel bookingRecord, CancellationToken cancellationToken = new());

    Task<(string Subject, string Body)> CreateSubmissionSlotCreatedAsync(SubmissionSlotModel submissionSlot, CancellationToken cancellationToken = new());

    Task<(string Subject, string Body)> CreateSubmissionSlotUpdatedAsync(
        SubmissionSlotModel oldSubmissionSlot,
        SubmissionSlotModel newSubmissionSlot,
        CancellationToken cancellationToken = new());

    Task<(string Subject, string Body)> CreateReminderAsync(
        SubmissionSlotModel submissionSlot,
        SubmissionRemindTimeModel submissionRemindTime,
        CancellationToken cancellationToken = new());
}
