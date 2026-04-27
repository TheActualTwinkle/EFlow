using System.Diagnostics.CodeAnalysis;
using EFlow.Common.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace EFlow.Common.Infrastructure;

public sealed class DomainEventDispatcher(
    DbContext context,
    IPublisher publisher,
    ILogger<DomainEventDispatcher> logger) : IDomainEventDispatcher
{
    private static readonly Lazy<IReadOnlyDictionary<Type, IReadOnlyList<Type>>> NotificationMappings = new(BuildNotificationMappings);

    public async Task DispatchEventsAsync(CancellationToken cancellationToken = new())
    {
        var domainEvents = context.ChangeTracker
            .Entries<Entity>()
            .SelectMany(entry => entry.Entity.DequeueDomainEvents())
            .ToList();

        if (domainEvents.Count == 0)
        {
            logger.LogDebug("No domain events found to dispatch");
            return;
        }

        logger.LogDebug("Dispatching {DomainEventCount} domain events", domainEvents.Count);

        foreach (var domainEvent in domainEvents)
        {
            var domainEventType = domainEvent.GetType();

            if (!NotificationMappings.Value.TryGetValue(domainEventType, out var notificationTypes) || notificationTypes.Count == 0)
            {
                logger.LogInformation(
                    "No notification mapping found for domain event {DomainEventType} with id {DomainEventId}. Event is skipped",
                    domainEventType.FullName,
                    domainEvent.Id);

                continue;
            }

            foreach (var notificationType in notificationTypes)
            {
                if (!TryCreateNotification(notificationType, domainEvent, out var notification))
                {
                    logger.LogError(
                        "Unable to create notification {NotificationType} for domain event {DomainEventType} with id {DomainEventId}. Notification is skipped",
                        notificationType.FullName,
                        domainEventType.FullName,
                        domainEvent.Id);

                    continue;
                }

                logger.LogInformation(
                    "Publishing notification {NotificationType} for domain event {DomainEventType} with id {DomainEventId}",
                    notificationType.FullName,
                    domainEventType.FullName,
                    domainEvent.Id);

                await publisher.Publish(notification, cancellationToken);
            }
        }
    }

    private static bool TryCreateNotification(
        Type notificationType,
        IDomainEvent domainEvent,
        [NotNullWhen(true)] out INotification? notification)
    {
        var domainEventType = domainEvent.GetType();

        var domainNotificationInterface = notificationType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType &&
                                 i.GetGenericTypeDefinition().Name == $"{nameof(IDomainNotification<>)}`1" &&
                                 i.GetGenericArguments()[0].IsAssignableFrom(domainEventType));

        notification = null;
        
        if (domainNotificationInterface is null)
            return false;

        var parameterlessConstructor = notificationType.GetConstructor(Type.EmptyTypes);
        
        if (parameterlessConstructor is null)
            return false;

        object notificationInstance;
        
        try
        {
            notificationInstance = parameterlessConstructor.Invoke(null);

            var prop = domainNotificationInterface.GetProperty(nameof(IDomainNotification<>.DomainEvent));
            
            if (prop is null || !prop.CanWrite)
                return false;

            prop.SetValue(notificationInstance, domainEvent);
        }
        catch
        {
            return false;
        }
        
        notification = notificationInstance as INotification;

        return notification is not null;
    }

    private static IReadOnlyDictionary<Type, IReadOnlyList<Type>> BuildNotificationMappings()
    {
        var mapping = new Dictionary<Type, List<Type>>();

        var notificationTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(GetLoadableTypes)
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .ToArray();

        foreach (var notificationType in notificationTypes)
        {
            var domainNotificationInterfaces = notificationType.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition().Name == "IDomainNotification`1");

            foreach (var iface in domainNotificationInterfaces)
            {
                var domainEventType = iface.GetGenericArguments()[0];
                if (!mapping.TryGetValue(domainEventType, out var mappedNotifications))
                {
                    mappedNotifications = [];
                    mapping[domainEventType] = mappedNotifications;
                }
                mappedNotifications.Add(notificationType);
            }
        }

        return mapping.ToDictionary(
            entry => entry.Key,
            entry => (IReadOnlyList<Type>)entry.Value
                .OrderBy(type => type.FullName)
                .ToArray());
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(type => type is not null)!;
        }
    }
}