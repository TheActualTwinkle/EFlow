namespace EFlow.Services.Outbox.Interfaces;

public interface IOutboxProcessor
{
    public Task ProcessPendingAsync(int batchSize, CancellationToken cancellationToken = new());
}