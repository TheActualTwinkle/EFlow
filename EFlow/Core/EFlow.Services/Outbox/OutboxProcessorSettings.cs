namespace EFlow.Services.Outbox;

public record OutboxProcessorSettings
{
    public required int BatchSize { get; init; }

    public required TimeSpan ProcessInterval { get; init; }
    
    public required TimeSpan DeleteAfter { get; init; }
}