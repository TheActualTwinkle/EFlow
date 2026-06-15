namespace EFlow.Notifications.Messaging.Booking.Settings;

public sealed record BookingReminderSettings
{
    public const string SectionName = "BookingReminder";

    public required Uri BookingApiBaseUrl { get; init; }

    public required TimeSpan PollInterval { get; init; }
}