using MemoryPack;

namespace EFlow.Common.Messaging.DeadLetter;

[MemoryPackable]
public sealed partial record DeadLetterMessage
{
    public required string SourceTopic { get; init; }

    public required byte[] Key { get; init; }

    public required byte[] Payload { get; init; }

    public required string Exception { get; init; }

    public required DateTime FailedAt { get; init; }

    public required bool Retryable { get; init; }

    public required string ConsumerGroup { get; init; }

    public required int Attempt { get; init; }

    public required int MaxAttempts { get; init; }
}
