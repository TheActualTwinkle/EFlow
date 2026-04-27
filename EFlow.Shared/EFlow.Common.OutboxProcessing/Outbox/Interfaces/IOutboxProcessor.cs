namespace EFlow.Common.OutboxProcessing.Outbox.Interfaces;

public interface IOutboxProcessor
{
    public Task ProcessPendingAsync(int batchSize, CancellationToken cancellationToken = new());

    public Task DeleteProcessedAsync(TimeSpan deleteAfter, CancellationToken cancellationToken = new());
}