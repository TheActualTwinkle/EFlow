namespace EFlow.Notifications.Services.NotificationServices;

public sealed record BookingReminderOptions
{
    public const string SectionName = "BookingReminder";

    public required string BookingApiBaseUrl { get; init; }

    public required TimeSpan PollInterval { get; init; }
}
