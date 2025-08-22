namespace EFlow.Common.Messaging.Producers;

public interface ICommitLogProducer<TKey, TValue>
{
    public Task ProduceAsync(
        string topic,
        TKey key,
        TValue value,
        CancellationToken cancellationToken = new());
}