namespace EFlow.Common.Messaging.DeadLetter;

public sealed record DeadLetterQueueProducerSettings
{
    public required string ConsumerGroup { get; init; }
    
    public required int MaxAttempts { get; init; }
    
    public required string DeadLetterTopic { get; init; }
}
