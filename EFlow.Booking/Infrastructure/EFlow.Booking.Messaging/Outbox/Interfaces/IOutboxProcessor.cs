namespace EFlow.Booking.Messaging.Outbox.Interfaces;

public interface IOutboxProcessor
{
    public Task ProcessPendingAsync(int batchSize, CancellationToken cancellationToken = new());

    public Task DeleteProcessedAsync(TimeSpan deleteAfter, CancellationToken cancellationToken = new());
}