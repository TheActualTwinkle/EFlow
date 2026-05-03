namespace EFlow.Notifications.Messaging.Booking.Settings;

public sealed record BookingClientJwtSettings
{
    public const string SectionName = "Jwt";

    public required string Key { get; init; }

    public required string Issuer { get; init; }

    public required string Audience { get; init; }

    public int ExpireMinutes { get; init; } = 5;
}
