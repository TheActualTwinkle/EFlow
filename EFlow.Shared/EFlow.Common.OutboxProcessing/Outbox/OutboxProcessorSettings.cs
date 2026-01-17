namespace EFlow.Booking.Messaging.Outbox;

public record OutboxProcessorSettings
{
    public required int BatchSize { get; init; }

    public required string ProcessIntervalCron { get; init; }

    public required string DeleteIntervalCron { get; init; }

    public required TimeSpan DeleteAfter { get; init; }
}