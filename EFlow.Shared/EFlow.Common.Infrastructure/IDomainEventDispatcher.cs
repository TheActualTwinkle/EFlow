namespace EFlow.Common.Infrastructure;

public interface IDomainEventDispatcher
{
    Task DispatchEventsAsync(CancellationToken cancellationToken = new());
}