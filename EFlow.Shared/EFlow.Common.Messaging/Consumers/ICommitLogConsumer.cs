namespace EFlow.Common.Messaging.Consumers;

public interface ICommitLogConsumer<TKey, out TValue>
{
    public Task StartAsync(
        string topic,
        Func<TValue, CancellationToken, ValueTask<bool>> handler,
        CancellationToken cancellationToken = new());
    
    public Task StopAsync(CancellationToken cancellationToken = new());
}