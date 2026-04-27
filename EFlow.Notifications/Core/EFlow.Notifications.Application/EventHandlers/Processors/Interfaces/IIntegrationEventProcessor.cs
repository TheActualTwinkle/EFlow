namespace EFlow.Notifications.Application.EventHandlers.Processors.Interfaces;

public interface IIntegrationEventProcessor<in TEvent>
{
    Task ProcessAsync(TEvent @event, CancellationToken cancellationToken = new());
}